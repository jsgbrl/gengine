// Factory. Six lines of dispatch, and the reason there is not a single #if in the
// repository: the choice of implementation is a value, made at runtime, from a probe that
// a test can lie to.

using System;

namespace GEngine.Core.Platform;

/// <summary>Picks the implementation matching the platform a probe reports.</summary>
public static class PlatformFactory
{
    /// <summary>Builds the implementation for the probed platform.</summary>
    /// <typeparam name="TImplementation">The abstraction being implemented.</typeparam>
    /// <param name="probe">Says which platform this is.</param>
    /// <param name="choices">One factory per platform.</param>
    /// <returns>The implementation for this platform.</returns>
    /// <exception cref="PlatformNotSupportedException">The probe reported an unknown system.</exception>
    public static TImplementation Create<TImplementation>(IPlatformProbe probe, PlatformChoices<TImplementation> choices)
    {
        ArgumentNullException.ThrowIfNull(probe);
        ArgumentNullException.ThrowIfNull(choices);
        return probe.Kind switch
        {
            PlatformKind.Windows => choices.Windows(),
            PlatformKind.MacOs => choices.MacOs(),
            PlatformKind.Linux => choices.Linux(),
            _ => throw new PlatformNotSupportedException(
                "gengine supports Windows, macOS and Linux; this system reports as " + probe.Description),
        };
    }
}
