using System;
using System.Collections.Generic;
using System.Text;

namespace UsbipDevice
{
    public static class KeyboardDescriptors
    {
        public static byte[] Report = {
           0x05, 0x01, //Usage Page (Generic Desktop),
           0x09, 0x06, //Usage (Keyboard),
           0xA1, 0x01, //Collection (Application),
           0x05, 0x07, //Usage Page (Key Codes);
           0x19, 0xE0, //Usage Minimum (224),
           0x29, 0xE7, //Usage Maximum (231),
           0x15, 0x00, //Logical Minimum (0),
           0x25, 0x01, //Logical Maximum (1),
           0x75, 0x01, //Report Size (1),
           0x95, 0x08, //Report Count (8),
           0x81, 0x02, //Input (Data, Variable, Absolute),
           0x95, 0x01, //Report Count (1),
           0x75, 0x08, //Report Size (8),
           0x81, 0x01, //Input (Constant),
           0x95, 0x05, //Report Count (5),
           0x75, 0x01, //Report Size (1),
           0x05, 0x08, //Usage Page (Page# for LEDs),
           0x19, 0x01, //Usage Minimum (1),
           0x29, 0x05, //Usage Maximum (5),
           0x91, 0x02, //Output (Data, Variable, Absolute),
           0x95, 0x01, //Report Count (1),
           0x75, 0x03, //Report Size (3),
           0x91, 0x01, //Output (Constant),
           0x95, 0x06, //Report Count (6),
           0x75, 0x08, //Report Size (8),
           0x15, 0x00, //Logical Minimum (0),
           0x25, 0x65, //Logical Maximum(101),
           0x05, 0x07, //Usage Page (Key Codes),
           0x19, 0x00, //Usage Minimum (0),
           0x29, 0x65, //Usage Maximum (101),
	       0x81, 0x00, //Input (Data, Array),
	       0xC0        //End Collection 
        };

        public static USB_DEVICE_DESCRIPTOR Device = new USB_DEVICE_DESCRIPTOR
        {
            bLength = 0x12,
            bDescriptorType = 0x01,
            bcdUSB = 0x0110,
            bDeviceClass = 0x00,
            bDeviceSubClass = 0x00,
            bDeviceProtocol = 0x00,
            bMaxPacketSize0 = 0x08,
            idVendor = 0x2706,
            idProduct = 0x0100,
            bcdDevice = 0x0000,
            iManufacturer = 0x00,
            iProduct = 0x00,
            iSerialNumber = 0x00,
            bNumConfigurations = 0x01
        };

        public static CONFIG_HID Hid = new CONFIG_HID
        {
            dev_conf = new USB_CONFIGURATION_DESCRIPTOR
            {
                /* Configuration Descriptor */
                bLength = 0x09,//sizeof(USB_CFG_DSC),    // Size of this descriptor in bytes
                bDescriptorType = (byte)DescriptorType.USB_DESCRIPTOR_CONFIGURATION,                // CONFIGURATION descriptor type
                wTotalLength = 0x0022,                 // Total length of data for this cfg
                bNumInterfaces = 1,                      // Number of interfaces in this cfg
                bConfigurationValue = 1,                      // Index value of this configuration
                iConfiguration = 0,                      // Configuration string index
                bmAttributes = 0x80,
                bMaxPower = 50,                     // Max power consumption (2X mA)
            },
            dev_int = new USB_INTERFACE_DESCRIPTOR
            {
                /* Interface Descriptor */
                bLength = 0x09,//sizeof(USB_INTF_DSC),   // Size of this descriptor in bytes
                bDescriptorType = (byte)DescriptorType.USB_DESCRIPTOR_INTERFACE,               // INTERFACE descriptor type
                bInterfaceNumber = 0,                      // Interface Number
                bAlternateSetting = 0,                      // Alternate Setting Number
                bNumEndpoints = 1,                      // Number of endpoints in this intf
                bInterfaceClass = 0x03,                   // Class code
                bInterfaceSubClass = 0x01,                   // Subclass code
                bInterfaceProtocol = 0x01,                   // Protocol code
                iInterface = 0,                      // Interface string index
            },
            dev_hid = new USB_HID_DESCRIPTOR
            {
                /* HID Class-Specific Descriptor */
                bLength = 0x09,               // Size of this descriptor in bytes RRoj hack
                bDescriptorType = 0x21,                // HID descriptor type
                bcdHID = 0x0001,                 // HID Spec Release Number in BCD format (1.11)
                bCountryCode = 0x00,                   // Country Code (0x00 for Not supported)
                bNumDescriptors = 0x01,         // Number of class descriptors, see usbcfg.h
                bRPDescriptorType = 0x22,                // Report descriptor type
                wRPDescriptorLength = 0x003F,           // Size of the report descriptor
            },
            dev_ep = new USB_ENDPOINT_DESCRIPTOR
            {
                /* Endpoint Descriptor */
                bLength = 0x07,/*sizeof(USB_EP_DSC)*/
                bDescriptorType = (byte)DescriptorType.USB_DESCRIPTOR_ENDPOINT,    //Endpoint Descriptor
                bEndpointAddress = 0x81,            //EndpointAddress
                bmAttributes = 0x03,                       //Attributes
                wMaxPacketSize = 0x0008,                  //size
                bInterval = 0xFF                        //Interval
            }
        };
    }
}
