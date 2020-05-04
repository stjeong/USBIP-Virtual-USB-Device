using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace UsbipDevice
{
    public class Usbip
    {
        public const int USBIP_PORT = 3240;
        public const short USBIP_PROTOCOL_VERSION = 273;

        USB_DEVICE_QUALIFIER_DESCRIPTOR dev_qua = new USB_DEVICE_QUALIFIER_DESCRIPTOR();

        USB_DEVICE_DESCRIPTOR _desc;
        CONFIG_HID _configuration_hid;
        byte[] _report_descriptor;

        Socket _clntSocket = null;

        public Usbip(USB_DEVICE_DESCRIPTOR desc, CONFIG_HID configuration_hid, byte [] report_descriptor)
        {
            _desc = desc;
            _configuration_hid = configuration_hid;
            _report_descriptor = report_descriptor;
        }

        public unsafe void Run()
        {
            using (Socket srvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, USBIP_PORT);

                srvSocket.Bind(endPoint);
                srvSocket.Listen(10);

                bool doLoop = true;

                while (doLoop)
                {
                    bool attached = false;
                    _clntSocket = null;
                    using (Socket clntSocket = srvSocket.Accept())
                    {
                        while (doLoop)
                        {
                            if (attached == false)
                            {
                                OP_REQ_DEVLIST req = clntSocket.ReadAs<OP_REQ_DEVLIST>();

                                UsbIpCommandType cmdType = req.GetCommandType();
                                Console.WriteLine(cmdType);

                                if (cmdType == UsbIpCommandType.REQ_DEVLIST)
                                {
                                    string busId = clntSocket.ReadBusId();
                                    if (AttachDevice(clntSocket, busId) == false)
                                    {
                                        Console.WriteLine("Failed to attach this device");
                                        break;
                                    }

                                    Console.WriteLine("Device attached.");
                                    attached = true;
                                }
                            }
                            else
                            {
                                USBIP_CMD_SUBMIT cmd = clntSocket.ReadAndUnpackAs<USBIP_CMD_SUBMIT>();

                                Console.WriteLine($"usbip cmd {cmd.command}");
                                Console.WriteLine($"usbip seqnum {cmd.seqnum}");
                                Console.WriteLine($"usbip devid {cmd.devid}");
                                Console.WriteLine($"usbip direction {cmd.direction}");
                                Console.WriteLine($"usbip ep {cmd.ep}");
                                Console.WriteLine($"usbip flags {cmd.transfer_flags}");
                                Console.WriteLine($"usbip number of packets {cmd.number_of_packets}");
                                Console.WriteLine($"usbip interval {cmd.interval}");
                                Console.WriteLine($"usbip setup {cmd.setup}");
                                Console.WriteLine($"usbip buffer length {cmd.transfer_buffer_length}");

                                switch (cmd.command)
                                {
                                    case 1:
                                        USBIP_RET_SUBMIT usb_req = new USBIP_RET_SUBMIT
                                        {
                                            command = 0,
                                            seqnum = cmd.seqnum,
                                            devid = cmd.devid,
                                            direction = cmd.direction,
                                            ep = cmd.ep,
                                            status = 0,
                                            actual_length = 0,
                                            start_frame = 0,
                                            number_of_packets = 0,
                                            error_count = 0,
                                            setup = cmd.setup,
                                        };

                                        handle_usb_request(clntSocket, usb_req, cmd.transfer_buffer_length);
                                        break;

                                    case 2: // unlink urb
                                        break;

                                    default:
                                        Console.WriteLine($"Unknown USBIP cmd!");
                                        doLoop = false;
                                        clntSocket.Close();
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void handle_usb_request(Socket clntSocket, USBIP_RET_SUBMIT usb_req, int transfer_buffer_length)
        {
            if (usb_req.ep == 0)
            {
                Console.WriteLine("#control requests");
                handle_usb_control(clntSocket, usb_req);
            }
            else
            {
                Console.WriteLine("#data requests");
                handle_data(clntSocket, usb_req, transfer_buffer_length);
            }
        }

        ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
        public void Send(byte [] buf)
        {
            _queue.Enqueue(buf);
        }

        void handle_data(Socket clntSocket, USBIP_RET_SUBMIT usb_req, int bl)
        {
            byte[] result = null;

            while (true)
            {
                if (_queue.TryDequeue(out result) == false)
                {
                    Thread.Sleep(16);
                    continue;
                }

                break;
            }

            send_usb_req(clntSocket, usb_req, result, (uint)result.Length, 0);
        }

        void handle_usb_control(Socket clntSocket, USBIP_RET_SUBMIT usb_req)
        {
            Console.WriteLine($"setup: {usb_req.setup}");

            int handled = 0;
            StandardDeviceRequest control_req = new StandardDeviceRequest();

            control_req.bmRequestType = (byte)(((ulong)usb_req.setup & 0xFF00000000000000) >> 56);
            control_req.bRequest = (byte)(((ulong)usb_req.setup & 0x00FF000000000000) >> 48);
            control_req.wValue0 = (byte)(((ulong)usb_req.setup & 0x0000FF0000000000) >> 40);
            control_req.wValue1 = (byte)(((ulong)usb_req.setup & 0x000000FF00000000) >> 32);
            control_req.wIndex0 = (byte)(((ulong)usb_req.setup & 0x00000000FF000000) >> 24);
            control_req.wIndex1 = (byte)(((ulong)usb_req.setup & 0x0000000000FF0000) >> 16);
            control_req.wLength = IPAddress.NetworkToHostOrder((short)(usb_req.setup & 0x000000000000FFFF));
            Console.WriteLine($"  UC Request Type {control_req.bmRequestType}");
            Console.WriteLine($"  UC Request {control_req.bRequest}");
            Console.WriteLine($"  UC Value  {control_req.wValue1}[{control_req.wValue0}]");
            Console.WriteLine($"  UCIndex  {control_req.wIndex1 }-{control_req.wIndex0}");
            Console.WriteLine($"  UC Length {control_req.wLength }");

            if (control_req.bmRequestType == 0x80) // Host Request
            {
                if (control_req.bRequest == 0x06) // Get Descriptor
                {
                    handled = handle_get_descriptor(clntSocket, _desc, _configuration_hid, control_req, usb_req);
                }
                if (control_req.bRequest == 0x00) // Get STATUS
                {
                    byte [] data = new byte[2];
                    data[0] = 0x01;
                    data[1] = 0x00;
                    send_usb_req(clntSocket, usb_req, data, 2, 0);
                    handled = 1;
                    Console.WriteLine("GET_STATUS\n");
                }
            }
            if (control_req.bmRequestType == 0x00) // 
            {
                if (control_req.bRequest == 0x09) // Set Configuration
                {
                    handled = handle_set_configuration(clntSocket, control_req, usb_req);
                }
            }
            if (control_req.bmRequestType == 0x01)
            {
                if (control_req.bRequest == 0x0B) //SET_INTERFACE  
                {
                    Console.WriteLine("SET_INTERFACE");
                    send_usb_req(clntSocket, usb_req, null, 0, 1);
                    handled = 1;
                }

                _clntSocket = clntSocket;
            }

            if (handled == 0)
            {
                handle_unknown_control(clntSocket, control_req, usb_req);
            }
        }

        void handle_unknown_control(Socket clntSocket, StandardDeviceRequest control_req, USBIP_RET_SUBMIT usb_req)
        {
            if (control_req.bmRequestType == 0x81)
            {
                if (control_req.bRequest == 0x6)  //# Get Descriptor
                {
                    if (control_req.wValue1 == 0x22)  // send initial report
                    {
                        Console.WriteLine("send initial report");
                        send_usb_req(clntSocket, usb_req, _report_descriptor, (uint)_report_descriptor.Length, 0);
                    }
                }
            }

            if (control_req.bmRequestType == 0x21)  // Host Request
            {
                if (control_req.bRequest == 0x0a)  // set idle
                {
                    Console.WriteLine("Idle");
                    send_usb_req(clntSocket, usb_req, null, 0, 0);
                }
                if (control_req.bRequest == 0x09)  // set report
                {
                    Console.WriteLine("set report");
                    
                    byte [] data = new byte[20];
                    if (clntSocket.Receive(data, control_req.wLength, 0) != control_req.wLength)
                    {
                        Console.WriteLine("receive error : {errno}");
                        Environment.Exit(-1);
                    };

                    send_usb_req(clntSocket, usb_req, null, 0, 0);
                }
            }
        }

        unsafe byte [] StructureToBytes<T>(T data) where T : unmanaged
        {
            int size = sizeof(T);
            byte[] buf = new byte[size];

            IntPtr ptrElem = Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0);
            Marshal.StructureToPtr<T>(data, ptrElem, true);

            return buf;
        }

        unsafe int handle_get_descriptor(Socket clntSocket, USB_DEVICE_DESCRIPTOR desc, CONFIG_HID configuration_hid, StandardDeviceRequest control_req, USBIP_RET_SUBMIT usb_req)
        {
            int handled = 0;
            Console.WriteLine($"handle_get_descriptor {control_req.wValue1} [{control_req.wValue0}]");
            if (control_req.wValue1 == 0x1) // Device
            {
                Console.WriteLine("Device");
                handled = 1;

                byte [] buf = StructureToBytes(desc);
                send_usb_req(clntSocket, usb_req, buf, (uint)sizeof(USB_DEVICE_DESCRIPTOR)/*control_req->wLength*/, 0);
            }
            if (control_req.wValue1 == 0x2) // configuration
            {
                Console.WriteLine("Configuration\n");
                handled = 1;
                byte[] buf = StructureToBytes(configuration_hid);
                send_usb_req(clntSocket, usb_req, buf, (uint)control_req.wLength, 0);
            }
            if (control_req.wValue1 == 0x3) // string
            {
                /*
                byte [] str = new byte[255];
                int i;
                for (i = 0; i < (*strings[control_req->wValue0] / 2) - 1; i++)
                    str[i] = strings[control_req->wValue0][i * 2 + 2];
                Console.WriteLine("String (%s)\n", str);
                handled = 1;
                send_usb_req(clntSocket, usb_req, (char*)strings[control_req->wValue0], *strings[control_req->wValue0], 0);
                */
            }
            if (control_req.wValue1 == 0x6) // qualifier
            {
                Console.WriteLine("Qualifier");
                handled = 1;
                byte[] buf = StructureToBytes(dev_qua);
                send_usb_req(clntSocket, usb_req, buf, (uint)control_req.wLength, 0);
            }
            if (control_req.wValue1 == 0xA) // config status ???
            {
                Console.WriteLine("Unknow");
                handled = 1;
                send_usb_req(clntSocket, usb_req, null, 0, 1);
            }
            return handled;
        }

        int handle_set_configuration(Socket clntSocket, StandardDeviceRequest control_req, USBIP_RET_SUBMIT usb_req)
        {
            int handled = 0;
            Console.WriteLine($"handle_set_configuration {control_req.wValue1}[{control_req.wValue0}]");
            handled = 1;
            send_usb_req(clntSocket, usb_req, null, 0, 0);
            return handled;
        }

        unsafe void send_usb_req(Socket clntSocket, USBIP_RET_SUBMIT usb_req, byte [] data, uint size, uint status)
        {
            usb_req.command = 0x3;
            usb_req.status = (int)status;
            usb_req.actual_length = (int)size;
            usb_req.start_frame = 0x0;
            usb_req.number_of_packets = 0x0;

            usb_req.setup = 0x0;
            usb_req.devid = 0x0;
            usb_req.direction = 0x0;
            usb_req.ep = 0x0;

            byte [] buf = StructureToBytes(usb_req);
            fixed (byte* pByte = buf)
            {
                int* intBytes = (int*)pByte;
                HelperExtension.pack((int*)intBytes, sizeof(USBIP_RET_SUBMIT));
            }

            if (clntSocket.Send(buf, buf.Length, SocketFlags.None) != sizeof(USBIP_RET_SUBMIT))
            {
                Console.WriteLine($"send error");
                Environment.Exit(-1);
            };

            if (size > 0)
            {
                if (clntSocket.Send(data, (int)size, SocketFlags.None) != size)
                {
                    Console.WriteLine($"send error");
                    Environment.Exit(-1);
                };
            }
        }

        private unsafe bool AttachDevice(Socket clntSocket, string busId)
        {
            OP_REP_IMPORT rep = new OP_REP_IMPORT
            {
                version = IPAddress.HostToNetworkOrder(USBIP_PROTOCOL_VERSION),
                command = IPAddress.HostToNetworkOrder((short)UsbIpCommandType.RESP_IMPORT),
                status = 0,

                busnum = IPAddress.HostToNetworkOrder(1),
                devnum = IPAddress.HostToNetworkOrder(2),
                speed = IPAddress.HostToNetworkOrder(2),

                idVendor = _desc.idVendor,
                idProduct = _desc.idProduct,

                bcdDevice = _desc.bcdDevice,
                bDeviceClass = _desc.bDeviceClass,
                bDeviceSubClass = _desc.bDeviceSubClass,
                bDeviceProtocol = _desc.bDeviceProtocol,
                bNumConfigurations = _desc.bNumConfigurations,
                bConfigurationValue = _configuration_hid.dev_conf.bConfigurationValue,
                bNumInterfaces = _configuration_hid.dev_conf.bNumInterfaces,
            };

            byte[] buf = Encoding.ASCII.GetBytes($"/sys/devices/pci0000:00/0000:00:01.2/usb1/{busId}");
            Marshal.Copy(buf, 0, new IntPtr(rep.usbPath), buf.Length);

            buf = Encoding.ASCII.GetBytes(busId);
            Marshal.Copy(buf, 0, new IntPtr(rep.busID), buf.Length);

            return clntSocket.SendAs(rep);
        }
    }
}
