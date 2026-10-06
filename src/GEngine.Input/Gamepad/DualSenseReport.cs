// Sixty-four bytes into a controller state.
//
// Every offset below is documented in the appendix of the build prompt and is a hypothesis
// until it has been measured against real hardware with examples/04-gamepad-probe.cs. Where
// this was written no DualSense was available: docs/input-and-gamepad.md says so, and the
// fixtures in the tests are marked as constructed from the table rather than captured.
//
// The one thing that is not a hypothesis is the axis convention, and it is a pleasant
// accident: the controller sends 0 for "stick pushed up", and this engine's Y axis grows
// downwards, so the raw byte converts to the engine's convention with no flip at all.

using System;
using GEngine.Core;
using GEngine.Input.Hid;

namespace GEngine.Input.Gamepad;

/// <summary>Decodes the USB input report of a DualSense.</summary>
public static class DualSenseReport
{
    /// <summary>The report identifier a DualSense sends over USB.</summary>
    public const byte UsbReportId = 0x01;

    /// <summary>The report identifier it sends over Bluetooth, which this engine refuses.</summary>
    public const byte BluetoothReportId = 0x31;

    /// <summary>How many bytes a USB report is.</summary>
    public const int UsbLength = 64;

    /// <summary>The last byte offset this decoder reads. Everything after it is out of scope.</summary>
    public const int LastDecodedOffset = 10;

    private const int LeftStickX = 1;
    private const int LeftStickY = 2;
    private const int RightStickX = 3;
    private const int RightStickY = 4;
    private const int LeftTriggerOffset = 5;
    private const int RightTriggerOffset = 6;
    private const int SequenceCounter = 7;
    private const int FaceAndHat = 8;
    private const int ShouldersAndMenus = 9;
    private const int SystemButtonsOffset = 10;

    /// <summary>How the controller sending a report is attached, judging by its report id.</summary>
    /// <param name="report">The bytes as the device sent them, including the report id.</param>
    /// <returns>USB, Bluetooth, or unknown.</returns>
    public static HidTransport TransportOf(ReadOnlySpan<byte> report)
    {
        if (report.Length == 0)
        {
            return HidTransport.Unknown;
        }

        return report[0] switch
        {
            UsbReportId => HidTransport.Usb,
            BluetoothReportId => HidTransport.Bluetooth,
            _ => HidTransport.Unknown,
        };
    }

    /// <summary>The sequence counter, which increments once per report the controller sends.</summary>
    /// <param name="report">The bytes as the device sent them.</param>
    /// <returns>The counter, or zero when the report is too short.</returns>
    public static byte SequenceOf(ReadOnlySpan<byte> report) =>
        report.Length > SequenceCounter ? report[SequenceCounter] : (byte)0;

    /// <summary>Decodes a USB report.</summary>
    /// <param name="report">The report, at least eleven bytes of it.</param>
    /// <param name="state">The decoded state, neutral when the report is not one.</param>
    /// <returns>False when the report is not a USB input report.</returns>
    public static bool TryDecode(ReadOnlySpan<byte> report, out GamepadState state)
    {
        if (TransportOf(report) != HidTransport.Usb || report.Length <= LastDecodedOffset)
        {
            state = GamepadState.Neutral;
            return false;
        }

        state = new GamepadState(
            Stick(report, LeftStickX, LeftStickY),
            Stick(report, RightStickX, RightStickY),
            new Vector2(report[LeftTriggerOffset] / 255.0f, report[RightTriggerOffset] / 255.0f),
            Buttons(report))
        {
            Hat = HatOf(report[FaceAndHat]),
        };

        return true;
    }

    /// <summary>Which way the D-pad is pointing, from the low half of the face byte.</summary>
    /// <param name="faceAndHat">The byte holding the hat and the four face buttons.</param>
    /// <returns>The direction. Anything above eight is read as neutral.</returns>
    public static HatDirection HatOf(byte faceAndHat)
    {
        int hat = faceAndHat & 0x0F;
        return hat <= (int)HatDirection.Neutral ? (HatDirection)hat : HatDirection.Neutral;
    }

    private static Vector2 Stick(ReadOnlySpan<byte> report, int xOffset, int yOffset) =>
        new(DeadZone.FromAxisByte(report[xOffset]), DeadZone.FromAxisByte(report[yOffset]));

    private static GamepadButtons Buttons(ReadOnlySpan<byte> report)
    {
        GamepadButtons buttons = FaceButtons(report[FaceAndHat]);
        buttons |= ShoulderButtons(report[ShouldersAndMenus]);
        buttons |= SystemButtons(report[SystemButtonsOffset]);
        return buttons;
    }

    private static GamepadButtons FaceButtons(byte raw)
    {
        GamepadButtons buttons = Bit(raw, 0x10, GamepadButtons.Square);
        buttons |= Bit(raw, 0x20, GamepadButtons.Cross);
        buttons |= Bit(raw, 0x40, GamepadButtons.Circle);
        buttons |= Bit(raw, 0x80, GamepadButtons.Triangle);
        return buttons;
    }

    private static GamepadButtons ShoulderButtons(byte raw)
    {
        GamepadButtons buttons = Bit(raw, 0x01, GamepadButtons.LeftShoulder);
        buttons |= Bit(raw, 0x02, GamepadButtons.RightShoulder);
        buttons |= Bit(raw, 0x04, GamepadButtons.LeftTrigger);
        buttons |= Bit(raw, 0x08, GamepadButtons.RightTrigger);
        buttons |= Bit(raw, 0x10, GamepadButtons.Create);
        buttons |= Bit(raw, 0x20, GamepadButtons.Options);
        buttons |= Bit(raw, 0x40, GamepadButtons.LeftStick);
        buttons |= Bit(raw, 0x80, GamepadButtons.RightStick);
        return buttons;
    }

    private static GamepadButtons SystemButtons(byte raw)
    {
        GamepadButtons buttons = Bit(raw, 0x01, GamepadButtons.PlayStation);
        buttons |= Bit(raw, 0x02, GamepadButtons.Touchpad);
        buttons |= Bit(raw, 0x04, GamepadButtons.Mute);
        return buttons;
    }

    private static GamepadButtons Bit(byte raw, byte mask, GamepadButtons button) =>
        (raw & mask) != 0 ? button : GamepadButtons.None;
}
