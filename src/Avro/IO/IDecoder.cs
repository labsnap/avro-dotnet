using System;
using System.Collections.Generic;

namespace Avro.IO
{
    public interface IDecoder : IDisposable
    {
        IList<T> ReadArray<T>(Func<IDecoder, T> itemsReader);
        bool ReadBoolean();
        byte[] ReadBytes();
        byte[] ReadBytes(byte[] bytes);
        DateTime ReadDate();
        decimal ReadDecimal(int scale);
        decimal ReadDecimal(int scale, int len);
        double ReadDouble();
        Tuple<int, int, int> ReadDuration();
        byte[] ReadFixed(byte[] bytes);
        byte[] ReadFixed(int len);
        float ReadFloat();
        int ReadInt();
        long ReadLong();
        IDictionary<string, T> ReadMap<T>(Func<IDecoder, T> valuesReader);
        object ReadNull();
        T ReadNullableObject<T>(Func<IDecoder, T> reader, long nullIndex) where T : class;
        T? ReadNullableValue<T>(Func<IDecoder, T> reader, long nullIndex) where T : struct;
        string ReadString();
        TimeSpan ReadTimeMS();
        TimeSpan ReadTimeNS();
        DateTime ReadTimestampMS();
        DateTime ReadTimestampNS();
        DateTime ReadTimestampUS();
        TimeSpan ReadTimeUS();
        object ReadUnion(Func<IDecoder, object>[] readers);
        Guid ReadUuid();
        void SkipArray(Action<IDecoder> itemsSkipper);
        void SkipBoolean();
        void SkipBytes();
        void SkipDate();
        void SkipDecimal();
        void SkipDecimal(int len);
        void SkipDouble();
        void SkipDuration();
        void SkipFixed(long len);
        void SkipFloat();
        void SkipInt();
        void SkipLong();
        void SkipMap(Action<IDecoder> valuesSkipper);
        void SkipNull();
        void SkipNullable(Action<IDecoder> skipper, long nullIndex);
        void SkipString();
        void SkipTimeMS();
        void SkipTimeNS();
        void SkipTimestampMS();
        void SkipTimestampNS();
        void SkipTimestampUS();
        void SkipTimeUS();
        void SkipUnion(Action<IDecoder>[] skippers);
        void SkipUuid();
    }
}