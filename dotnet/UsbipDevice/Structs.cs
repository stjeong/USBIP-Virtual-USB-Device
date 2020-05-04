using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbipDevice
{
    public enum UsbIpCommandType
    {
        UNKNOWN = 0x0,

        RESP_IMPORT = 0x03,

        REQ_DEVLIST = 0x8003,
    }

    public enum DescriptorType : byte
    {
        USB_DESCRIPTOR_DEVICE = 0x01,    // Device Descriptor.
        USB_DESCRIPTOR_CONFIGURATION = 0x02,    // Configuration Descriptor.
        USB_DESCRIPTOR_STRING = 0x03,    // String Descriptor.
        USB_DESCRIPTOR_INTERFACE = 0x04,    // Interface Descriptor.
        USB_DESCRIPTOR_ENDPOINT = 0x05,    // Endpoint Descriptor.
        USB_DESCRIPTOR_DEVICE_QUALIFIER = 0x06,    // Device Qualifier.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OP_REQ_DEVLIST
    {
        public short version;
        public ushort command;
        public int status;

        public UsbIpCommandType GetCommandType()
        {
            ushort command = (ushort)IPAddress.NetworkToHostOrder((short)this.command);
            return (UsbIpCommandType)command;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USBIP_RET_SUBMIT
    {
        public int command;
        public int seqnum;
        public int devid;
        public int direction;
        public int ep;
        public int status;
        public int actual_length;
        public int start_frame;
        public int number_of_packets;
        public int error_count; 
        public long setup;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USB_DEVICE_QUALIFIER_DESCRIPTOR
    {
        public byte bLength;               // Size of this descriptor
        public byte bType;                 // Type, always USB_DESCRIPTOR_DEVICE_QUALIFIER
        public short bcdUSB;                // USB spec version, in BCD
        public byte bDeviceClass;          // Device class code
        public byte bDeviceSubClass;       // Device sub-class code
        public byte bDeviceProtocol;       // Device protocol
        public byte bMaxPacketSize0;       // EP0, max packet size
        public byte bNumConfigurations;    // Number of "other-speed" configurations
        public byte bReserved;             // Always zero (0)
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OP_REP_IMPORT
    {
        public short version;
        public short command;
        public int  status;
        //------------- if not ok, finish here
        public fixed byte usbPath [256];
        public fixed byte busID [32];
        public int busnum;
        public int devnum;
        public int speed;
        public short idVendor;
        public short idProduct;
        public short bcdDevice;
        public byte bDeviceClass;
        public byte bDeviceSubClass;
        public byte bDeviceProtocol;
        public byte bConfigurationValue;
        public byte bNumConfigurations;
        public byte bNumInterfaces;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StandardDeviceRequest
    {
        public byte bmRequestType;
        public byte bRequest;
        public byte wValue0;
        public byte wValue1;
        public byte wIndex0;
        public byte wIndex1;
        public short wLength;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USBIP_CMD_SUBMIT
    {
        public int command;
        public int seqnum;
        public int devid;
        public int direction;
        public int ep;
        public int transfer_flags;
        public int transfer_buffer_length;
        public int start_frame;
        public int number_of_packets;
        public int interval;
        public long setup;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USB_DEVICE_DESCRIPTOR
    {
        public byte bLength;               // Length of this descriptor.
        public byte bDescriptorType;       // DEVICE descriptor type (USB_DESCRIPTOR_DEVICE).
        public short bcdUSB;                // USB Spec Release Number (BCD).
        public byte bDeviceClass;          // Class code (assigned by the USB-IF). 0xFF-Vendor specific.
        public byte bDeviceSubClass;       // Subclass code (assigned by the USB-IF).
        public byte bDeviceProtocol;       // Protocol code (assigned by the USB-IF). 0xFF-Vendor specific.
        public byte bMaxPacketSize0;       // Maximum packet size for endpoint 0.
        public short idVendor;              // Vendor ID (assigned by the USB-IF).
        public short idProduct;             // Product ID (assigned by the manufacturer).
        public short bcdDevice;             // Device release number (BCD).
        public byte iManufacturer;         // Index of String Descriptor describing the manufacturer.
        public byte iProduct;              // Index of String Descriptor describing the product.
        public byte iSerialNumber;         // Index of String Descriptor with the device's serial number.
        public byte bNumConfigurations;    // Number of possible configurations.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USB_CONFIGURATION_DESCRIPTOR
    {
        public byte bLength;               // Length of this descriptor.
        public byte bDescriptorType;       // CONFIGURATION descriptor type (USB_DESCRIPTOR_CONFIGURATION).
        public short wTotalLength;          // Total length of all descriptors for this configuration.
        public byte bNumInterfaces;        // Number of interfaces in this configuration.
        public byte bConfigurationValue;   // Value of this configuration (1 based).
        public byte iConfiguration;        // Index of String Descriptor describing the configuration.
        public byte bmAttributes;          // Configuration characteristics.
        public byte bMaxPower;             // Maximum power consumed by this configuration.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USB_INTERFACE_DESCRIPTOR
    {
        public byte bLength;               // Length of this descriptor.
        public byte bDescriptorType;       // INTERFACE descriptor type (USB_DESCRIPTOR_INTERFACE).
        public byte bInterfaceNumber;      // Number of this interface (0 based).
        public byte bAlternateSetting;     // Value of this alternate interface setting.
        public byte bNumEndpoints;         // Number of endpoints in this interface.
        public byte bInterfaceClass;       // Class code (assigned by the USB-IF).  0xFF-Vendor specific.
        public byte bInterfaceSubClass;    // Subclass code (assigned by the USB-IF).
        public byte bInterfaceProtocol;    // Protocol code (assigned by the USB-IF).  0xFF-Vendor specific.
        public byte iInterface;            // Index of String Descriptor describing the interface.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USB_HID_DESCRIPTOR
    {
        public byte bLength;
        public byte bDescriptorType;
        public short bcdHID;
        public byte bCountryCode;
        public byte bNumDescriptors;
        public byte bRPDescriptorType;
        public short wRPDescriptorLength;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USB_ENDPOINT_DESCRIPTOR
    {
        public byte bLength;               // Length of this descriptor.
        public byte bDescriptorType;       // ENDPOINT descriptor type (USB_DESCRIPTOR_ENDPOINT).
        public byte bEndpointAddress;      // Endpoint address. Bit 7 indicates direction (0=OUT, 1=IN).
        public byte bmAttributes;          // Endpoint transfer type.
        public short wMaxPacketSize;        // Maximum packet size.
        public byte bInterval;             // Polling interval in frames.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CONFIG_HID
    {
        public USB_CONFIGURATION_DESCRIPTOR dev_conf;
        public USB_INTERFACE_DESCRIPTOR dev_int;
        public USB_HID_DESCRIPTOR dev_hid;
        public USB_ENDPOINT_DESCRIPTOR dev_ep;
    }
}
