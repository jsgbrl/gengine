// One implementation per system, in one object. Writing them as factories rather than as
// instances matters: constructing the Windows console driver on Linux would fail, so
// nothing may be built until the probe has said which one is wanted.

using System;

namespace GEngine.Core.Platform;

/// <summary>The three implementations of one abstraction, one per operating system.</summary>
/// <typeparam name="TImplementation">The abstraction being implemented.</typeparam>
public sealed class PlatformChoices<TImplementation>
{
    /// <summary>Creates a set of choices.</summary>
    /// <param name="windows">Builds the Windows implementation.</param>
    /// <param name="macOs">Builds the macOS implementation.</param>
    /// <param name="linux">Builds the Linux implementation.</param>
    public PlatformChoices(
        Func<TImplementation> windows,
        Func<TImplementation> macOs,
        Func<TImplementation> linux)
    {
        ArgumentNullException.ThrowIfNull(windows);
        ArgumentNullException.ThrowIfNull(macOs);
        ArgumentNullException.ThrowIfNull(linux);
        Windows = windows;
        MacOs = macOs;
        Linux = linux;
    }

    /// <summary>Builds the Windows implementation.</summary>
    public Func<TImplementation> Windows { get; }

    /// <summary>Builds the macOS implementation.</summary>
    public Func<TImplementation> MacOs { get; }

    /// <summary>Builds the Linux implementation.</summary>
    public Func<TImplementation> Linux { get; }
}
