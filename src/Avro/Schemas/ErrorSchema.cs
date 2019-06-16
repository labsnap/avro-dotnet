using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avro.Schemas
{
    public class ErrorSchema : RecordSchema
    {
        public ErrorSchema()
            : base() { }

        public ErrorSchema(string name)
            : base(name) { }

        public ErrorSchema(string name, string ns)
            : base(name, ns) { }

        public ErrorSchema(IEnumerable<Field> fields)
            : base(fields) { }

        public ErrorSchema(string name, IEnumerable<Field> fields)
            : base(name, fields) { }

        public ErrorSchema(string name, string ns, IEnumerable<Field> fields)
            : base(name, ns, fields) { }
    }
}
