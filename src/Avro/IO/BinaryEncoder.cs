using Avro.Types;
using Avro.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace Avro.IO
{
    public class BinaryEncoder : IAvroEncoder
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;

        public BinaryEncoder(Stream stream, bool leaveOpen = true)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
        }

        public void WriteBoolean(bool value)
        {
            _stream.WriteByte((byte)(value ? 1 : 0));
        }

        public void WriteInt(int value)
        {
            var n = (uint)((value << 1) ^ (value >> 31));
            while ((n & ~0x7FU) != 0)
            {
                _stream.WriteByte((byte)((n & 0x7f) | 0x80));
                n >>= 7;
            }
            _stream.WriteByte((byte)n);
        }

        public void WriteLong(long value)
        {
            var n = (ulong)((value << 1) ^ (value >> 63));
            while ((n & ~0x7FUL) != 0)
            {
                _stream.WriteByte((byte)((n & 0x7f) | 0x80));
                n >>= 7;
            }
            _stream.WriteByte((byte)n);
        }

        public void WriteFloat(float value)
        {
            var bits = BitConverter.SingleToInt32Bits(value);
            var bytes = new byte[]
            {
                (byte)((bits) & 0xFF),
                (byte)((bits >> 8) & 0xFF),
                (byte)((bits >> 16) & 0xFF),
                (byte)((bits >> 24) & 0xFF)
            };
            _stream.Write(bytes, 0, 4);
        }

        public void WriteDouble(double value)
        {
            var bits = BitConverter.DoubleToInt64Bits(value);
            var bytes = new byte[]
            {
                (byte)((bits) & 0xFF),
                (byte)((bits >> 8) & 0xFF),
                (byte)((bits >> 16) & 0xFF),
                (byte)((bits >> 24) & 0xFF),
                (byte)((bits >> 32) & 0xFF),
                (byte)((bits >> 40) & 0xFF),
                (byte)((bits >> 48) & 0xFF),
                (byte)((bits >> 56) & 0xFF)
            };
            _stream.Write(bytes, 0, 8);
        }

        public void WriteBytes(byte[] value)
        {
            WriteLong(value.LongLength);
            _stream.Write(value, 0, value.Length);
        }

        public void WriteFixed(byte[] value)
        {
            _stream.Write(value, 0, value.Length);
        }

        public void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteDecimal(decimal value, int scale)
        {
            var bytes = new BigInteger(value * (long)Math.Pow(10, scale)).ToByteArray(isBigEndian: true);
            WriteBytes(bytes);
        }

        public void WriteDecimal(decimal value, int scale, int len)
        {
            var bytes = new BigInteger(value * (long)Math.Pow(10, scale)).ToByteArray(isBigEndian: true);
            var fix = new byte[len];
            var offset = len - bytes.Length;
            if (value < 0)
                Array.Fill(fix, (byte)0xFF, 0, offset);
            bytes.CopyTo(fix, offset);
            WriteFixed(fix);
        }

        public void WriteDate(DateTime value)
        {
            var days = (value - Constants.UNIX_EPOCH).Days;
            WriteInt(days);
        }

        public void WriteTimeMS(TimeSpan value)
        {
            var time = value.Subtract(TimeSpan.FromDays(value.Days));
            var milliseconds = (int)Math.Round(time.TotalMilliseconds);
            WriteInt(milliseconds);
        }

        public void WriteTimeUS(TimeSpan value)
        {
            var time = value.Subtract(TimeSpan.FromDays(value.Days));
            var microseconds = time.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
            WriteLong(microseconds);
        }

        public void WriteTimeNS(TimeSpan value)
        {
            var time = value.Subtract(TimeSpan.FromDays(value.Days));
            var nanosecond = time.Ticks;
            WriteLong(nanosecond);
        }

        public void WriteTimestampMS(DateTime value)
        {
            var time = (value - Constants.UNIX_EPOCH);
            var milliseconds = (long)Math.Floor(time.TotalMilliseconds);
            WriteLong(milliseconds);
        }

        public void WriteTimestampUS(DateTime value)
        {
            var time = (value - Constants.UNIX_EPOCH);
            var microseconds = time.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
            WriteLong(microseconds);
        }

        public void WriteTimestampNS(DateTime value)
        {
            var time = (value - Constants.UNIX_EPOCH);
            var nanosecond = time.Ticks;
            WriteLong(nanosecond);
        }

        public void WriteDuration(AvroDuration value)
        {
            _stream.WriteByte((byte)((value.Months >> 24) & 0xFF));
            _stream.WriteByte((byte)((value.Months >> 16) & 0xFF));
            _stream.WriteByte((byte)((value.Months >> 8) & 0xFF));
            _stream.WriteByte((byte)((value.Months) & 0xFF));

            _stream.WriteByte((byte)((value.Days >> 24) & 0xFF));
            _stream.WriteByte((byte)((value.Days >> 16) & 0xFF));
            _stream.WriteByte((byte)((value.Days >> 8) & 0xFF));
            _stream.WriteByte((byte)((value.Days) & 0xFF));

            _stream.WriteByte((byte)((value.MilliSeconds >> 24) & 0xFF));
            _stream.WriteByte((byte)((value.MilliSeconds >> 16) & 0xFF));
            _stream.WriteByte((byte)((value.MilliSeconds >> 8) & 0xFF));
            _stream.WriteByte((byte)((value.MilliSeconds) & 0xFF));
        }

        public void WriteUuid(Guid value)
        {
            var s = value.ToString();
            WriteString(s);
        }

        public void WriteArray<T>(IList<T> items, Action<IAvroEncoder, T> itemsWriter)
        {
            if (items.Count > 0)
            {
                WriteLong(items.Count);
                foreach (var item in items)
                    itemsWriter.Invoke(this, item);
            }
            WriteLong(0);
        }

        public void WriteArrayBlock<T>(IList<T> items, Action<IAvroEncoder, T> itemsWriter)
        {
            if (items.Count == 0)
            {
                WriteLong(0);
                return;
            }

            var localEncoder = new BinaryEncoder(new MemoryStream());
            localEncoder.WriteLong(items.Count);
            foreach (var item in items)
                itemsWriter.Invoke(localEncoder, item);
            WriteLong(-1 * localEncoder._stream.Length);
            localEncoder._stream.Seek(0, SeekOrigin.Begin);
            localEncoder._stream.CopyTo(_stream);
        }

        public void WriteMap<T>(IDictionary<string, T> keyValues, Action<IAvroEncoder, T> valuesWriter)
        {
            if (keyValues.Count > 0)
            {
                WriteLong(keyValues.Count);
                foreach (var keyValue in keyValues)
                {
                    WriteString(keyValue.Key);
                    valuesWriter.Invoke(this, keyValue.Value);
                }
            }
            WriteLong(0);
        }

        public void WriteMapBlock<T>(IDictionary<string, T> keyValues, Action<IAvroEncoder, T> valuesWriter)
        {
            if (keyValues.Count == 0)
            {
                WriteLong(0);
                return;
            }

            var localEncoder = new BinaryEncoder(new MemoryStream());
            localEncoder.WriteLong(keyValues.Count);
            foreach (var keyValue in keyValues)
            {
                localEncoder.WriteString(keyValue.Key);
                valuesWriter.Invoke(localEncoder, keyValue.Value);
            }
            WriteLong(-1 * localEncoder._stream.Length);
            localEncoder._stream.Seek(0, SeekOrigin.Begin);
            localEncoder._stream.CopyTo(_stream);
        }

        public void WriteNull(AvroNull value) { }

        public void WriteNullableObject<T>(T value, Action<IAvroEncoder, T> valueWriter, long nullIndex) where T : class
        {
            if (value == null)
            {
                WriteLong(nullIndex % 2);
            }
            else
            {
                WriteLong((nullIndex + 1) % 2);
                valueWriter.Invoke(this, value);
            }
        }

        public void WriteNullableValue<T>(T? value, Action<IAvroEncoder, T> valueWriter, long nullIndex) where T : struct
        {
            if (value.HasValue)
            {
                WriteLong((nullIndex + 1) % 2);
                valueWriter.Invoke(this, value.Value);
            }
            else
            {
                WriteLong(nullIndex % 2);
            }
        }

        public void WriteUnion<T1, T2>(
            AvroUnion<T1, T2> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3>(
            AvroUnion<T1, T2, T3> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3, T4>(
            AvroUnion<T1, T2, T3, T4> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3,
            Action<IAvroEncoder, T4> valueWriter4
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
                case 3:
                    valueWriter4.Invoke(this, value.GetT4());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3, T4, T5>(
            AvroUnion<T1, T2, T3, T4, T5> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3,
            Action<IAvroEncoder, T4> valueWriter4,
            Action<IAvroEncoder, T5> valueWriter5
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
                case 3:
                    valueWriter4.Invoke(this, value.GetT4());
                    break;
                case 4:
                    valueWriter5.Invoke(this, value.GetT5());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3, T4, T5, T6>(
            AvroUnion<T1, T2, T3, T4, T5, T6> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3,
            Action<IAvroEncoder, T4> valueWriter4,
            Action<IAvroEncoder, T5> valueWriter5,
            Action<IAvroEncoder, T6> valueWriter6
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
                case 3:
                    valueWriter4.Invoke(this, value.GetT4());
                    break;
                case 4:
                    valueWriter5.Invoke(this, value.GetT5());
                    break;
                case 5:
                    valueWriter6.Invoke(this, value.GetT6());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3, T4, T5, T6, T7>(
            AvroUnion<T1, T2, T3, T4, T5, T6, T7> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3,
            Action<IAvroEncoder, T4> valueWriter4,
            Action<IAvroEncoder, T5> valueWriter5,
            Action<IAvroEncoder, T6> valueWriter6,
            Action<IAvroEncoder, T7> valueWriter7
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
                case 3:
                    valueWriter4.Invoke(this, value.GetT4());
                    break;
                case 4:
                    valueWriter5.Invoke(this, value.GetT5());
                    break;
                case 5:
                    valueWriter6.Invoke(this, value.GetT6());
                    break;
                case 6:
                    valueWriter7.Invoke(this, value.GetT7());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3, T4, T5, T6, T7, T8>(
            AvroUnion<T1, T2, T3, T4, T5, T6, T7, T8> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3,
            Action<IAvroEncoder, T4> valueWriter4,
            Action<IAvroEncoder, T5> valueWriter5,
            Action<IAvroEncoder, T6> valueWriter6,
            Action<IAvroEncoder, T7> valueWriter7,
            Action<IAvroEncoder, T8> valueWriter8
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
                case 3:
                    valueWriter4.Invoke(this, value.GetT4());
                    break;
                case 4:
                    valueWriter5.Invoke(this, value.GetT5());
                    break;
                case 5:
                    valueWriter6.Invoke(this, value.GetT6());
                    break;
                case 6:
                    valueWriter7.Invoke(this, value.GetT7());
                    break;
                case 7:
                    valueWriter8.Invoke(this, value.GetT8());
                    break;
            }
        }

        public void WriteUnion<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            AvroUnion<T1, T2, T3, T4, T5, T6, T7, T8, T9> value,
            Action<IAvroEncoder, T1> valueWriter1,
            Action<IAvroEncoder, T2> valueWriter2,
            Action<IAvroEncoder, T3> valueWriter3,
            Action<IAvroEncoder, T4> valueWriter4,
            Action<IAvroEncoder, T5> valueWriter5,
            Action<IAvroEncoder, T6> valueWriter6,
            Action<IAvroEncoder, T7> valueWriter7,
            Action<IAvroEncoder, T8> valueWriter8,
            Action<IAvroEncoder, T9> valueWriter9
        )
        {
            switch (value.Index)
            {
                case 0:
                    valueWriter1.Invoke(this, value.GetT1());
                    break;
                case 1:
                    valueWriter2.Invoke(this, value.GetT2());
                    break;
                case 2:
                    valueWriter3.Invoke(this, value.GetT3());
                    break;
                case 3:
                    valueWriter4.Invoke(this, value.GetT4());
                    break;
                case 4:
                    valueWriter5.Invoke(this, value.GetT5());
                    break;
                case 5:
                    valueWriter6.Invoke(this, value.GetT6());
                    break;
                case 6:
                    valueWriter7.Invoke(this, value.GetT7());
                    break;
                case 7:
                    valueWriter8.Invoke(this, value.GetT8());
                    break;
                case 8:
                    valueWriter9.Invoke(this, value.GetT9());
                    break;
            }
        }

        public void Dispose()
        {
            if (!_leaveOpen && _stream != null)
                _stream.Dispose();
        }
    }
}
