// The real probe. OperatingSystem.IsWindows() and friends are the runtime checks the
// compiler also understands, so platform-guarded APIs stay analysable without a single
// #if in the repository.

using System;
using System.Runtime.InteropServices;

namespace GEngine.Core.Platform;

/// <summary>Asks the runtime which operating system this is.</summary>
public sealed class SystemPlatformProbe : IPlatformProbe
{
    /// <summary>The shared instance. The answer cannot change while the process runs.</summary>
    public static SystemPlatformProbe Instance { get; } = new();

    /// <inheritdoc/>
    public PlatformKind Kind => Detect();

    /// <inheritdoc/>
    public string Description => RuntimeInformation.OSDescription + " / " + RuntimeInformation.OSArchitecture;

    private static PlatformKind Detect()
    {
        if (OperatingSystem.IsWindows())
        {
            return PlatformKind.Windows;
        }

        if (OperatingSystem.IsMacOS())
        {
            return PlatformKind.MacOs;
        }

        return OperatingSystem.IsLinux() ? PlatformKind.Linux : PlatformKind.Unknown;
    }
}
