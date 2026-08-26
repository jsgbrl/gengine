// The three systems, plus the honest fourth. Nothing in the engine branches on this enum
// directly: it is the key PlatformFactory looks an implementation up with.

namespace GEngine.Core.Platform;

/// <summary>Which operating system the process is running on.</summary>
public enum PlatformKind
{
    /// <summary>Not one of the three the engine knows about.</summary>
    Unknown,

    /// <summary>Microsoft Windows.</summary>
    Windows,

    /// <summary>Apple macOS.</summary>
    MacOs,

    /// <summary>Linux.</summary>
    Linux,
}
