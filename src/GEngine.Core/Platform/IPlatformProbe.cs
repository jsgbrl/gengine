// Rule 4 of the build prompt bans conditional compilation: one binary runs everywhere and
// decides at runtime. This is the interface that decision goes through, and because it is
// an interface a test on a Windows laptop can ask the engine what it would do on macOS.

namespace GEngine.Core.Platform;

/// <summary>Reports which operating system the engine is running on.</summary>
public interface IPlatformProbe
{
    /// <summary>Which of the three systems this is.</summary>
    PlatformKind Kind { get; }

    /// <summary>A human-readable description, shown in diagnostics and on the title screen.</summary>
    string Description { get; }
}
