// The one place that turns "which system is this?" into "which driver?". It goes through
// PlatformFactory, so the choice is a value made at runtime from a probe a test can lie to -
// and all three branches are covered from whichever machine runs the suite.

using GEngine.Core.Contracts;
using GEngine.Core.Platform;

namespace GEngine.Rendering;

/// <summary>Builds the console driver for the running system.</summary>
public static class ConsoleDriverFactory
{
    /// <summary>Creates the driver matching the platform a probe reports.</summary>
    /// <param name="probe">Says which platform this is.</param>
    /// <param name="logger">Where the driver explains what it managed to turn on.</param>
    /// <returns>The driver. The caller owns it and must dispose it.</returns>
    public static IConsoleDriver Create(IPlatformProbe probe, ILogger logger)
    {
        var choices = new PlatformChoices<IConsoleDriver>(
            () => new WindowsConsoleDriver(logger),
            () => new MacOsConsoleDriver(logger),
            () => new LinuxConsoleDriver(logger));

        return PlatformFactory.Create(probe, choices);
    }
}
