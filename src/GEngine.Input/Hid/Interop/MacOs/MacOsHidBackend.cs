// macOS, where a device is an object in a registry rather than a file.
//
// IOHIDManagerCreate makes the manager, a matching dictionary tells it what to look for -
// here, everything - and IOHIDManagerCopyDevices hands back a CFSet of IOHIDDeviceRefs. Each
// one is asked for its VendorID and ProductID, which are Core Foundation numbers, and for its
// Product, which is a Core Foundation string.
//
// There is no path on macOS, so one is invented: the LocationID, which the system assigns per
// physical port and keeps stable while the device stays plugged in. The devices themselves are
// retained between enumerating and opening - that is the CFRetain and CFRelease discipline the
// build prompt warns about, and it is why this backend holds a dictionary rather than
// returning bare pointers.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GEngine.Input.Hid.Interop.MacOs;

/// <summary>The HID backend for macOS, over the IOKit HID manager.</summary>
public sealed class MacOsHidBackend : IHidBackend, IDisposable
{
    private readonly Dictionary<string, IntPtr> _retained = [];

    /// <inheritdoc/>
    public string Name => "macos iokit";

    /// <inheritdoc/>
    public IReadOnlyList<HidDeviceInfo> Enumerate()
    {
        List<HidDeviceInfo> found = [];
        if (!OperatingSystem.IsMacOS())
        {
            return found;
        }

        ReleaseRetained();
        WithManager(found);
        found.Sort(static (left, right) => string.CompareOrdinal(left.Path, right.Path));
        return found;
    }

    // Core Foundation counts references by hand, so every Create has to have its Release on
    // the way out however the way out happens. That is what the finally is for.
    private void WithManager(List<HidDeviceInfo> found)
    {
        IntPtr manager = NativeMethods.IOHIDManagerCreate(IntPtr.Zero, 0);
        if (manager == IntPtr.Zero)
        {
            return;
        }

        try
        {
            Collect(manager, found);
        }
        finally
        {
            _ = NativeMethods.IOHIDManagerClose(manager, 0);
            CoreFoundationStrings.Release(manager);
        }
    }

    /// <inheritdoc/>
    public IHidDevice? Open(HidDeviceInfo device)
    {
        if (!OperatingSystem.IsMacOS() || !_retained.TryGetValue(device.Path, out IntPtr reference))
        {
            return null;
        }

        if (NativeMethods.IOHIDDeviceOpen(reference, 0) != 0)
        {
            return null;
        }

        // The device object is handed to MacOsHidDevice, which releases it; this backend must
        // therefore stop counting it as retained, or it would release it a second time.
        _retained.Remove(device.Path);
        return new MacOsHidDevice(device.Path, reference, KnownDevices.UsbReportLength);
    }

    /// <summary>Releases every device this backend is still holding on to.</summary>
    public void Dispose() => ReleaseRetained();

    private void Collect(IntPtr manager, List<HidDeviceInfo> found)
    {
        NativeMethods.IOHIDManagerSetDeviceMatching(manager, IntPtr.Zero);
        if (NativeMethods.IOHIDManagerOpen(manager, 0) != 0)
        {
            return;
        }

        WithDevices(NativeMethods.IOHIDManagerCopyDevices(manager), found);
    }

    private void WithDevices(IntPtr devices, List<HidDeviceInfo> found)
    {
        if (devices == IntPtr.Zero)
        {
            return;
        }

        try
        {
            AddAll(devices, found);
        }
        finally
        {
            CoreFoundationStrings.Release(devices);
        }
    }

    private void AddAll(IntPtr devices, List<HidDeviceInfo> found)
    {
        long reported = NativeMethods.CFSetGetCount(devices).ToInt64();
        int count = (int)Math.Min(reported, int.MaxValue);
        if (count <= 0)
        {
            return;
        }

        IntPtr[] references = new IntPtr[count];
        NativeMethods.CFSetGetValues(devices, references);
        foreach (IntPtr reference in references)
        {
            Add(reference, found);
        }
    }

    private void Add(IntPtr reference, List<HidDeviceInfo> found)
    {
        int vendorId = CoreFoundationStrings.ReadIntProperty(reference, "VendorID");
        int productId = CoreFoundationStrings.ReadIntProperty(reference, "ProductID");
        if (vendorId == 0 && productId == 0)
        {
            return;
        }

        string path = PathOf(reference, vendorId, productId);
        found.Add(new HidDeviceInfo(vendorId, productId, path, CoreFoundationStrings.ReadStringProperty(reference, "Product")));

        // Retained because the CFSet that owns it is released as soon as enumeration ends, and
        // Open needs the object to still exist.
        _retained[path] = NativeMethods.CFRetain(reference);
    }

    private static string PathOf(IntPtr reference, int vendorId, int productId)
    {
        int location = CoreFoundationStrings.ReadIntProperty(reference, "LocationID");
        return string.Format(
            CultureInfo.InvariantCulture,
            "iohid://{0:X4}:{1:X4}/{2:X8}",
            vendorId,
            productId,
            location);
    }

    private void ReleaseRetained()
    {
        foreach (IntPtr reference in _retained.Values)
        {
            CoreFoundationStrings.Release(reference);
        }

        _retained.Clear();
    }
}
