using System.Collections.Generic;

namespace Avro.Schemas
{
    public class LogicalSchema : AvroSchema
    {
        public LogicalSchema(AvroSchema type, string logicalType)
        {
            Type = type;
            LogicalType = logicalType;
        }

        public AvroSchema Type { get; set; }
        public string LogicalType { get; protected set; }
        public override string ToString() => LogicalType;
    }
}
