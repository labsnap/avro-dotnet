﻿using Avro.Generic;
using Avro.IO;
using Avro.Schemas;
using Avro.Types;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Avro.Test.IO
{
    [TestFixture]
    public class JsonEncoderTest
    {
        [TestCase]
        public void TestSimple()
        {
            var schema = AvroParser.ReadSchema(@"{
                ""name"":""Test.Foobar.Record.Thing"",
                ""type"":""record"",
                ""fields"":[
                    {""name"":""ID"",""type"":""int""},
                    {""name"":""Name"",""type"":""string""},
                    {""name"":""Tags"",""type"":{
                        ""type"":""array"",
                        ""items"":""string""
                    }},
                    {""name"":""Nuller"",""type"":""null""},
                    {""name"":""family"",""type"":{""type"":""map"",""values"":""string""}}
                ]
            }") as RecordSchema;

            var genericRecord = new GenericAvroRecord(schema);
            var reader = new GenericReader<GenericAvroRecord>(schema);
            var writer = new GenericWriter<GenericAvroRecord>(schema);


            var delimiter = ",";
            var stringBuilder = new StringBuilder();
            using (var stream = new StringWriter(stringBuilder))
            using (var encoder = new JsonEncoder(stream, schema, delimiter))
            {
                for (int i = 0; i < 10; i++)
                {
                    var record = new GenericAvroRecord(genericRecord);
                    record[0] = i;
                    record[1] = $"foo{i}";
                    record[2] = new List<object> { };
                    record[3] = null;
                    record[4] = new Dictionary<string, object>() { };
                    writer.Write(encoder, record);
                }
            }
            var json = stringBuilder.ToString();


            using (var stream = new StringReader(json))
            using (var decoder = new JsonDecoder(stream, schema, delimiter))
            {
                for (int i = 0; i < 10; i++)
                {
                    var record = reader.Read(decoder);
                    Debug.WriteLine(record.ToString());
                }
            }
        }
    }
}
