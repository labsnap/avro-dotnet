﻿using Avro.Generic;
using Avro.Schemas;
using Avro.Types;
using NUnit.Framework;
using System;

namespace Avro.Test.Generic
{
    [TestFixture]
    public class GenericObjectTest
    {
        [TestCase]
        public void TestEnum()
        {
            var enumSchema = AvroParser.ReadSchema(@"{""name"":""TestEnum"",""type"":""enum"",""symbols"":[""A"",""B"",""C""]}") as EnumSchema;

            var enumInstance01 = new GenericAvroEnum(enumSchema);
            Assert.AreEqual(0, enumInstance01.Value);
            Assert.AreEqual("A", enumInstance01.Symbol);
            Assert.AreEqual("A", enumInstance01.ToString());

            var enumInstance02 = new GenericAvroEnum(enumSchema, 1);
            Assert.AreEqual(1, enumInstance02.Value);
            Assert.AreEqual("B", enumInstance02.Symbol);
            Assert.AreEqual("B", enumInstance02.ToString());

            var enumInstance03 = new GenericAvroEnum(enumSchema, "C");
            Assert.AreEqual(2, enumInstance03.Value);
            Assert.AreEqual("C", enumInstance03.Symbol);
            Assert.AreEqual("C", enumInstance03.ToString());

            Assert.Throws<IndexOutOfRangeException>(() => new GenericAvroEnum(enumSchema, -1));
            Assert.Throws<IndexOutOfRangeException>(() => new GenericAvroEnum(enumSchema, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GenericAvroEnum(enumSchema, "X"));
        }

        [TestCase]
        public void TestFixed()
        {
            var fixedSchema = AvroParser.ReadSchema(@"{""name"":""TestFixed"",""type"":""fixed"",""size"":12}") as FixedSchema;

            var genericFixed01 = new GenericAvroFixed(fixedSchema);
            Assert.AreEqual(12, genericFixed01.Size);
            Assert.AreEqual(new byte[12], genericFixed01);
            Assert.AreNotEqual(genericFixed01, new byte[10]);
            Assert.AreNotEqual(genericFixed01, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });


            var genericFixed02 = new GenericAvroFixed(fixedSchema, new byte[12] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3 });
            Assert.AreEqual(12, genericFixed02.Size);
            Assert.AreEqual(genericFixed02, new byte[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3 });

            Assert.AreEqual(genericFixed02, genericFixed02);
            Assert.AreNotEqual(genericFixed02, genericFixed01);

            Assert.Throws<ArgumentException>(() => new GenericAvroFixed(fixedSchema, new byte[6]));
        }

        [TestCase]
        public void TestRecord()
        {
            var recordSchema01 = AvroParser.ReadSchema(@"{""name"":""Avro.Test.Generic.TestRecord1"",""type"":""record"",""fields"":[{""name"":""ID"",""type"":""int""},{""name"":""Name"",""type"":""string""}]}") as RecordSchema;
            var recordSchema02 = AvroParser.ReadSchema(@"{""name"":""Avro.Test.Generic.TestRecord2"",""type"":""record"",""fields"":[{""name"":""ID"",""type"":""int""},{""name"":""Name"",""type"":""string""},{""name"":""Value"",""type"":""double"",""default"":4.5}]}") as RecordSchema;
            var recordSchema03 = AvroParser.ReadSchema(@"{""name"":""Avro.Test.Generic.TestRecord1"",""type"":""record"",""fields"":[{""name"":""NameX"",""type"":""string""},{""name"":""ID"",""type"":""int""}]}") as RecordSchema;

            var genericRecord01 = new GenericAvroRecord(recordSchema01);
            Assert.AreEqual(2, genericRecord01.Index.Count);
            Assert.AreEqual(0, genericRecord01.DefaultInitializers.Count);
            Assert.True(genericRecord01.Equals(genericRecord01));

            var genericRecord02 = new GenericAvroRecord(recordSchema02);
            Assert.AreEqual(3, genericRecord02.Index.Count);
            Assert.AreEqual(1, genericRecord02.DefaultInitializers.Count);
            Assert.False(genericRecord02.Equals(genericRecord01));

            var genericRecord03 = new GenericAvroRecord(recordSchema01, null, null);
            Assert.AreEqual(0, genericRecord03.Index.Count);
            Assert.AreEqual(0, genericRecord03.DefaultInitializers.Count);
            Assert.True(genericRecord03.Equals(genericRecord01));

            genericRecord01[1] = "X";
            Assert.False(genericRecord02.Equals(genericRecord01));
            Assert.False(genericRecord03.Equals(genericRecord01));

            genericRecord02[1] = "Y";
            genericRecord03[1] = "Y";
            Assert.False(genericRecord02.Equals(genericRecord01));
            Assert.False(genericRecord03.Equals(genericRecord01));

            var genericRecord04 = new GenericAvroRecord(genericRecord01);
            genericRecord04[1] = "Y";
            Assert.False(genericRecord04.Equals(genericRecord01));
        }
    }
}
