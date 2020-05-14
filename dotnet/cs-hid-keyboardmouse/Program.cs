using HelperExtension;
using System;
using System.Diagnostics;
using System.Threading;
using UsbipDevice;

namespace cs_hid_keyboardmouse
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("cs-hid-keyboardmouse");
            bool waitLocalHost = true;

            if (args.Length >= 1)
            {
                if (args[0] == "-w")
                {
                    waitLocalHost = false;
                }
            }

            int cxScreen = SafeMethods.GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN);
            int cyScreen = SafeMethods.GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN);
            Console.WriteLine($"CX: {cxScreen}");
            Console.WriteLine($"CY: {cyScreen}");

            byte[] reportBuffer = KeyboardMouseDescriptors.Report;
            using (Usbip device = new Usbip(UsbDescriptors.Device, KeyboardMouseDescriptors.Hid, reportBuffer))
            {

                MouseDevice mouse = new MouseDevice(device, reportBuffer, cxScreen, cyScreen);
                KeyboardDevice keyboard = new KeyboardDevice(device, reportBuffer);

                device.Run();

                if (waitLocalHost == true)
                {
                    Thread usbipServer = new Thread(usbipServer_Run);
                    usbipServer.IsBackground = true;
                    usbipServer.Start();
                }

                KeyboardMouseTest(mouse, keyboard);
            }
        }

        private static void usbipServer_Run(object obj)
        {
            foreach (Process process in Process.GetProcessesByName("usbip"))
            {
                process.Kill();
            }

            // usbip attach -r 127.0.0.1 -b 1-1
            Process.Start("usbip", "attach -r 127.0.0.1 -b 1-1");
        }

        private static void KeyboardMouseTest(MouseDevice mouse, KeyboardDevice keyboard)
        {
            Console.WriteLine("Wait for usbip...");
            while (true)
            {
                Console.Write(".");

                if (mouse.Connected == true)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            bool mouseMode = false;

            while (true)
            {
                Console.Write(((mouseMode == true) ? "Mouse" : "Keyboard") + "> ");
                string text = Console.ReadLine();

                if (text == "quit")
                {
                    break;
                }

                if (text == "--mode")
                {
                    mouseMode ^= mouseMode;
                    continue;
                }

                Thread.Sleep(2000);

                if (mouseMode == true)
                {
                    mouse.SendText(text);
                }
                else
                {
                    keyboard.SendText(text);
                }
            }
        }
    }
}



