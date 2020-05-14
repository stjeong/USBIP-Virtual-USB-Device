using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UsbipDevice
{
    public static class UsbDescriptors
    {
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

        internal static CONFIG_HID CommonHid = new CONFIG_HID
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
                wRPDescriptorLength = 0x0,           // Size of the report descriptor
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
           0x75, 0x08, //Report Size (8),
           0x95, 0x01, //Report Count (1),
           0x81, 0x01, //Input (Constant),
           0x75, 0x01, //Report Size (1),
           0x95, 0x05, //Report Count (5),
           0x05, 0x08, //Usage Page (Page# for LEDs),
           0x19, 0x01, //Usage Minimum (1),
           0x29, 0x05, //Usage Maximum (5),
           0x91, 0x02, //Output (Data, Variable, Absolute),
           0x75, 0x03, //Report Size (3),
           0x95, 0x01, //Report Count (1),
           0x91, 0x01, //Output (Constant),
           0x75, 0x08, //Report Size (8),
           0x95, 0x06, //Report Count (6),
           0x15, 0x00, //Logical Minimum (0),
           0x25, 0x65, //Logical Maximum(101),
           0x05, 0x07, //Usage Page (Key Codes),
           0x19, 0x00, //Usage Minimum (0),
           0x29, 0x65, //Usage Maximum (101),
	       0x81, 0x00, //Input (Data, Array),
	       0xC0        //End Collection 
        };

        static CONFIG_HID _hid;
        public static CONFIG_HID Hid => _hid;

        static KeyboardDescriptors()
        {
            _hid = UsbDescriptors.CommonHid;
            _hid.dev_hid.wRPDescriptorLength = (short)Report.Length;
        }
    }

    public static class MouseDescriptors
    {
        // hid-mouse.c
        //public static byte[] Report = {
        //    0x05, 0x01, /* Usage Page (Generic Desktop)             */
        //    0x09, 0x02, /* Usage (Mouse)                            */
        //    0xA1, 0x01, /* Collection (Application)                 */
        //    0x09, 0x01, /*  Usage (Pointer)                         */
        //    0xA1, 0x00, /*  Collection (Physical)                   */
        //    0x05, 0x09, /*      Usage Page (Buttons)                */
        //    0x19, 0x01, /*      Usage Minimum (01)                  */
        //    0x29, 0x03, /*      Usage Maximum (03)                  */
        //    0x15, 0x00, /*      Logical Minimum (0)                 */
        //    0x25, 0x01, /*      Logical Maximum (1)                 */
        //    0x95, 0x03, /*      Report Count (3)                    */
        //    0x75, 0x01, /*      Report Size (1)                     */
        //    0x81, 0x02, /*      Input (Data, Variable, Absolute)    */
        //    0x95, 0x01, /*      Report Count (1)                    */
        //    0x75, 0x05, /*      Report Size (5)                     */
        //    0x81, 0x01, /*      Input (Constant)    ;5 bit padding  */
        //    0x05, 0x01, /*      Usage Page (Generic Desktop)        */
        //    0x09, 0x30, /*      Usage (X)                           */
        //    0x09, 0x31, /*      Usage (Y)                           */
        //    0x09, 0x38, /*      Logical Minimum (-127)              */
        //    0x15, 0x81, /*      Logical Maximum (127)               */
        //    0x25, 0x7F, /*      Report Size (8)                     */
        //    0x75, 0x08, /*      Report Count (3)                    */
        //    0x95, 0x03, /*      Input (Data, Variable, Relative)    */
        //    0x81, 0x06,
        //    0xC0, 0xC0
        //};

        public static byte[] RelativeReport = {
            0x05, 0x01, //  USAGE_PAGE (Generic Desktop)
            0x09, 0x02, //  USAGE (Mouse)
            0xa1, 0x01, //  COLLECTION (Application)
            0x09, 0x01, //      USAGE (Pointer)
            0xa1, 0x00, //      COLLECTION (Physical)
            0x05, 0x09, //          USAGE_PAGE (Button)
            0x19, 0x01, //          USAGE_MINIMUM (Button 1)
            0x29, 0x03, //          USAGE_MAXIMUM (Button 3)
            0x15, 0x00, //          LOGICAL_MINIMUM (0)
            0x25, 0x01, //          LOGICAL_MAXIMUM (1)
            0x75, 0x01, //          REPORT_SIZE (1)
            0x95, 0x03, //          REPORT_COUNT (3)
            0x81, 0x02, //          INPUT (Data, Var, Abs)
            0x75, 0x05, //          REPORT_SIZE (5)
            0x95, 0x01, //          REPORT_COUNT (1)
            0x81, 0x03, //          INPUT (Cnst, Var, Abs)
            0x05, 0x01, //          USAGE_PAGE (Generic Desktop)
            0x09, 0x30, //          USAGE (X)
            0x09, 0x31, //          USAGE (Y)
            0x15, 0x81, //          LOGICAL_MINIMUM (-127)
            0x25, 0x7f, //          LOGICAL_MAXIMUM (127)
            0x75, 0x08, //          REPORT_SIZE (8)
            0x95, 0x02, //          REPORT_COUNT (2)
            0x81, 0x06, //          INPUT (Data, Var, Rel)
            0xC0    , //      END_COLLECTION
            0xC0    , //  END_COLLECTION
        };

        public static byte[] AbsoluteReport = {
            0x05, 0x01, //  USAGE_PAGE (Generic Desktop)
            0x09, 0x02, //  USAGE (Mouse)
            0xa1, 0x01, //  COLLECTION (Application)
            0x09, 0x01, //      USAGE (Pointer)
            0xa1, 0x00, //      COLLECTION (Physical)
            0x05, 0x09, //          USAGE_PAGE (Button)
            0x19, 0x01, //          USAGE_MINIMUM (Button 1)
            0x29, 0x03, //          USAGE_MAXIMUM (Button 3)
            0x15, 0x00, //          LOGICAL_MINIMUM (0)
            0x25, 0x01, //          LOGICAL_MAXIMUM (1)
            0x75, 0x01, //          REPORT_SIZE (1)
            0x95, 0x03, //          REPORT_COUNT (3)
            0x81, 0x02, //          INPUT (Data, Var, Abs)
            0x75, 0x05, //          REPORT_SIZE (5)
            0x95, 0x01, //          REPORT_COUNT (1)
            0x81, 0x03, //          INPUT (Cnst, Var, Abs)
            0x05, 0x01, //          USAGE_PAGE (Generic Desktop)
            0x09, 0x30, //          USAGE (X)
            0x09, 0x31, //          USAGE (Y)

            0x15, 0x00, //          LOGICAL_MINIMUM (0)
            0x26, 0xff, 0x7f, //          LOGICAL_MAXIMUM (32767)

            0x75, 0x10, //          REPORT_SIZE (16)
            0x95, 0x02, //          REPORT_COUNT (2)
            0x81, 0x02, //          INPUT (Data, Var, Abs)
            0xC0    , //      END_COLLECTION
            0xC0    , //  END_COLLECTION
        };

        public static byte RelativeMouseReportId = 2;
        public static byte AbsoluteMouseReportId = 3;

        public static byte[] AbsoluteAndRelativeReport = {
            0x05, 0x01,         // USAGE_PAGE (Generic Desktop)
            0x09, 0x02,         // USAGE (Mouse)
            0xa1, 0x01,         // COLLECTION (Application)
            0x09, 0x01,         //     USAGE (Pointer)
            0xa1, 0x00,         //     COLLECTION (Physical)
            0x85, RelativeMouseReportId,         //         REPORT_ID (2)
            0x05, 0x09,         //         USAGE_PAGE (Button)
            0x19, 0x01,         //         USAGE_MINIMUM (Button 1)
            0x29, 0x03,         //         USAGE_MAXIMUM (Button 3)
            0x15, 0x00,         //         LOGICAL_MINIMUM (0)
            0x25, 0x01,         //         LOGICAL_MAXIMUM (1)
            0x75, 0x01,         //         REPORT_SIZE (1)
            0x95, 0x03,         //         REPORT_COUNT (3)
            0x81, 0x02,         //         INPUT (Data, Var, Abs)
            0x75, 0x05,         //         REPORT_SIZE (5)
            0x95, 0x01,         //         REPORT_COUNT (1)
            0x81, 0x03,         //         INPUT (Cnst, Var, Abs)
            0x05, 0x01,         //         USAGE_PAGE (Generic Desktop)
            0x09, 0x30,         //         USAGE (X)
            0x09, 0x31,         //         USAGE (Y)
            0x15, 0x81,         //         LOGICAL_MINIMUM (-127)
            0x25, 0x7f,         //         LOGICAL_MAXIMUM (127)
            0x75, 0x08,         //         REPORT_SIZE (8)
            0x95, 0x02,         //         REPORT_COUNT (2)
            0x81, 0x06,         //         INPUT (Data, Var, Rel)
            0xC0,               //     END_COLLECTION
            0xC0,               // END_COLLECTION

            0x05, 0x01,         //USAGE_PAGE (Generic Desktop)
            0x09, 0x02,         //USAGE (Mouse)
            0xa1, 0x01,         //COLLECTION (Application)
            0x09, 0x01,         //    USAGE (Pointer)
            0xa1, 0x00,         //    COLLECTION (Physical)
            0x85, AbsoluteMouseReportId,         //        REPORT_ID (3)
            0x05, 0x01,         //        USAGE_PAGE (Generic Desktop)
            0x09, 0x30,         //        USAGE (X)
            0x09, 0x31,         //        USAGE (Y)
            0x15, 0x00,         //        LOGICAL_MINIMUM(0)
            0x26, 0xff, 0x7f,   //      LOGICAL_MAXIMUM(32767)
            0x75, 0x10,         //      REPORT_SIZE (16)
            0x95, 0x02,         //      REPORT_COUNT (2)
            0x81, 0x02,         //      INPUT (Data, Var, Abs)
            0x09, 0x38,         //      USAGE (Wheel)
            0x15, 0x81,         //      LOGICAL_MINIMUM (-127)
            0x25, 0x7f,         //      LOGICAL_MAXIMUM (127)
            0x75, 0x08,         //      REPORT_SIZE (8)
            0x95, 0x01,         //      REPORT_COUNT (1)
            0x81, 0x06,         //      INPUT (Data, Var, Rel)
            0xC0,               //  END_COLLECTION
            0xC0                //  END_COLLECTION
        };

        static CONFIG_HID _relativeHid;
        public static CONFIG_HID RelativeHid => _relativeHid;

        static CONFIG_HID _absoluteHid;
        public static CONFIG_HID AbsoluteHid => _absoluteHid;

        static CONFIG_HID _absoluteAndRelativeHid;
        public static CONFIG_HID AbsoluteAndRelativeHid => _absoluteAndRelativeHid;

        static MouseDescriptors()
        {
            _relativeHid = UsbDescriptors.CommonHid;
            _relativeHid.dev_hid.wRPDescriptorLength = (short)RelativeReport.Length;

            _absoluteHid = UsbDescriptors.CommonHid;
            _absoluteHid.dev_hid.wRPDescriptorLength = (short)AbsoluteReport.Length;

            _absoluteAndRelativeHid = UsbDescriptors.CommonHid;
            _absoluteAndRelativeHid.dev_hid.wRPDescriptorLength = (short)AbsoluteAndRelativeReport.Length;
        }
    }

    public static class KeyboardMouseDescriptors
    {
        public static byte KeyboardReportId = 1;
        public static byte RelativeMouseReportId = 2;
        public static byte AbsoluteMouseReportId = 3;

        public static byte[] Report = {

            0x05, 0x01, // USAGE_PAGE (Generic Desktop)
            0x09, 0x06, // USAGE (Keyboard)
            0xa1, 0x01, // COLLECTION (Application)
            0x85, KeyboardReportId, //     REPORT_ID (1)
            0x05, 0x07,  //     USAGE_PAGE (Keyboard)
            0x19, 0xe0,  //     USAGE_MINIMUM (Keyboard LeftControl)
            0x29, 0xe7,  //     USAGE_MINIMUM (Keyboard Right GUI)
            0x15, 0x00,  //     LOGICAL_MINIMUM (0)
            0x25, 0x01,  //     LOGICAL_MAXIMUM (1)
            0x75, 0x01,  //     REPORT_SIZE (1)
            0x95, 0x08,  //     REPORT_COUNT (8)
            0x81, 0x02,  //     INPUT (Data, Var, Abs)
            0x75, 0x08,  //     REPORT_SIZE (8)
            0x95, 0x01,  //     REPORT_COUNT (1)
            0x81, 0x03,  //     INPUT (Cnst, Var, Abs)
            0x75, 0x01,  //     REPORT_SIZE (1)
            0x95, 0x05,  //     REPORT_COUNT (5)
            0x05, 0x08,  //     USAGE_PAGE (LEDs)
            0x19, 0x01,  //     USAGE_MINIMUM (Num Lock)
            0x29, 0x05,  //     USAGE_MAXIMUM (Kana)
            0x91, 0x02,  //     OUTPUT (Data, Var, Abs)
            0x75, 0x03,  //     REPORT_SIZE (3)
            0x95, 0x01,  //     REPORT_COUNT (1)
            0x91, 0x03,  //     OUTPUT (Cnst, Var, Abs)

            0x75, 0x08,  //     REPORT_SIZE (8)
            0x95, 0x05,  //     REPORT_COUNT (5)

            0x15, 0x00,  //     LOGICAL_MINIMUM (0)
            0x25, 0x65,  //     LOGICAL_MAXIMUM (101)
            0x05, 0x07,  //     USAGE_PAGE (Keyboard)
            0x19, 0x00,  //     USAGE_MINIMUM (Reserved (no event indicated))
            0x29, 0x65,  //     USAGE_MAXIMUM (Keyboard Application)
            0x81, 0x00,  //     INPUT(Data, Ary, Abs)
            0xC0, // END_COLLECTION

            0x05, 0x01  ,  // USAGE_PAGE (Generic Desktop)
            0x09, 0x02  ,  // USAGE (Mouse)
            0xa1, 0x01  ,  // COLLECTION (Application)
            0x09, 0x01  ,  //     USAGE (Pointer)
            0xa1, 0x00  ,  //     COLLECTION (Physical)
            0x85, RelativeMouseReportId  ,  //         REPORT_ID (2)
            0x05, 0x09  ,  //         USAGE_PAGE (Button)
            0x19, 0x01  ,  //         USAGE_MINIMUM (Button 1)
            0x29, 0x03  ,  //         USAGE_MAXIMUM (Button 3)
            0x15, 0x00  ,  //         LOGICAL_MINIMUM (0)
            0x25, 0x01  ,  //         LOGICAL_MAXIMUM (1)
            0x75, 0x01  ,  //         REPORT_SIZE (1)
            0x95, 0x03  ,  //         REPORT_COUNT (3)
            0x81, 0x02  ,  //         INPUT (Data, Var, Abs)
            0x75, 0x05  ,  //         REPORT_SIZE (5)
            0x95, 0x01  ,  //         REPORT_COUNT (1)
            0x81, 0x03  ,  //         INPUT (Cnst, Var, Abs)
            0x05, 0x01  ,  //         USAGE_PAGE (Generic Desktop)
            0x09, 0x30  ,  //         USAGE (X)
            0x09, 0x31  ,  //         USAGE (Y)
            0x15, 0x81  ,  //         LOGICAL_MINIMUM (-127)
            0x25, 0x7f  ,  //         LOGICAL_MAXIMUM (127)
            0x75, 0x08  ,  //         REPORT_SIZE (8)
            0x95, 0x02  ,  //         REPORT_COUNT (2)
            0x81, 0x06  ,  //         INPUT (Data, Var, Rel)
            0xC0      ,  //     END_COLLECTION
            0xC0      ,  // END_COLLECTION

            0x05, 0x01  , //USAGE_PAGE (Generic Desktop)
            0x09, 0x02  , //USAGE (Mouse)
            0xa1, 0x01  , //COLLECTION (Application)
            0x09, 0x01  , //    USAGE (Pointer)
            0xa1, 0x00  , //    COLLECTION (Physical)
            0x85, AbsoluteMouseReportId  , //        REPORT_ID (3)
            0x05, 0x01  , //        USAGE_PAGE (Generic Desktop)
            0x09, 0x30  , //        USAGE (X)
            0x09, 0x31  , //        USAGE (Y)
            0x15, 0x00,   //    LOGICAL_MINIMUM(0)
            0x26, 0xff, 0x7f  , //    LOGICAL_MAXIMUM(32767)
            0x75, 0x10      , //    REPORT_SIZE (16)
            0x95, 0x02      , //    REPORT_COUNT (2)
            0x81, 0x02      , //    INPUT (Data, Var, Abs)
            0x09, 0x38      , //    USAGE (Wheel)
            0x15, 0x81      , //    LOGICAL_MINIMUM (-127)
            0x25, 0x7f      , //    LOGICAL_MAXIMUM (127)
            0x75, 0x08      , //    REPORT_SIZE (8)
            0x95, 0x01      , //    REPORT_COUNT (1)
            0x81, 0x06      , //    INPUT (Data, Var, Rel)
            0xC0          , //END_COLLECTION
            0xC0   , //   END_COLLECTION
        };

        static CONFIG_HID _hid;
        public static CONFIG_HID Hid => _hid;

        static KeyboardMouseDescriptors()
        {
            _hid = UsbDescriptors.CommonHid;
            _hid.dev_hid.wRPDescriptorLength = (short)Report.Length;
        }
    }
}
