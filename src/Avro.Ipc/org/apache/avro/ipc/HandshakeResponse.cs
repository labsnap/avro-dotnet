using Avro;
using Avro.Schema;
using Avro.Types;
using System;
using System.Collections;
using System.Collections.Generic;

namespace org.apache.avro.ipc
{
    /// <summary></summary>
    [AvroNamedType("org.apache.avro.ipc", "HandshakeResponse")]
    public class HandshakeResponse : IAvroRecord
    {
        public static readonly RecordSchema _SCHEMA = AvroParser.ReadSchema<RecordSchema>("{\"name\":\"org.apache.avro.ipc.HandshakeResponse\",\"type\":\"record\",\"fields\":[{\"name\":\"match\",\"type\":{\"name\":\"org.apache.avro.ipc.HandshakeMatch\",\"type\":\"enum\",\"symbols\":[\"BOTH\",\"CLIENT\",\"NONE\"]}},{\"name\":\"serverProtocol\",\"type\":[\"null\",\"string\"]},{\"name\":\"serverHash\",\"type\":[\"null\",{\"name\":\"org.apache.avro.ipc.MD5\",\"type\":\"fixed\",\"size\":16}]},{\"name\":\"meta\",\"type\":[\"null\",{\"type\":\"map\",\"values\":\"bytes\"}]}]}");
        public RecordSchema Schema => _SCHEMA;
        public int FieldCount => 4;
        /// <summary></summary>
        [AvroField("match")]
        public HandshakeMatch match
        {
            get;
            set;
        }

        /// <summary></summary>
        [AvroField("serverProtocol")]
        public string serverProtocol
        {
            get;
            set;
        }

        /// <summary></summary>
        [AvroField("serverHash")]
        public MD5 serverHash
        {
            get;
            set;
        }

        /// <summary></summary>
        [AvroField("meta")]
        public IDictionary<string, byte[]> meta
        {
            get;
            set;
        }

        public object this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return match;
                    case 1:
                        return serverProtocol;
                    case 2:
                        return serverHash;
                    case 3:
                        return meta;
                    default:
                        throw new IndexOutOfRangeException("Expected range: [0:3].");
                }
            }

            set
            {
                switch (i)
                {
                    case 0:
                        match = (HandshakeMatch)value;
                        break;
                    case 1:
                        serverProtocol = (string)value;
                        break;
                    case 2:
                        serverHash = (MD5)value;
                        break;
                    case 3:
                        meta = (IDictionary<string, byte[]>)value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Expected range: [0:3].");
                }
            }
        }
    }
}