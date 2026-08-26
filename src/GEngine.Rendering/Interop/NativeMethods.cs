// Signatures only. Every one of them names a library Windows itself ships and has already
// loaded into the process; the logic that decides whether to call them lives in
// WindowsConsoleDriver, where it can be read and tested.
//
// [DllImport] and not [LibraryImport]: the source generator behind LibraryImport emits
// marshalling stubs that require AllowUnsafeBlocks, and rule 6 of the build prompt bans
// unsafe. The SYSLIB1054 suggestion is turned off for this folder in .editorconfig, with
// that reason written next to it.

using System;
using System.Runtime.InteropServices;

namespace GEngine.Rendering.Interop;

internal static class NativeMethods
{
    /// <summary>The standard output device, as GetStdHandle names it.</summary>
    internal const int StandardOutputHandle = -11;

    /// <summary>Console mode bit that makes the terminal interpret escape sequences.</summary>
    internal const uint EnableVirtualTerminalProcessing = 0x0004;

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr GetStdHandle(int standardHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetConsoleMode(IntPtr consoleHandle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetConsoleMode(IntPtr consoleHandle, uint mode);
}
