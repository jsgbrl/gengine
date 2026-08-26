#:project ../src/GEngine.Core/GEngine.Core.csproj
#:project ../src/GEngine.Input/GEngine.Input.csproj

// 04 - what the controller actually sends.
//
// Run it:  dotnet run examples/04-gamepad-probe.cs
//
// This is the tool that turns the byte table in the documentation from a hypothesis into a
// measurement. It lists every HID device the system will admit to, opens a DualSense if one
// is plugged in, and prints every report that differs from the last one: the raw bytes in
// hexadecimal, which bytes moved, and what the decoder made of them.
//
// How to verify the table with it:
//
//   1. run it and leave the controller alone - the first line is the neutral report
//   2. press one button, let go, and look at which byte moved and which bit inside it
//   3. compare with the table in docs/input-and-gamepad.md
//   4. if a bit disagrees, fix the constant in DualSenseReport, replace the matching file in
//      tests/GEngine.Input.Tests/fixtures/dualsense, and write down which model you measured
//
// Press Esc, or Ctrl+C, or wait for the report limit.
//
// Pass --capture to print one whole report as fixture-ready hexadecimal instead, ready to be
// pasted straight into tests/GEngine.Input.Tests/fixtures/dualsense.

using System;
using System.Collections.Generic;
using GEngine.Core.Platform;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;

const int MaximumReports = 4000;
const int DecodedBytes = DualSenseReport.LastDecodedOffset + 1;
const int SequenceCounterByte = 7;
const int FirstAxisByte = 1;
const int LastAxisByte = 6;
const int AxisNoiseFloor = 4;

IPlatformProbe platform = SystemPlatformProbe.Instance;
IHidBackend backend = HidBackendFactory.Create(platform);

Console.WriteLine("gengine example 04 - gamepad probe");
Console.WriteLine($"platform:    {platform.Kind} ({platform.Description})");
Console.WriteLine($"hid backend: {backend.Name}");
Console.WriteLine();

IReadOnlyList<HidDeviceInfo> devices = backend.Enumerate();
Console.WriteLine($"{devices.Count} HID device(s):");
foreach (HidDeviceInfo device in devices)
{
    string mark = KnownDevices.IsDualSense(device) ? " <- a controller gengine can decode" : string.Empty;
    Console.WriteLine($"  {device}{mark}");
}

Console.WriteLine();
if (!KnownDevices.TryFindDualSense(devices, out HidDeviceInfo controller))
{
    Console.WriteLine("No DualSense found. Plug one in with a USB cable and run this again.");
    Console.WriteLine("Bluetooth is not enough: the report is laid out differently and gengine refuses it.");
    Console.WriteLine("On Linux, /dev/hidraw* is usually root-only - see docs/platforms.md for the udev rule.");
    return 0;
}

IHidDevice? opened = backend.Open(controller);
if (opened is null)
{
    Console.WriteLine($"Found {KnownDevices.NameOf(controller)} at {controller.Path} but could not open it.");
    Console.WriteLine("On Linux this is almost always the udev rule; on Windows, another program holding it;");
    Console.WriteLine("on macOS, Input Monitoring permission for the terminal. docs/platforms.md has all three.");
    return 1;
}

using var reader = new HidReportReader(opened);
if (Array.IndexOf(args, "--capture") >= 0)
{
    return Capture(reader, KnownDevices.NameOf(controller));
}

Console.WriteLine($"reading {KnownDevices.NameOf(controller)} at {controller.Path}");
Console.WriteLine("press buttons; only reports that differ from the last one are printed");
Console.WriteLine();
Console.WriteLine("  bytes 0..10                                       decoded");
Console.WriteLine("  ----------------------------------------------    -------");

byte[] current = new byte[KnownDevices.UsbReportLength];
byte[] previous = new byte[KnownDevices.UsbReportLength];
int printed = 0;

for (int report = 0; report < MaximumReports; report++)
{
    if (IsEscapePressed() || !reader.PumpOnce() || !reader.TryTakeLatest(current))
    {
        break;
    }

    if (printed > 0 && SameApartFromNoise(current, previous))
    {
        continue;
    }

    Print(current, previous, printed);
    Array.Copy(current, previous, current.Length);
    printed++;
}

Console.WriteLine();
Console.WriteLine($"{reader.ReportCount} reports read, {printed} of them different.");
Console.WriteLine("Transport:  " + DualSenseReport.TransportOf(current));
Console.WriteLine("Read size:  " + reader.LastReportLength + " bytes");
return 0;

// One whole report, formatted the way the fixtures are: sixteen bytes to a line, with a
// header saying what it is and where it came from.
static int Capture(HidReportReader reader, string model)
{
    byte[] report = new byte[KnownDevices.UsbReportLength];
    if (!reader.PumpOnce() || !reader.TryTakeLatest(report))
    {
        Console.WriteLine("# the controller sent nothing");
        return 1;
    }

    Console.WriteLine("# DualSense USB input report, " + reader.LastReportLength + " bytes.");
    Console.WriteLine("#");
    Console.WriteLine("# CAPTURED FROM HARDWARE with examples/04-gamepad-probe.cs --capture");
    Console.WriteLine("# model: " + model);
    Console.WriteLine("#");
    Console.WriteLine("# describe here what was being held when this was taken");
    for (int row = 0; row < report.Length; row += 16)
    {
        Console.WriteLine(HidReportHex.ToText(report.AsSpan(row, Math.Min(16, report.Length - row))));
    }

    return 0;
}

// Two kinds of change are not news. The sequence counter changes on every report by design.
// And a stick at rest wobbles by a unit or two for ever - which is worth seeing once, and is
// the entire reason DeadZone exists, but is not worth a line of output every four
// milliseconds. So an axis has to move by more than the noise floor to count.
static bool SameApartFromNoise(byte[] current, byte[] previous)
{
    for (int index = 0; index < DecodedBytes; index++)
    {
        if (index == SequenceCounterByte)
        {
            continue;
        }

        int difference = Math.Abs(current[index] - previous[index]);
        int allowed = index is >= FirstAxisByte and <= LastAxisByte ? AxisNoiseFloor : 0;
        if (difference > allowed)
        {
            return false;
        }
    }

    return true;
}

static void Print(byte[] current, byte[] previous, int printed)
{
    Console.Write("  " + HidReportHex.ToText(current.AsSpan(0, DecodedBytes)));
    Console.Write("    ");
    if (DualSenseReport.TryDecode(current, out GamepadState state))
    {
        Console.Write(state);
    }
    else
    {
        Console.Write("not a USB input report");
    }

    Console.WriteLine();
    if (printed > 0)
    {
        Console.WriteLine("  " + Changes(current, previous));
    }
}

// A caret under every byte that moved, so the byte a button lives in is visible at a glance.
static string Changes(byte[] current, byte[] previous)
{
    var marks = new System.Text.StringBuilder();
    for (int index = 0; index < DecodedBytes; index++)
    {
        marks.Append(current[index] == previous[index] ? "   " : " ^^");
    }

    return marks.ToString().TrimEnd();
}

static bool IsEscapePressed()
{
    if (Console.IsInputRedirected)
    {
        return false;
    }

    return Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape;
}
