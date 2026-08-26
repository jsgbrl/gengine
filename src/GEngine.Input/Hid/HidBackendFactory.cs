// The one place that turns "which system is this?" into "which HID backend?". Same Factory as
// the console driver next door, and for the same reason: the choice is a value made at
// runtime from a probe a test can lie to, so all three branches are covered from one machine.

using GEngine.Core.Platform;
using GEngine.Input.Hid.Interop.Linux;
using GEngine.Input.Hid.Interop.MacOs;
using GEngine.Input.Hid.Interop.Windows;

namespace GEngine.Input.Hid;

/// <summary>Builds the HID backend for the running system.</summary>
public static class HidBackendFactory
{
    /// <summary>Creates the backend matching the platform a probe reports.</summary>
    /// <param name="probe">Says which platform this is.</param>
    /// <returns>The backend.</returns>
    public static IHidBackend Create(IPlatformProbe probe)
    {
        var choices = new PlatformChoices<IHidBackend>(
            static () => new WindowsHidBackend(),
            static () => new MacOsHidBackend(),
            static () => new LinuxHidBackend());

        return PlatformFactory.Create(probe, choices);
    }
}
