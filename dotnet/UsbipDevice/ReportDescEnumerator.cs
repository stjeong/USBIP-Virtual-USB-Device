using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UsbipDevice
{
    public enum ReportDescKey : byte
    {
        USAGE_PAGE = 0x05,
        USAGE = 0x09,
        UNIT_EXPONENT = 0x55,
        LOGICAL_MINIMUM_8 = 0x15,
        LOGICAL_MINIMUM_16 = 0x16,
        LOGICAL_MINIMUM_32 = 0x17,
        USAGE_MINIMUM = 0x19,
        LOGICAL_MAXIMUM_8 = 0x25,
        LOGICAL_MAXIMUM_16 = 0x26,
        LOGICAL_MAXIMUM_32 = 0x27,
        USAGE_MAXIMUM = 0x29,
        PHYSICAL_MINIMUM_8 = 0x35,
        PHYSICAL_MINIMUM_16 = 0x36,
        PHYSICAL_MAXIMUM_8 = 0x45,
        PHYSICAL_MAXIMUM_16 = 0x46,
        UNIT = 0x65,
        REPORT_SIZE = 0x75,
        INPUT = 0x81,
        REPORT_ID = 0x85,
        OUTPUT = 0x91,
        REPORT_COUNT = 0x95,
        COLLECTION = 0xa1,
        FEATURE = 0xb1,

        END_COLLECTION = 0xc0,
    }

    public class ReportDescEnumerator : IEnumerator<ReportItem>, IEnumerable<ReportItem>
    {
        byte[] _buffer;

        int _index;
        int _dataSize;
        ReportItem _current;

        public static bool HasReportId(byte[] buffer)
        {
            ReportDescEnumerator desc = new ReportDescEnumerator(buffer);

            foreach (ReportItem item in desc)
            {
                if (item.Key == ReportDescKey.REPORT_ID)
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetReportSize(byte[] buffer, int reportId)
        {
            ReportDescEnumerator desc = new ReportDescEnumerator(buffer);
            int bitCount = 0;
            int currentReportId = 0;
            int collectionDepth = 0;

            foreach (ReportItem item in desc)
            {
                if (item.Key == ReportDescKey.REPORT_SIZE)
                {
                    ReportItem nextItem = desc.PeekNext();

                    if (nextItem.Key == ReportDescKey.REPORT_COUNT)
                    {
                        bitCount += (item.Data8 * nextItem.Data8);
                    }
                }
                else if (item.Key == ReportDescKey.REPORT_COUNT)
                {
                    ReportItem nextItem = desc.PeekNext();

                    if (nextItem.Key == ReportDescKey.REPORT_SIZE)
                    {
                        bitCount += (item.Data8 * nextItem.Data8);
                    }
                }

                if (item.Key == ReportDescKey.COLLECTION)
                {
                    collectionDepth++;
                    continue;
                }
                else if (item.Key == ReportDescKey.END_COLLECTION)
                {
                    collectionDepth--;
                }

                if (item.Key == ReportDescKey.REPORT_ID)
                {
                    currentReportId = item.Data8;
                    bitCount += 8;
                    continue;
                }

                if (collectionDepth == 0)
                {
                    if (bitCount == 0)
                    {
                        continue;
                    }

                    if (currentReportId == reportId)
                    {
                        return bitCount / 8;
                    }

                    bitCount = 0;
                }
            }

            return bitCount / 8;
        }

        public ReportDescEnumerator(byte[] buffer)
        {
            _buffer = buffer;
            _index = 0;
        }

        public ReportItem Current
        {
            get { return _current; }
        }

        object IEnumerator.Current
        {
            get { return _current; }
        }

        public void Dispose()
        {
        }

        public ReportItem PeekNext()
        {
            int oldIndex = _index;
            bool hasNext = MoveNext();
            _index = oldIndex;

            if (hasNext == true)
            {
                return _current;
            }

            return default;
        }

        public bool MoveNext()
        {
            if (_index >= (_buffer.Length - (_dataSize + 1)))
            {
                return false;
            }

            byte key = _buffer[_index];
            _dataSize = (key & 0b0000_0011);

            byte[] data = null;

            if (_dataSize >= 1)
            {
                data = new byte[_dataSize];
                Array.Copy(_buffer, _index + 1, data, 0, data.Length);
            }

            _current = new ReportItem((ReportDescKey)key, data);

            _index += (_dataSize + 1);
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public IEnumerator<ReportItem> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }

    public struct ReportItem
    {
        ReportDescKey _key;
        public ReportDescKey Key => _key;

        byte[] _dataBuffer;
        public byte Data8
        {
            get { return _dataBuffer[0]; }
        }

        public short Data16
        {
            get { return BitConverter.ToInt16(_dataBuffer, 0); }
        }

        public ReportItem(ReportDescKey key, byte[] dataBuffer)
        {
            _key = key;
            _dataBuffer = dataBuffer;
        }

        public bool IsGenericDesktopPage
        {
            get
            {
                return (_key == ReportDescKey.USAGE_PAGE && Data8 == 0x01);
            }
        }
    }
}
