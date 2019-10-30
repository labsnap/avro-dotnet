namespace Avro.Schema
{
    public sealed class TimestampMillisSchema : LogicalSchema
    {
        public TimestampMillisSchema()
            : this(new LongSchema()) { }

        public TimestampMillisSchema(AvroSchema type)
            : base(type, "timestamp-millis")
        {
            if (!(type is LongSchema))
                throw new AvroParseException("Expected 'long' as type for logical type 'timestamp-micros'");
        }
    }
}
