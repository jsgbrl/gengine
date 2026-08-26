// A DualSense as an input source: find it, open it, read it on its own thread, decode what it
// sends, and notice when it goes away.
//
// Hot-plug is the whole reason this class exists rather than a plain "open the device once".
// A controller unplugged mid-level makes the reader thread's next read return nothing; the
// backend goes quiet, the router simply stops hearing from it, and the game pauses with a
// message. Plug it back in and the rescan finds it again on the next second.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;
using GEngine.Input.Hid;

namespace GEngine.Input.Gamepad;

/// <summary>An input backend driven by a DualSense over USB.</summary>
public sealed partial class DualSenseGamepad : IInputBackend
{
    /// <summary>How often to look for a controller while none is attached.</summary>
    public const float DefaultRescanSeconds = 1.0f;

    private readonly byte[] _report = new byte[KnownDevices.UsbReportLength];
    private readonly float[] _values = new float[InputActions.Count];
    private readonly IHidBackend _backend;
    private readonly ILogger _logger;
    private HidReportReader? _reader;
    private float _sinceRescanSeconds;
    private bool _hasWarnedAboutBluetooth;

    /// <summary>Creates the backend. It does not look for a controller until the first poll.</summary>
    /// <param name="backend">Where HID devices come from.</param>
    /// <param name="map">Which buttons ask for which actions.</param>
    /// <param name="logger">Where it explains what it found and what it lost.</param>
    public DualSenseGamepad(IHidBackend backend, GamepadMap map, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(backend);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(logger);
        _backend = backend;
        _logger = logger;
        Map = map;
    }

    /// <summary>Which buttons ask for which actions. Rebinding takes effect on the next poll.</summary>
    public GamepadMap Map { get; }

    /// <inheritdoc/>
    public string Name { get; private set; } = "DualSense";

    /// <inheritdoc/>
    public bool IsConnected => _reader is not null && _reader.IsConnected;

    /// <summary>How the attached controller is connected.</summary>
    public HidTransport Transport { get; private set; } = HidTransport.Unknown;

    /// <summary>The decoded controller state, after dead zones.</summary>
    public GamepadState State { get; private set; } = GamepadState.Neutral;

    /// <summary>How far the sticks must move before they count as moved.</summary>
    public float DeadZoneRadius { get; init; } = DeadZone.DefaultRadius;

    /// <summary>How often to look for a controller while none is attached.</summary>
    public float RescanSeconds { get; init; } = DefaultRescanSeconds;

    /// <summary>Where the blocking read happens. A game leaves this alone.</summary>
    public GamepadReading Reading { get; init; } = GamepadReading.OnItsOwnThread;

    /// <summary>How many reports have arrived from the attached controller.</summary>
    public long ReportCount => _reader?.ReportCount ?? 0L;

    /// <inheritdoc/>
    public void Poll(float deltaSeconds)
    {
        if (_reader is null)
        {
            Rescan(deltaSeconds);
            return;
        }

        if (Reading == GamepadReading.WhenPolled)
        {
            _reader.PumpOnce();
        }

        if (!_reader.IsConnected)
        {
            Detach("the controller was unplugged");
            return;
        }

        if (_reader.TryTakeLatest(_report))
        {
            Decode();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Detach(string.Empty);

    private void Rescan(float deltaSeconds)
    {
        _sinceRescanSeconds += deltaSeconds;
        if (_sinceRescanSeconds < RescanSeconds)
        {
            return;
        }

        _sinceRescanSeconds = 0.0f;
        IReadOnlyList<HidDeviceInfo> devices = _backend.Enumerate();
        if (KnownDevices.TryFindDualSense(devices, out HidDeviceInfo found))
        {
            Attach(found);
        }
    }

    private void Attach(HidDeviceInfo info)
    {
        IHidDevice? device = _backend.Open(info);
        if (device is null)
        {
            _logger.Warning("found a " + KnownDevices.NameOf(info) + " but could not open it: " + info.Path);
            return;
        }

        Name = KnownDevices.NameOf(info);
        _reader = new HidReportReader(device);
        if (Reading == GamepadReading.OnItsOwnThread)
        {
            _reader.Start();
        }

        _logger.Info("using a " + Name + " at " + info.Path);
    }

    private void Detach(string reason)
    {
        _reader?.Dispose();
        _reader = null;
        Transport = HidTransport.Unknown;
        State = GamepadState.Neutral;
        Array.Clear(_values);
        if (reason.Length > 0)
        {
            _logger.Warning(reason);
        }
    }

    // Bluetooth is refused rather than decoded. The report identifier is different, the
    // fields sit at different offsets, and decoding one as the other gives a controller that
    // appears to be holding down buttons nobody is touching - which is far worse than saying
    // plainly that the cable is what works.
    private void Decode()
    {
        Transport = DualSenseReport.TransportOf(_report);
        if (Transport == HidTransport.Bluetooth)
        {
            WarnAboutBluetoothOnce();
            return;
        }

        if (DualSenseReport.TryDecode(_report, out GamepadState decoded))
        {
            State = ApplyDeadZones(decoded);
            FillActions();
        }
    }

    private void WarnAboutBluetoothOnce()
    {
        if (_hasWarnedAboutBluetooth)
        {
            return;
        }

        _hasWarnedAboutBluetooth = true;
        _logger.Warning("this DualSense is on Bluetooth, which gengine does not decode - plug in the USB cable");
        Detach(string.Empty);
    }
}
