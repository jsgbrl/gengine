// Signatures only. Three system libraries, all of them already loaded into every Windows
// process: setupapi enumerates devices, hid asks one what it is, and kernel32 opens and reads
// it. The logic that decides whether to call them lives in WindowsHidBackend.
//
// [DllImport] and not [LibraryImport]: the LibraryImport generator emits marshalling stubs
// that require AllowUnsafeBlocks, which rule 6 of the build prompt bans. SYSLIB1054 is turned
// off for this folder in .editorconfig with that reason written next to it.

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GEngine.Input.Hid.Interop.Windows;

internal static class NativeMethods
{
    internal const uint DeviceInterfacePresent = 0x00000012;
    internal const uint GenericRead = 0x80000000;
    internal const uint FileShareReadWrite = 0x00000003;
    internal const uint OpenExisting = 3;

    [DllImport("hid.dll")]
    internal static extern void HidD_GetHidGuid(out Guid hidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool HidD_GetAttributes(SafeFileHandle device, ref HiddAttributes attributes);

    // A char array rather than a StringBuilder: CA1838 is right that a StringBuilder costs an
    // extra copy each way, and here the caller wants the characters anyway.
    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool HidD_GetProductString(SafeFileHandle device, char[] buffer, int byteLength);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr SetupDiGetClassDevs(
        ref Guid classGuid,
        IntPtr enumerator,
        IntPtr parent,
        uint flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetupDiEnumDeviceInterfaces(
        IntPtr deviceInfoSet,
        IntPtr deviceInfoData,
        ref Guid interfaceClassGuid,
        uint memberIndex,
        ref SpDeviceInterfaceData interfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "SetupDiGetDeviceInterfaceDetailW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr deviceInfoSet,
        ref SpDeviceInterfaceData interfaceData,
        IntPtr detailData,
        int detailSize,
        out int requiredSize,
        IntPtr deviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateFileW")]
    internal static extern SafeFileHandle CreateFile(
        string fileName,
        uint access,
        uint share,
        IntPtr security,
        uint creation,
        uint flags,
        IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReadFile(
        SafeFileHandle device,
        byte[] buffer,
        int bytesToRead,
        out int bytesRead,
        IntPtr overlapped);

    [StructLayout(LayoutKind.Sequential)]
    internal struct HiddAttributes
    {
        internal int Size;
        internal ushort VendorId;
        internal ushort ProductId;
        internal ushort VersionNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpDeviceInterfaceData
    {
        internal int Size;
        internal Guid InterfaceClassGuid;
        internal int Flags;
        internal IntPtr Reserved;
    }
}
