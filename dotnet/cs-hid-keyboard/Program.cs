using System;
using System.Collections.Generic;
using System.Threading;
using UsbipDevice;

namespace cs_hid_keyboard
{
    class Program
    {
        static byte[] _keyboard_report = {
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

        static USB_DEVICE_DESCRIPTOR _dev_dsc = new USB_DEVICE_DESCRIPTOR
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

        static CONFIG_HID _configuration_hid = new CONFIG_HID
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

        // usbip -a 127.0.0.1 "1-1"
        // usbip attach -r 127.0.0.1 -b 1-1
        static void Main(string[] args)
        {
            Console.WriteLine("cs-hid-keyboard");

            Usbip device = new Usbip(_dev_dsc, _configuration_hid, _keyboard_report);

            Thread t = new Thread(key_func);
            t.IsBackground = true;
            t.Start(device);

            device.Run();
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static void key_func(object obj)
        {
            Usbip device = obj as Usbip;

            while (true)
            {
                Thread.Sleep(1000 * 3);
                SendKey(device, 't');
            }
        }

        static void SendKey(Usbip device, char ch)
        {
            byte[] buf = GetKeyBuf(ch);
            device.Send(buf);
            device.Send(new byte[8]);
        }

        static byte[] GetKeyBuf(char ch)
        {
            byte[] buf = new byte[8];
            buf[2] = CharToCode(ch);

            return buf;
        }

        static Dictionary<char, byte> _letterKey = new Dictionary<char, byte>();
        static Dictionary<char, byte> _controlKey = new Dictionary<char, byte>();
        static HashSet<char> _upper = new HashSet<char>();
        static Dictionary<char, byte> _modifierKey = new Dictionary<char, byte>();
        
        public const char VK_IME_KEY = (char)0x0f;
        public const char VK_CAPITAL = (char)0x14;

        static byte CharToCode(char ch)
        {
            if (char.IsLetter(ch) == true)
            {
                short lower = (short)char.ToLower(ch);
                byte keyCode = (byte)((lower - 'a') + 4);

                return keyCode;
            }

            if (_letterKey.ContainsKey(ch) == true)
            {
                return _letterKey[ch];
            }

            if (_controlKey.ContainsKey(ch) == true)
            {
                return _controlKey[ch];
            }

            return 0;
        }

        static Program()
        {
            for (int i = (int)'A'; i < (int)'Z'; i++)
            {
                _upper.Add((char)i);
            }

            _upper.Add('~');
            _upper.Add('!');
            _upper.Add('@');
            _upper.Add('#');
            _upper.Add('$');
            _upper.Add('%');
            _upper.Add('^');
            _upper.Add('&');
            _upper.Add('*');
            _upper.Add('(');
            _upper.Add(')');
            _upper.Add('_');
            _upper.Add('+');
            _upper.Add('{');
            _upper.Add('}');
            _upper.Add('|');
            _upper.Add(':');
            _upper.Add('"');
            _upper.Add('<');
            _upper.Add('>');
            _upper.Add('?');

            // lower alphabet
            // 4 ~ 1d

            // 1e ~ 26
            {
                for (int i = 0; i < 9; i++)
                {
                    _letterKey.Add((char)('1' + i), (byte)(0x1e + i));
                }
                _letterKey.Add('0', 0x27);

                _letterKey.Add('!', 0x1e);
                _letterKey.Add('@', 0x1f);
                _letterKey.Add('#', 0x20);
                _letterKey.Add('$', 0x21);
                _letterKey.Add('%', 0x22);
                _letterKey.Add('^', 0x23);
                _letterKey.Add('&', 0x24);
                _letterKey.Add('*', 0x25);
                _letterKey.Add('(', 0x26);
                _letterKey.Add(')', 0x27);
            }

            _controlKey.Add('\r', 0x28);
            _controlKey.Add((char)ConsoleKey.Escape, 0x29);

            _controlKey.Add('\b', 0x2a);
            _controlKey.Add('\t', 0x2b);

            _letterKey.Add(' ', 0x2c);

            _letterKey.Add('-', 0x2d);
            _letterKey.Add('_', 0x2d);

            _letterKey.Add('=', 0x2e);
            _letterKey.Add('+', 0x2e);

            _letterKey.Add('[', 0x2f);
            _letterKey.Add('{', 0x2f);
            _letterKey.Add(']', 0x30);
            _letterKey.Add('}', 0x30);

            _letterKey.Add('\\', 0x31);
            _letterKey.Add('|', 0x31);

            // 0x32 ???
            _letterKey.Add(';', 0x33);
            _letterKey.Add(':', 0x33);

            _letterKey.Add('\'', 0x34);
            _letterKey.Add('"', 0x34);

            _letterKey.Add('`', 0x35);
            _letterKey.Add('~', 0x35);

            _letterKey.Add(',', 0x36);
            _letterKey.Add('<', 0x36);

            _letterKey.Add('.', 0x37);
            _letterKey.Add('>', 0x37);

            _letterKey.Add('/', 0x38);
            _letterKey.Add('?', 0x38);


            // ConsoleKey.CapsLock == 0x14(20)
            _controlKey.Add(VK_CAPITAL, 0x39);

            // 3a ~ 45
            for (int i = 0; i < 12; i++)
            {
                _controlKey.Add((char)(ConsoleKey.F1 + i), (byte)(0x3a + i));
            }

            // 0x46 (Open OneDrive Dialog)
            // 0x47 ???
            // 0x48 ???

            _controlKey.Add((char)ConsoleKey.Insert, 0x49);
            _controlKey.Add((char)ConsoleKey.Home, 0x4a);
            _controlKey.Add((char)ConsoleKey.PageUp, 0x4b);
            _controlKey.Add((char)ConsoleKey.Delete, 0x4c);
            _controlKey.Add((char)ConsoleKey.End, 0x4d);
            _controlKey.Add((char)ConsoleKey.PageDown, 0x4e);
            _controlKey.Add((char)ConsoleKey.RightArrow, 0x4f);

            _controlKey.Add((char)ConsoleKey.LeftArrow, 0x50);
            _controlKey.Add((char)ConsoleKey.DownArrow, 0x51);
            _controlKey.Add((char)ConsoleKey.UpArrow, 0x52);

            // 0x53 NumLock
            // 0x54 '/'
            // _controlKey.Add((char)ConsoleKey.Divide, 0x54);
            // 0x55 '*'
            // _controlKey.Add((char)ConsoleKey.Multiply, 0x55);
            // 0x56 '-'
            // _controlKey.Add((char)ConsoleKey.Subtract, 0x56);
            // 0x57 '+'
            // _controlKey.Add((char)ConsoleKey.Add, 0x57);
            // 0x58 ENTER (kp-enter)
            // 0x59 '1' ~ 0x61 '9'
            // _controlKey.Add((char)ConsoleKey.NumPad1, 0x59);
            //                     ~ ConsoleKey.NumPad9, 0x61
            // 0x62 '0'
            // _controlKey.Add((char)ConsoleKey.NumPad0, 0x62);
            // 0x63 '.'
            // _controlKey.Add((char)ConsoleKey.Decimal, 0x63);

            // 0x64 '\\'
            // 0x65 Fn + Ins
            // 0x66 ~ 0xff ???

            // _letterKey.Add((char)ConsoleKey.Multiply, 0x55);

            // https://msdn.microsoft.com/ko-kr/library/windows/desktop/dd375731(v=vs.85).aspx
            // 0x0f(15) == IME Hangul mode
            _modifierKey.Add(VK_IME_KEY, (byte)ModifierKey.Hangul_Toggle_Key);

            // 0x19(25) == HanjaKey
            _modifierKey.Add((char)0x19, (byte)ModifierKey.Hanja_Toggle_Key);

            _modifierKey.Add((char)ConsoleKey.LeftWindows, (byte)ModifierKey.Left_Window_Key);
            _modifierKey.Add((char)ConsoleKey.RightWindows, (byte)ModifierKey.Left_Window_Key);
        }
    }


    [Flags]
    public enum ModifierKey : byte
    {
        None,
        Left_Ctrl_Key = 0x01,
        Left_Shift_Key = 0x02,
        Left_Alt_Key = 0x04,
        Left_Window_Key = 0x08,

        Right_Ctrl_Key = 0x10,
        Hanja_Toggle_Key = 0x10,

        Right_Shift_Key = 0x20,

        Right_Alt_Key = 0x40,
        Hangul_Toggle_Key = 0x40,

        Right_Window_Key = 0x80,
    }
}
