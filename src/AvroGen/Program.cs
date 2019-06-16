using Avro.CodeDom;
using System;
using System.Collections.Generic;
using System.IO;

namespace Avro
{
    public class AvroGen
    {
        static void Main(string[] args)
        {
            // Print usage if no arguments provided or help requested
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                Usage();
                return;
            }

            // Parse command line arguments
            bool? isProtocol = null;
            string inputFile = null;
            string outputDir = null;
            var namespaceMapping = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-p")
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Missing path to protocol file");
                        Usage();
                        return;
                    }

                    isProtocol = true;
                    inputFile = args[++i];
                }
                else if (args[i] == "-s")
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Missing path to schema file");
                        Usage();
                        return;
                    }

                    isProtocol = false;
                    inputFile = args[++i];
                }
                else if (args[i] == "--namespace")
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("Missing namespace mapping");
                        Usage();
                        return;
                    }

                    var parts = args[++i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        Console.WriteLine("Malformed namespace mapping. Required format is \"avro.namespace:csharp.namespace\"");
                        Usage();
                        return;
                    }

                    namespaceMapping[parts[0]] = parts[1];
                }
                else if (outputDir == null)
                {
                    outputDir = args[i];
                }
                else
                {
                    Console.WriteLine("Unexpected command line argument: {0}", args[i]);
                    Usage();
                }
            }

            // Ensure we got all the command line arguments we need
            bool isValid = true;
            if (!isProtocol.HasValue || inputFile == null)
            {
                Console.WriteLine("Must provide either '-p <protocolfile>' or '-s <schemafile>'");
                isValid = false;
            }
            else if (outputDir == null)
            {
                Console.WriteLine("Must provide 'outputdir'");
                isValid = false;
            }

            if (!isValid)
                Usage();
            else if (isProtocol.Value)
                GenProtocol(inputFile, outputDir, namespaceMapping);
            else
                GenSchema(inputFile, outputDir, namespaceMapping);
        }

        static void Usage()
        {
            Console.WriteLine("{0}\n\n" +
                "Usage:\n" +
                "  avrogen -p <protocolfile> <outputdir> [--namespace <my.avro.ns:my.csharp.ns>]\n" +
                "  avrogen -s <schemafile> <outputdir> [--namespace <my.avro.ns:my.csharp.ns>]\n\n" +
                "Options:\n" +
                "  -h --help   Show this screen.\n" +
                "  --namespace Map an Avro schema/protocol namespace to a C# namespace.\n" +
                "              The format is \"my.avro.namespace:my.csharp.namespace\".\n" +
                "              May be specified multiple times to map multiple namespaces.\n",
                AppDomain.CurrentDomain.FriendlyName);
            return;
        }
        static void GenProtocol(string infile, string outdir, IEnumerable<KeyValuePair<string, string>> namespaceMapping)
        {
            //try
            //{
            //    string text = System.IO.File.ReadAllText(infile);
            //    Protocol protocol = Protocol.Parse(text);

            //    CodeGen codegen = new CodeGen();
            //    codegen.AddProtocol(protocol);

            //    foreach (var entry in namespaceMapping)
            //        codegen.NamespaceMapping[entry.Key] = entry.Value;

            //    codegen.GenerateCode();
            //    codegen.WriteTypes(outdir);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Exception occurred. " + ex.Message);
            //}
        }

        static void GenSchema(string infile, string outdir, IEnumerable<KeyValuePair<string, string>> namespaceMapping)
        {
            try
            {
                var text = File.ReadAllText(infile);
                var schema = Schema.Parse(text);

                var codegen = new CodeGen();
                //foreach (var entry in namespaceMapping)
                //    codegen.NamespaceMapping[entry.Key] = entry.Value;
                codegen.AddSchema(schema);
                codegen.CreateProject(outdir);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred. " + ex.Message);
            }
        }
    }
}
