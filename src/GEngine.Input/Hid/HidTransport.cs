// How a controller is attached.
//
// It matters because the two ways send different reports: the USB report is 64 bytes starting
// with 0x01, the Bluetooth one is 78 bytes starting with 0x31, and the fields sit at different
// offsets. Decoding one as the other produces a controller that appears to be holding down
// buttons nobody is touching.

namespace GEngine.Input.Hid;

/// <summary>How a HID device is attached.</summary>
public enum HidTransport
{
    /// <summary>Not established yet, or not one the engine recognises.</summary>
    Unknown,

    /// <summary>USB. This is the one gengine decodes.</summary>
    Usb,

    /// <summary>Bluetooth. Recognised, reported, and refused rather than decoded as rubbish.</summary>
    Bluetooth,
}
