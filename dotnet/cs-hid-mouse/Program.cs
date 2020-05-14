using HelperExtension;
using System;
using System.Diagnostics;
using System.Threading;
using UsbipDevice;

namespace cs_hid_mouse
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("cs-hid-mouse");
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

            // byte[] reportBuffer = MouseDescriptors.RelativeReport;
            // Usbip device = new Usbip(MouseDescriptors.Device, MouseDescriptors.RelativeHid, reportBuffer);

            // byte[] reportBuffer = MouseDescriptors.AbsoluteReport;
            // Usbip device = new Usbip(MouseDescriptors.Device, MouseDescriptors.AbsoluteHid, reportBuffer);

            byte[] reportBuffer = MouseDescriptors.AbsoluteAndRelativeReport;
            using (Usbip device = new Usbip(UsbDescriptors.Device, MouseDescriptors.AbsoluteAndRelativeHid, reportBuffer))
            {
                MouseDevice mouse = new MouseDevice(device, reportBuffer, cxScreen, cyScreen);

                device.Run();

                if (waitLocalHost == true)
                {
                    Thread usbipServer = new Thread(usbipServer_Run);
                    usbipServer.IsBackground = true;
                    usbipServer.Start();
                }

                MouseTest(mouse);
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

        private static void MouseTest(MouseDevice mouse)
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

            while (true)
            {
                Console.Write("Mouse> ");
                string text = Console.ReadLine();

                Thread.Sleep(2000);

                if (text == "quit")
                {
                    mouse.Dispose();
                    break;
                }

                Thread.Sleep(2000);
                mouse.SendText(text);
            }
        }
    }
}
