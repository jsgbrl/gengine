// Windows, where finding a device takes four calls and opening it takes one.
//
// SetupDiGetClassDevs asks for every device presenting the HID interface; SetupDiEnumDevice-
// Interfaces walks them; SetupDiGetDeviceInterfaceDetail turns each one into a path that
// looks like \\?\hid#vid_054c&pid_0ce6#... - which is then handed straight back to CreateFile
// without ever being parsed. HidD_GetAttributes on the open handle gives the vendor and
// product, which is the only way to tell a controller from a keyboard.
//
// The handle is opened with sharing on both read and write. A DualSense is very often already
// open in another process - Steam, DS4Windows - and asking for exclusive access there fails
// with a message nobody can act on. Sharing means both can read it.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GEngine.Input.Hid.Interop.Windows;

/// <summary>The HID backend for Windows, over setupapi and hid.</summary>
public sealed class WindowsHidBackend : IHidBackend
{
    private const int MaximumProductNameLength = 128;

    /// <inheritdoc/>
    public string Name => "windows hid";

    /// <inheritdoc/>
    public IReadOnlyList<HidDeviceInfo> Enumerate()
    {
        List<HidDeviceInfo> found = [];
        if (!OperatingSystem.IsWindows())
        {
            return found;
        }

        NativeMethods.HidD_GetHidGuid(out Guid hidGuid);
        IntPtr set = NativeMethods.SetupDiGetClassDevs(
            ref hidGuid,
            IntPtr.Zero,
            IntPtr.Zero,
            NativeMethods.DeviceInterfacePresent);

        if (set == IntPtr.Zero || set == new IntPtr(-1))
        {
            return found;
        }

        try
        {
            Walk(set, hidGuid, found);
        }
        finally
        {
            NativeMethods.SetupDiDestroyDeviceInfoList(set);
        }

        found.Sort(static (left, right) => string.CompareOrdinal(left.Path, right.Path));
        return found;
    }

    /// <inheritdoc/>
    public IHidDevice? Open(HidDeviceInfo device)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        SafeFileHandle handle = OpenHandle(device.Path);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            return null;
        }

        return new WindowsHidDevice(device.Path, handle);
    }

    private static SafeFileHandle OpenHandle(string path) => NativeMethods.CreateFile(
        path,
        NativeMethods.GenericRead,
        NativeMethods.FileShareReadWrite,
        IntPtr.Zero,
        NativeMethods.OpenExisting,
        0,
        IntPtr.Zero);

    private static void Walk(IntPtr set, Guid hidGuid, List<HidDeviceInfo> found)
    {
        for (uint index = 0; ; index++)
        {
            var data = new NativeMethods.SpDeviceInterfaceData
            {
                Size = Marshal.SizeOf<NativeMethods.SpDeviceInterfaceData>(),
                InterfaceClassGuid = hidGuid,
                Flags = 0,
                Reserved = IntPtr.Zero,
            };

            if (!NativeMethods.SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref hidGuid, index, ref data))
            {
                return;
            }

            AddIfReadable(set, ref data, found);
        }
    }

    private static void AddIfReadable(IntPtr set, ref NativeMethods.SpDeviceInterfaceData data, List<HidDeviceInfo> found)
    {
        string path = ReadPath(set, ref data);
        if (path.Length == 0)
        {
            return;
        }

        using SafeFileHandle handle = OpenHandle(path);
        if (handle.IsInvalid || !TryReadAttributes(handle, out int vendorId, out int productId))
        {
            return;
        }

        found.Add(new HidDeviceInfo(vendorId, productId, path, ReadProductName(handle)));
    }

    // SP_DEVICE_INTERFACE_DETAIL_DATA is a variable-length structure: four bytes of size and
    // then the path, inline. Asking with a null buffer reports how much room it needs; the
    // cbSize written back is the size of the fixed part, which is 8 on 64-bit Windows because
    // of alignment and 6 on 32-bit - not the size of the whole buffer, which is the mistake
    // this call is famous for.
    private static string ReadPath(IntPtr set, ref NativeMethods.SpDeviceInterfaceData data)
    {
        NativeMethods.SetupDiGetDeviceInterfaceDetail(set, ref data, IntPtr.Zero, 0, out int required, IntPtr.Zero);
        if (required <= 4)
        {
            return string.Empty;
        }

        IntPtr detail = Marshal.AllocHGlobal(required);
        try
        {
            Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6);
            if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(set, ref data, detail, required, out _, IntPtr.Zero))
            {
                return string.Empty;
            }

            return Marshal.PtrToStringUni(IntPtr.Add(detail, 4)) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(detail);
        }
    }

    private static bool TryReadAttributes(SafeFileHandle handle, out int vendorId, out int productId)
    {
        var attributes = new NativeMethods.HiddAttributes
        {
            Size = Marshal.SizeOf<NativeMethods.HiddAttributes>(),
            VendorId = 0,
            ProductId = 0,
            VersionNumber = 0,
        };

        if (!NativeMethods.HidD_GetAttributes(handle, ref attributes))
        {
            vendorId = 0;
            productId = 0;
            return false;
        }

        vendorId = attributes.VendorId;
        productId = attributes.ProductId;
        return true;
    }

    private static string ReadProductName(SafeFileHandle handle)
    {
        char[] buffer = new char[MaximumProductNameLength];
        if (!NativeMethods.HidD_GetProductString(handle, buffer, buffer.Length * sizeof(char)))
        {
            return string.Empty;
        }

        int end = Array.IndexOf(buffer, '\0');
        return new string(buffer, 0, end < 0 ? buffer.Length : end);
    }
}
