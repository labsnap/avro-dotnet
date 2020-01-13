#pragma warning disable CS8600, CS8601, CS8618 // Nullability warnings.

#pragma warning disable IDE1006, IDE0066 // Style warnings.

using Avro;
using Avro.Schema;
using Avro.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.apache.avro.ipc
{
    /// <summary></summary>
    [DataContract(Name = "MD5", Namespace = "org.apache.avro.ipc")]
    public class MD5 : IAvroFixed
    {
        public static readonly FixedSchema SCHEMA = AvroParser.ReadSchema<FixedSchema>("{\"name\":\"org.apache.avro.ipc.MD5\",\"type\":\"fixed\",\"size\":16}");
        public const int _SIZE = 16;
        private readonly byte[] _value;
        public MD5()
        {
            _value = new byte[_SIZE];
        }

        public MD5(byte[] value)
        {
            if (value.Length != _SIZE)
                throw new ArgumentException($"Array must be of size: {_SIZE}");
            _value = value;
        }

        public FixedSchema Schema => SCHEMA;
        public int Size => _SIZE;
        public byte this[int i]
        {
            get => _value[i];
            set => _value[i] = value;
        }

        public byte[] Value => _value;
        public bool Equals(IAvroFixed other)
        {
            if (Size != other.Size)
                return false;
            for (int i = 0; i < Size; i++)
                if (this[i] != other[i])
                    return false;
            return true;
        }

        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in _value)
                yield return b;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public static implicit operator MD5(byte[] value) => new MD5(value);
        public static implicit operator byte[](MD5 value) => value._value;
    }
}