// A probe that answers whatever it was told. This is what lets one machine test all three
// platform paths - and it is the reason PlatformFactory takes a probe instead of calling
// OperatingSystem.IsWindows() itself.

namespace GEngine.Core.Platform;

/// <summary>A probe that reports a platform chosen by the caller.</summary>
public sealed class FixedPlatformProbe : IPlatformProbe
{
    /// <summary>Creates a probe reporting a given platform.</summary>
    /// <param name="kind">The platform to report.</param>
    /// <param name="description">The description to report.</param>
    public FixedPlatformProbe(PlatformKind kind, string description = "fixed probe")
    {
        Kind = kind;
        Description = description;
    }

    /// <inheritdoc/>
    public PlatformKind Kind { get; }

    /// <inheritdoc/>
    public string Description { get; }
}
