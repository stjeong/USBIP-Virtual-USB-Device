using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UsbipDevice;

namespace cs_hid_keyboard
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("cs-hid-keyboard");
            bool waitLocalHost = true;

            if (args.Length >= 1)
            {
                if (args[0] == "-w")
                {
                    waitLocalHost = false;
                }
            }

            Usbip device = new Usbip(KeyboardDescriptors.Device, KeyboardDescriptors.Hid, KeyboardDescriptors.Report);
            KeyboardDevice keyboard = new KeyboardDevice(device);

            device.Run();

            if (waitLocalHost == true)
            {
                Thread usbipServer = new Thread(usbipServer_Run);
                usbipServer.IsBackground = true;
                usbipServer.Start();
            }

            KeyboardTest(keyboard);
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

        private static void key_func(object obj)
        {
            KeyboardDevice keyboard = obj as KeyboardDevice;
            KeyboardTest(keyboard);
        }

        private static void KeyboardTest(KeyboardDevice keyboard)
        {
            //{
            //    string txt = "abc+*()<ime>xptmxm<ime>";
            //    _usbController.SendText(txt + Environment.NewLine);
            //}

            //{
            //    string txt = "<shift_down>abc<shift_up>";
            //    _usbController.SendText(txt + Environment.NewLine);
            //}

            //{
            //    string txt = "<ctrl_down><esc><ctrl_up>";
            //    _usbController.SendText(txt);
            //}

            //{
            //    string txt = "<capslock>test is good<capslock><return>";
            //    _usbController.SendText(txt);
            //}

            //{
            //    string txt = "<ctrl_down><shift_down><esc><shift_up><ctrl_up>";
            //    _usbController.SendText(txt);
            //}

            Console.WriteLine("Wait for usbip...");
            while (true)
            {
                Console.Write(".");

                if (keyboard.Connected == true)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            while (true)
            {
                Console.Write("Keyboard> ");
                string text = Console.ReadLine();

                Thread.Sleep(2000);

                if (text == "quit")
                {
                    keyboard.Dispose();
                    break;
                }

                keyboard.SendText(text);
            }
        }
    }
}
