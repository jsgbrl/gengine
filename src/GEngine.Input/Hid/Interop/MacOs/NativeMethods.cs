// Signatures only, and twice as many as the other two systems need - because on macOS there
// is no file to open and no handle to read. A device is an object in the IOKit registry,
// described by a Core Foundation dictionary, and it delivers reports by calling back into a
// run loop. So this file imports two frameworks: IOKit for the devices and CoreFoundation for
// the dictionaries, numbers and strings that describe them.
//
// [DllImport] and not [LibraryImport]: the LibraryImport generator emits marshalling stubs
// that require AllowUnsafeBlocks, which rule 6 of the build prompt bans. SYSLIB1054 is turned
// off for this folder in .editorconfig with that reason written next to it.

using System;
using System.Runtime.InteropServices;

namespace GEngine.Input.Hid.Interop.MacOs;

internal static class NativeMethods
{
    internal const string CoreFoundationLibrary =
        "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    internal const string IoKitLibrary =
        "/System/Library/Frameworks/IOKit.framework/IOKit";

    /// <summary>kCFStringEncodingUTF8.</summary>
    internal const uint Utf8Encoding = 0x08000100;

    /// <summary>kCFNumberSInt32Type.</summary>
    internal const int SignedInt32 = 3;

    /// <summary>CFRunLoopRunInMode said the loop has nothing left to do.</summary>
    internal const int RunLoopFinished = 1;

    /// <summary>CFRunLoopRunInMode said somebody stopped the loop.</summary>
    internal const int RunLoopStopped = 2;

    /// <summary>CFRunLoopRunInMode said the time ran out with nothing to do.</summary>
    internal const int RunLoopTimedOut = 3;

    /// <summary>CFRunLoopRunInMode said it delivered something.</summary>
    internal const int RunLoopHandledSource = 4;

    /// <summary>kIOHIDReportTypeInput.</summary>
    internal const uint InputReport = 0;

    internal delegate void InputReportCallback(
        IntPtr context,
        int result,
        IntPtr sender,
        uint type,
        uint reportId,
        IntPtr report,
        IntPtr reportLength);

    // The text arrives as bytes rather than as a string. Handing a managed string to a
    // function that takes a C string leaves the encoding to the marshaller, which is what
    // CA2101 objects to and what makes a non-ASCII device name come out as rubbish; encoding
    // it here means the bytes and the encoding argument cannot disagree.
    [DllImport(CoreFoundationLibrary)]
    internal static extern IntPtr CFStringCreateWithCString(IntPtr allocator, byte[] utf8Text, uint encoding);

    [DllImport(CoreFoundationLibrary)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CFStringGetCString(IntPtr text, byte[] buffer, long bufferSize, uint encoding);

    [DllImport(CoreFoundationLibrary)]
    internal static extern IntPtr CFNumberCreate(IntPtr allocator, int type, ref int value);

    [DllImport(CoreFoundationLibrary)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CFNumberGetValue(IntPtr number, int type, out int value);

    [DllImport(CoreFoundationLibrary)]
    internal static extern IntPtr CFDictionaryCreateMutable(
        IntPtr allocator,
        IntPtr capacity,
        IntPtr keyCallbacks,
        IntPtr valueCallbacks);

    [DllImport(CoreFoundationLibrary)]
    internal static extern void CFDictionarySetValue(IntPtr dictionary, IntPtr key, IntPtr value);

    [DllImport(CoreFoundationLibrary)]
    internal static extern IntPtr CFRetain(IntPtr reference);

    [DllImport(CoreFoundationLibrary)]
    internal static extern void CFRelease(IntPtr reference);

    [DllImport(CoreFoundationLibrary)]
    internal static extern IntPtr CFSetGetCount(IntPtr set);

    [DllImport(CoreFoundationLibrary)]
    internal static extern void CFSetGetValues(IntPtr set, IntPtr[] values);

    [DllImport(CoreFoundationLibrary)]
    internal static extern IntPtr CFRunLoopGetCurrent();

    [DllImport(CoreFoundationLibrary)]
    internal static extern int CFRunLoopRunInMode(IntPtr mode, double seconds, [MarshalAs(UnmanagedType.Bool)] bool returnAfterSourceHandled);

    [DllImport(IoKitLibrary)]
    internal static extern IntPtr IOHIDManagerCreate(IntPtr allocator, uint options);

    [DllImport(IoKitLibrary)]
    internal static extern void IOHIDManagerSetDeviceMatching(IntPtr manager, IntPtr matching);

    [DllImport(IoKitLibrary)]
    internal static extern int IOHIDManagerOpen(IntPtr manager, uint options);

    [DllImport(IoKitLibrary)]
    internal static extern int IOHIDManagerClose(IntPtr manager, uint options);

    [DllImport(IoKitLibrary)]
    internal static extern IntPtr IOHIDManagerCopyDevices(IntPtr manager);

    [DllImport(IoKitLibrary)]
    internal static extern IntPtr IOHIDDeviceGetProperty(IntPtr device, IntPtr key);

    [DllImport(IoKitLibrary)]
    internal static extern int IOHIDDeviceOpen(IntPtr device, uint options);

    [DllImport(IoKitLibrary)]
    internal static extern int IOHIDDeviceClose(IntPtr device, uint options);

    [DllImport(IoKitLibrary)]
    internal static extern void IOHIDDeviceRegisterInputReportCallback(
        IntPtr device,
        IntPtr report,
        IntPtr reportLength,
        InputReportCallback callback,
        IntPtr context);

    [DllImport(IoKitLibrary)]
    internal static extern void IOHIDDeviceScheduleWithRunLoop(IntPtr device, IntPtr runLoop, IntPtr runLoopMode);

    [DllImport(IoKitLibrary)]
    internal static extern void IOHIDDeviceUnscheduleFromRunLoop(IntPtr device, IntPtr runLoop, IntPtr runLoopMode);
}
