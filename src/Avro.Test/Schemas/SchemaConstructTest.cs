using Avro;
using Avro.Schemas;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Avro.Test.Schemas
{
    [TestFixture()]
    public class SchemaConstructTest
    {
        [Test, TestCaseSource(typeof(NoExeptionStructors))]
        public void SchemaConstructOK(Func<Schema> invoker)
        {
            Assert.DoesNotThrow(() => invoker.Invoke());
        }

        [Test, TestCaseSource(typeof(ExeptionStructors))]
        public void SchemaConstructError(Action invoker, Type exceptionType)
        {
            Assert.Throws(exceptionType, () => invoker.Invoke());
        }

        class NoExeptionStructors : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new Func<Schema>(() => new DateSchema());
                yield return new Func<Schema>(() => new DateSchema(new IntSchema()));

                yield return new Func<Schema>(() => new TimeMillisSchema());
                yield return new Func<Schema>(() => new TimeMillisSchema(new IntSchema()));
                yield return new Func<Schema>(() => new TimeMicrosSchema());
                yield return new Func<Schema>(() => new TimeMicrosSchema(new LongSchema()));
                yield return new Func<Schema>(() => new TimeNanosSchema());
                yield return new Func<Schema>(() => new TimeNanosSchema(new LongSchema()));

                yield return new Func<Schema>(() => new TimestampMillisSchema());
                yield return new Func<Schema>(() => new TimestampMillisSchema(new LongSchema()));
                yield return new Func<Schema>(() => new TimestampMicrosSchema());
                yield return new Func<Schema>(() => new TimestampMicrosSchema(new LongSchema()));
                yield return new Func<Schema>(() => new TimestampNanosSchema());
                yield return new Func<Schema>(() => new TimestampNanosSchema(new LongSchema()));

                yield return new Func<Schema>(() => new DecimalSchema(new BytesSchema()));
                yield return new Func<Schema>(() => new DecimalSchema(20, 8));

                yield return new Func<Schema>(() => new RecordSchema());
                yield return new Func<Schema>(() => new RecordSchema("TestRecordSchema"));
                yield return new Func<Schema>(() => new RecordSchema("TestRecordSchema", "TestNamespace"));
                yield return new Func<Schema>(() => new RecordSchema("TestRecordSchema", new List<RecordSchema.Field>() { new RecordSchema.Field("X", new IntSchema()) }));
                yield return new Func<Schema>(() => new RecordSchema("TestRecordSchema", "TestNamespace", new List<RecordSchema.Field>() { new RecordSchema.Field("TestField") }));

                yield return new Func<Schema>(() => new ErrorSchema());
                yield return new Func<Schema>(() => new ErrorSchema("TestErrorSchema"));
                yield return new Func<Schema>(() => new ErrorSchema("TestErrorSchema", "TestNamespace"));
                yield return new Func<Schema>(() => new ErrorSchema("TestErrorSchema", new List<RecordSchema.Field>()));
                yield return new Func<Schema>(() => new ErrorSchema("TestErrorSchema", "TestNamespace", new List<RecordSchema.Field>() { new RecordSchema.Field("TestField") }));
            }
        }

        class ExeptionStructors : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { new Action(() => new DecimalSchema(new ArraySchema(new IntSchema()))), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new DecimalSchema(20, -1)), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new DecimalSchema(4, 8)), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new DecimalSchema(0, 0)), typeof(AvroParseException) };

                yield return new object[] { new Action(() => new DurationSchema(new IntSchema())), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new DurationSchema(new FixedSchema("WrongSize", null, 4))), typeof(AvroParseException) };

                yield return new object[] { new Action(() => new FixedSchema("TestFixed", "TestNamespace", -1)), typeof(AvroParseException) };

                yield return new object[] { new Action(() => new UuidSchema(new FloatSchema())), typeof(AvroParseException) };

                yield return new object[] { new Action(() => new DateSchema(new StringSchema())), typeof(AvroParseException) };

                yield return new object[] { new Action(() => new TimeMillisSchema(new NullSchema())), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new TimeMicrosSchema(new BooleanSchema())), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new TimeNanosSchema(new IntSchema())), typeof(AvroParseException) };

                yield return new object[] { new Action(() => new TimestampMillisSchema(new NullSchema())), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new TimestampMicrosSchema(new BooleanSchema())), typeof(AvroParseException) };
                yield return new object[] { new Action(() => new TimestampNanosSchema(new IntSchema())), typeof(AvroParseException) };
            }
        }
    }
}
