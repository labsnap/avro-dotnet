using Avro.Serialization;
using System;

namespace Avro.Schema
{
    [SerializationType(typeof(DateTime))]
    public sealed class TimestampMicrosSchema : LogicalSchema
    {
        public TimestampMicrosSchema()
            : this(new LongSchema()) { }

        public TimestampMicrosSchema(AvroSchema type)
            : base(type, "timestamp-micros")
        {
            if (!(type is LongSchema))
                throw new AvroParseException("Expected 'long' as type for logical type 'timestamp-micros'");
        }
    }
}
