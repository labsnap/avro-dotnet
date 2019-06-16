using Avro.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace Avro.IO
{
    public sealed class BinaryDecoder : IDecoder
    {
        private Stream _stream;

        public BinaryDecoder(Stream stream)
        {
            _stream = stream;
        }

        public IList<T> ReadArray<T>(Func<IDecoder, T> itemsReader)
        {
            var array = new List<T>();
            var len = 0L;
            do
            {
                len = ReadLong();
                if (len < 0)
                    len = ReadLong();
                for (int i = 0; i < len; i++)
                {
                    var value = itemsReader.Invoke(this);
                    array.Add(value);
                }
            }
            while (len != 0);
            return array;
        }

        public bool ReadBoolean()
        {
            var b = (byte)_stream.ReadByte();
            if (b == 0)
                return false;
            return true;
        }

        public byte[] ReadBytes()
        {
            var len = ReadLong();
            var bytes = new byte[len];
            _stream.Read(bytes, 0, (int)len);
            return bytes;
        }

        public DateTime ReadDate()
        {
            var days = ReadInt();
            return Constants.UNIX_EPOCH.AddDays(days);
        }

        public byte[] ReadBytes(byte[] bytes)
        {
            var len = ReadLong();
            if (len > bytes.LongLength)
                throw new IndexOutOfRangeException("Stream range exceeds input array.");
            _stream.Read(bytes, 0, (int)len);
            return bytes;
        }

        public decimal ReadDecimal(int scale)
        {
            var bytes = ReadBytes();
            var unscaled = new BigInteger(bytes.AsSpan(), isBigEndian: true);
            var value = (decimal)unscaled / (decimal)Math.Pow(10, scale);
            return value;
        }

        public decimal ReadDecimal(int scale, int len)
        {
            var bytes = new byte[len];
            bytes = ReadFixed(bytes);
            var x = 1;
            while (bytes[x] != 0 && x < len)
                x++;
            var unscaled = new BigInteger(bytes.AsSpan(0, x), isBigEndian: true);
            var value = (decimal)unscaled / (decimal)Math.Pow(10, scale);
            return value;
        }

        public double ReadDouble()
        {
            var bytes = new byte[8];
            _stream.Read(bytes, 0, 8);
            var bits =
                (bytes[0] & 0xFFL) |
                ((bytes[1] & 0xFFL) << 8) |
                ((bytes[2] & 0xFFL) << 16) |
                ((bytes[3] & 0xFFL) << 24) |
                ((bytes[4] & 0xFFL) << 32) |
                ((bytes[5] & 0xFFL) << 40) |
                ((bytes[6] & 0xFFL) << 48) |
                ((bytes[7] & 0xFFL) << 56)
            ;
            return BitConverter.Int64BitsToDouble(bits);
        }

        public Tuple<int, int, int> ReadDuration()
        {
            var mm = ReadInt();
            var dd = ReadInt();
            var ms = ReadInt();
            return new Tuple<int, int, int>(mm, dd, ms);
        }
        
        public byte[] ReadFixed(int len)
        {
            var bytes = new byte[len];
            _stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        public byte[] ReadFixed(byte[] bytes)
        {
            _stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        public float ReadFloat()
        {
            var bytes = new byte[4];
            _stream.Read(bytes, 0, 4);
            var bits =
                (bytes[0] & 0xFF) |
                ((bytes[1] & 0xFF) << 8) |
                ((bytes[2] & 0xFF) << 16) |
                ((bytes[3] & 0xFF) << 24)
            ;
            return BitConverter.Int32BitsToSingle(bits);
        }
        
        public int ReadInt()
        {
            var b = (byte)_stream.ReadByte();
            var n = b & 0x7FU;
            var shift = 7;
            while ((b & 0x80) != 0)
            {
                b = (byte)_stream.ReadByte();
                n |= (b & 0x7FU) << shift;
                shift += 7;
            }
            var value = (int)n;
            return (-(value & 0x01)) ^ ((value >> 1) & 0x7FFFFFFF);
        }

        public long ReadLong()
        {
            var b = (byte)_stream.ReadByte();
            var n = b & 0x7FUL;
            var shift = 7;
            while ((b & 0x80) != 0)
            {
                b = (byte)_stream.ReadByte();
                n |= (b & 0x7FUL) << shift;
                shift += 7;
            }
            var value = (long)n;
            return (-(value & 0x01L)) ^ ((value >> 1) & 0x7FFFFFFFFFFFFFFFL);
        }

        public IDictionary<string, T> ReadMap<T>(Func<IDecoder, T> valuesReader)
        {
            var map = new Dictionary<string, T>() as IDictionary<string, T>;
            var len = 0L;
            do
            {
                len = ReadLong();
                if (len < 0)
                    len = ReadLong();
                for (int i = 0; i < len; i++)
                {
                    var key = ReadString();
                    var value = valuesReader.Invoke(this);
                    map.Add(key, value);
                }
            }
            while (len != 0);
            return map;
        }

        public object ReadNull()
        {
            return null;
        }

        public T ReadNullableObject<T>(Func<IDecoder, T> reader, long nullIndex) where T : class
        {
            var index = ReadLong();
            if (index == nullIndex)
                return null;
            return reader.Invoke(this);
        }

        public T? ReadNullableValue<T>(Func<IDecoder, T> reader, long nullIndex) where T : struct
        {
            var index = ReadLong();
            if (index == nullIndex)
                return null;
            return reader.Invoke(this);
        }

        public string ReadString()
        {
            var bytes = ReadBytes();
            return Encoding.UTF8.GetString(bytes);
        }

        public TimeSpan ReadTimeMS()
        {
            var val = ReadInt();
            return TimeSpan.FromMilliseconds(val);
        }

        public TimeSpan ReadTimeUS()
        {
            var val = ReadLong();
            return TimeSpan.FromTicks(val * TimeSpan.TicksPerMillisecond / 1000);
        }

        public TimeSpan ReadTimeNS()
        {
            var val = ReadLong();
            return TimeSpan.FromTicks(val);
        }

        public DateTime ReadTimestampMS()
        {
            var val = ReadLong();
            return Constants.UNIX_EPOCH.AddMilliseconds(val);
        }

        public DateTime ReadTimestampUS()
        {
            var val = ReadLong();
            return Constants.UNIX_EPOCH.AddTicks(val * (TimeSpan.TicksPerMillisecond / 1000));
        }

        public DateTime ReadTimestampNS()
        {
            var val = ReadLong();
            return Constants.UNIX_EPOCH.AddTicks(val);
        }

        public object ReadUnion(Func<IDecoder, object>[] readers)
        {
            var index = ReadLong();
            return readers[index].Invoke(this);
        }

        public Guid ReadUuid()
        {
            var s = ReadString();
            return Guid.Parse(s);
        }

        public void SkipArray(Action<IDecoder> itemsSkipper)
        {
            var len = 0L;
            do
            {
                len = ReadLong();
                if (len < 0)
                {
                    SkipFixed(-1 * len);
                    continue;
                }

                for (int i = 0; i < len; i++)
                    itemsSkipper.Invoke(this);
            }
            while (len != 0);
        }

        public void SkipBoolean()
        {
            _stream.Seek(1, SeekOrigin.Current);
        }

        public void SkipBytes()
        {
            var len = ReadLong();
            _stream.Seek(len, SeekOrigin.Current);
        }
        
        public void SkipDate()
        {
            SkipInt();
        }

        public void SkipDecimal()
        {
            SkipBytes();
        }

        public void SkipDecimal(int len)
        {
            SkipFixed(len);
        }

        public void SkipDouble()
        {
            _stream.Seek(8, SeekOrigin.Current);
        }

        public void SkipDuration()
        {
            SkipFixed(12);
        }

        public void SkipFixed(long len)
        {
            _stream.Seek(len, SeekOrigin.Current);
        }

        public void SkipFloat()
        {
            _stream.Seek(4, SeekOrigin.Current);
        }

        public void SkipInt()
        {
            var b = (byte)_stream.ReadByte();
            while ((b & 0x80) != 0)
                b = (byte)_stream.ReadByte();
        }

        public void SkipLong()
        {
            var b = (byte)_stream.ReadByte();
            while ((b & 0x80) != 0)
                b = (byte)_stream.ReadByte();
        }

        public void SkipMap(Action<IDecoder> valuesSkipper)
        {
            var len = 0L;
            do
            {
                len = ReadLong();
                if (len < 0)
                {
                    SkipFixed(-1 * len);
                    continue;
                }

                for (int i = 0; i < len; i++)
                {
                    SkipString();
                    valuesSkipper.Invoke(this);
                }
            }
            while (len != 0);
        }

        public void SkipNull() { }

        public void SkipNullable(Action<IDecoder> skipper, long nullIndex)
        {
            var index = ReadLong();
            if (index != nullIndex)
                skipper.Invoke(this);
        }

        public void SkipString()
        {
            var len = ReadLong();
            _stream.Seek(len, SeekOrigin.Current);
        }


        public void SkipTimeMS()
        {
            SkipInt();
        }

        public void SkipTimeUS()
        {
            SkipLong();
        }

        public void SkipTimeNS()
        {
            SkipLong();
        }

        public void SkipTimestampMS()
        {
            SkipLong();
        }

        public void SkipTimestampUS()
        {
            SkipLong();
        }

        public void SkipTimestampNS()
        {
            SkipLong();
        }

        public void SkipUnion(Action<IDecoder>[] skippers)
        {
            var index = ReadLong();
            skippers[index].Invoke(this);
        }

        public void SkipUuid()
        {
            SkipString();
        }

        public void Dispose()
        {
            _stream = null;
        }
    }
}