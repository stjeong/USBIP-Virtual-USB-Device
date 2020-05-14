using System;
using System.Collections.Generic;
using System.Text;

namespace UsbipDevice
{
    public enum MouseMode
    {
        None,
        Relative,
        Absolute,
        RelativeAndAbsolute,
    }

    public class MouseDevice : IDisposable
    {
        Usbip _device;
        int _screenSizeX;
        int _screenSizeY;
        byte[] _reportDescriptor;

        public bool Connected
        {
            get { return _device.Connected; }
        }

        bool _hasReportId = false;

        public MouseDevice(Usbip device, byte [] reportDescriptor, int screenSizeX, int screenSizeY)
        {
            _device = device;
            _reportDescriptor = reportDescriptor;
            _screenSizeX = screenSizeX;
            _screenSizeY = screenSizeY;

            _hasReportId = ReportDescEnumerator.HasReportId(_reportDescriptor);
        }

        public void Dispose()
        {
            _device.Dispose();
        }

        public void SendText(string text)
        {
            foreach (byte[] buffer in ConvertToMouseCommand(text))
            {
                _device.Send(buffer);
            }
        }

        internal List<byte[]> ConvertToMouseCommand(string txt)
        {
            List<byte[]> list = new List<byte[]>();

            string[] tokens = txt.Split(' ');

            for (int i = 0; i < tokens.Length; i++)
            {
                byte reportId = 0;

                byte[] keyDown = TokenToCommand(tokens, ref i, out reportId);
                if (keyDown == null)
                {
                    continue;
                }

                list.Add(keyDown);

                byte[] keyUp = new byte[ReportDescEnumerator.GetReportSize(_reportDescriptor, reportId)];
                keyUp[0] = reportId;

                list.Add(keyUp);
            }

            return list;
        }

        public byte GetReportId(bool isRelative)
        {
            if (_hasReportId == false)
            {
                return 0;
            }

            return (isRelative == true) ? MouseDescriptors.RelativeMouseReportId : MouseDescriptors.AbsoluteMouseReportId;
        }

        private byte[] TokenToCommand(string[] tokens, ref int tokenIndex, out byte reportId)
        {
            string token = tokens[tokenIndex];
            reportId = 0;

            if (string.IsNullOrEmpty(token) == true)
            {
                return null;
            }

            bool isRelative = (token[0] == '+' || token[0] == '-');

            if (token[0] == 'w')
            {
                if (short.TryParse(token.Substring(1), out short wheel) == true)
                {
                    reportId = GetReportId(false);
                    return AbsoluteBuffer(reportId, 0, 0, (byte)-wheel);
                }

                return null;
            }

            bool hasX = short.TryParse(token, out short xPos);
            if (hasX == true)
            {
                if (tokens.Length == tokenIndex + 1)
                {
                    return null;
                }

                token = tokens[tokenIndex + 1];
                bool hasY = short.TryParse(token, out short yPos);
                if (hasY == true)
                {
                    tokenIndex++;

                    if (isRelative == false)
                    {
                        isRelative = (token[0] == '+' || token[0] == '-');
                    }

                    reportId = GetReportId(isRelative);

                    if (isRelative == true)
                    {
                        byte[] buf = RelativeBuffer(reportId, 0, (byte)xPos, (byte)yPos);
                        return buf;
                    }
                    else
                    {
                        byte[] buf = AbsoluteBuffer(reportId, xPos, yPos, 0);
                        return buf;
                    }
                }
            }

            reportId = GetReportId(true);
            switch (token)
            {
                case "b1":
                    return RelativeBuffer(reportId, 0x01, 0x0, 0x0);

                case "b2":
                    return RelativeBuffer(reportId, 0x02, 0x0, 0x0);

                case "b3":
                    return RelativeBuffer(reportId, 0x04, 0x0, 0x0);
            }

            return null;
        }

        public byte[] RelativeBuffer(byte reportId, byte button, byte xPos, byte yPos)
        {
            byte[] buffer = new byte[ReportDescEnumerator.GetReportSize(_reportDescriptor, reportId)];
            int idx = 0;

            if (reportId != 0)
            {
                buffer[idx++] = reportId;
            }

            buffer[idx++] = button;
            buffer[idx++] = xPos;
            buffer[idx++] = yPos;

            return buffer;
        }

        public byte[] AbsoluteBuffer(byte reportId, int xPos, int yPos, byte wheel)
        {
            int x = (int)(xPos * 32767 / _screenSizeX);
            int y = (int)(yPos * 32767 / _screenSizeY);

            byte[] buffer = new byte[ReportDescEnumerator.GetReportSize(_reportDescriptor, reportId)];

            buffer[0] = reportId;
            buffer[1] = (byte)(x & 0xff);
            buffer[2] = (byte)((x & 0xff00) >> 8);
            buffer[3] = (byte)(y & 0xff);
            buffer[4] = (byte)((y & 0xff00) >> 8);

            if (buffer.Length > 5)
            {
                buffer[5] = wheel;
            }

            return buffer;
        }
    }
}
