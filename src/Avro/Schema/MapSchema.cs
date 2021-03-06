using Avro.Serialization;
using System.Collections.Generic;

namespace Avro.Schema
{
    [SerializationType(typeof(IDictionary<,>), ReservedGenericArguments = new [] { typeof(string) })]
    public sealed class MapSchema : AvroSchema
    {
        public MapSchema(AvroSchema values)
        {
            Values = values;
        }

        public AvroSchema Values { get; set; }

        public override bool Equals(AvroSchema other)
        {
            return base.Equals(other) &&
                (other as MapSchema).Values.Equals(Values);
        }

        public override string ToString() => "map";
    }
}
