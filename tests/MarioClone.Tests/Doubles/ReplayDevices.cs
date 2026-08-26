// The two ways the same recorded run reaches the game: as keys on a keyboard, and as bytes
// from a DualSense. Everything above the backend - the router, the action state, the player -
// is identical, which is exactly what the replay test is there to prove.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Keyboard;
using GEngine.Input.Scripted;

namespace MarioClone.Tests.Doubles;

/// <summary>Hands out the keys a script's actions are bound to, one frame at a time.</summary>
internal sealed class ScriptedKeyReader : IKeyReader
{
    private readonly Queue<ConsoleKey> _pending = new();
    private readonly InputScript _script;
    private readonly InputMap _map;

    public ScriptedKeyReader(InputScript script, InputMap map)
    {
        _script = script;
        _map = map;
    }

    public void LoadFrame(int frame)
    {
        foreach (InputAction action in InputActions.All)
        {
            if (_script.IsHeld(frame, action) && _map.KeysFor(action).Count > 0)
            {
                _pending.Enqueue(_map.KeysFor(action)[0]);
            }
        }
    }

    public bool TryReadKey(out ConsoleKey key)
    {
        if (_pending.Count == 0)
        {
            key = default;
            return false;
        }

        key = _pending.Dequeue();
        return true;
    }
}

/// <summary>Turns a script into the DualSense reports that would have produced it.</summary>
internal static class ReplayReports
{
    private const int LeftStickX = 1;
    private const int FaceAndHat = 8;
    private const byte Centre = 0x80;
    private const byte HatNeutral = 0x08;
    private const byte Cross = 0x20;
    private const byte Square = 0x10;

    public static FakeHidDevice Device(InputScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        var device = new FakeHidDevice();
        for (int frame = 0; frame < script.FrameCount; frame++)
        {
            device.Queue(Report(script, frame));
        }

        return device;
    }

    public static HidDeviceInfo Controller { get; } =
        new(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId, "fake://replay", "DualSense");

    private static byte[] Report(InputScript script, int frame)
    {
        byte[] report = new byte[KnownDevices.UsbReportLength];
        report[0] = DualSenseReport.UsbReportId;
        report[LeftStickX] = script.IsHeld(frame, InputAction.MoveRight) ? (byte)0xFF : Centre;
        report[LeftStickX + 1] = Centre;
        report[LeftStickX + 2] = Centre;
        report[LeftStickX + 3] = Centre;

        byte face = HatNeutral;
        face |= script.IsHeld(frame, InputAction.Jump) ? Cross : (byte)0x00;
        face |= script.IsHeld(frame, InputAction.Run) ? Square : (byte)0x00;
        report[FaceAndHat] = face;
        return report;
    }
}
