using Avro.Serialization;

namespace Avro.Schema
{
    [SerializationType(typeof(decimal))]
    public sealed class DecimalSchema : LogicalSchema
    {
        public DecimalSchema()
            : this(new BytesSchema(), 15, 2) { }

        public DecimalSchema(AvroSchema underlyingType)
            : this(underlyingType, 15, 2) { }

        public DecimalSchema(int precision, int scale)
            : this(new BytesSchema(), precision, scale) { }

        public DecimalSchema(AvroSchema underlyingType, int precision, int scale)
            : base(underlyingType, "decimal")
        {
            if (!(underlyingType is BytesSchema || underlyingType is FixedSchema))
                throw new AvroParseException("Expected 'bytes' or 'fixed' as type for logical type 'decimal'");
            if (precision < 1)
                throw new AvroParseException("Decimal precision must be a postive non zero number");
            if (scale < 0 || scale > precision)
                throw new AvroParseException("Decimal scale must be a postive or zero and less or equal to precision");
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; set; }
        public int Scale { get; set; }

        public override string ToString()
        {
            return  $"{base.ToString()}({Precision},{Scale})";
        }

        public override bool Equals(AvroSchema obj)
        {
            return base.Equals(obj) &&
                (obj is DecimalSchema) &&
                (obj as DecimalSchema).Precision == Precision &&
                (obj as DecimalSchema).Scale == Scale
            ;
        }
    }
}
