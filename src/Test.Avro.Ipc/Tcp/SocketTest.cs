﻿using Avro.Ipc.Http;
using Avro.Ipc.IO;
using Avro.Ipc.Local;
using Avro.Ipc.Tcp;
using Avro.Schema;
using Avro.Types;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Avro.Ipc.Test.Tcp
{
    [TestFixture]
    public class SocketTest
    {

        private static readonly AvroProtocol HELLO_PROTOCOL = AvroParser.ReadProtocol(@"
        {
          ""namespace"": ""com.acme"",
          ""protocol"": ""HelloWorld"",
          ""doc"": ""Protocol Greetings"",

          ""types"": [
            {""name"": ""Greeting"", ""type"": ""record"", ""fields"": [
              {""name"": ""message"", ""type"": ""string""}]},
            {""name"": ""Curse"", ""type"": ""error"", ""fields"": [
              {""name"": ""message"", ""type"": ""string""}]}
          ],

          ""messages"": {
            ""hello"": {
              ""doc"": ""Say hello."",
              ""request"": [{""name"": ""greeting"", ""type"": ""Greeting"" }],
              ""response"": ""Greeting"",
              ""errors"": [""Curse""]
            }
          }
        }");

        [TestCase]
        public void TestHelloWorldLocal()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            var serverStream = new BlockingCollection<FrameStream>();
            var clientStream = new BlockingCollection<FrameStream>();
            var serverTransport = new LocalServer(serverStream, clientStream);
            var clientTransport = new LocalClient(clientStream, serverStream);
            var serverTask = Task.Factory.StartNew(() => RunLocalServer(HELLO_PROTOCOL, serverTransport, cancellationTokenSource.Token));
            var client = ConnectLocalClient(HELLO_PROTOCOL, clientTransport);
            var responseRecord = RunGenericClient(client, cancellationTokenSource.Token).Result;
            cancellationTokenSource.Cancel();
            Task.WaitAll(serverTask);
            Assert.AreEqual("World!", responseRecord.GetT1()[0]);
        }

        [TestCase]
        public void TestHelloWorldTcp()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            var serverTask = Task.Factory.StartNew(() => RunTcpServer(HELLO_PROTOCOL, cancellationTokenSource.Token));
            var client = ConnectTcpClient(HELLO_PROTOCOL).Result;
            var responseRecord = RunGenericClient(client, cancellationTokenSource.Token).Result;
            Task.WaitAll(serverTask);
            Assert.AreEqual("World!", responseRecord.GetT1()[0]);
        }

        //[TestCase]
        //public void TestHelloWorldHttp()
        //{
        //    using (var cancellationTokenSource = new CancellationTokenSource())
        //    {
        //        var serverTask = Task.Factory.StartNew(() => RunHttpServer(HELLO_PROTOCOL, cancellationTokenSource.Token));
        //        var client = ConnectHttpClient(HELLO_PROTOCOL, cancellationTokenSource.Token).Result;
        //        var responseRecord = RunGenericClient(client, cancellationTokenSource.Token).Result;
        //        Task.WaitAll(serverTask);
        //        serverTask.Result.Result.Close();
        //        Assert.AreEqual("World!", responseRecord?[0]);
        //    }
        //}

        private GenericClient ConnectLocalClient(AvroProtocol protocol, LocalClient tranceiver)
        {
            return new GenericClient(protocol, tranceiver);
        }

        private async Task<GenericClient> ConnectTcpClient(AvroProtocol protocol)
        {
            var tranceiver = await SocketClient.ConnectAsync("127.0.0.1", 3456);
            return new GenericClient(protocol, tranceiver);
        }

        private async Task<GenericClient> ConnectHttpClient(AvroProtocol protocol)
        {
            var tranceiver = new HttpClient(new Uri($"http://localhost:8080/{protocol.Name}"));
            return new GenericClient(protocol, tranceiver);
        }

        private async Task<GenericServer> RunLocalServer(AvroProtocol protocol, LocalServer tranceiver, CancellationToken token)
        {
            var server = new GenericServer(protocol, tranceiver);
            await RunGenericServer(server, token);
            return server;
        }

        private async Task<GenericServer> RunTcpServer(AvroProtocol protocol, CancellationToken token)
        {
            var listener = new SocketListener("127.0.0.1", 3456);
            listener.Start();
            var tranceiver = await listener.ListenAsync();
            var server = new GenericServer(protocol, tranceiver);
            await RunGenericServer(server, token);
            listener.Stop();
            return server;
        }

        private async Task<GenericServer> RunHttpServer(AvroProtocol protocol, CancellationToken token)
        {
            var urls = protocol.Messages.Select(r => $"http://localhost:8080/{protocol.Name}/{r.Name}/");
            var tranceiver = new HttpServer(urls.ToArray());
            var server = new GenericServer(protocol, tranceiver);
            await RunGenericServer(server, token);
            return server;
        }

        public async Task RunGenericServer(GenericServer server, CancellationToken token)
        {
            var rpcContext = await server.ReceiveAsync(token);
            var response = new GenericRecord((RecordSchema)server.Protocol.Types.First(r => r.Name == "Greeting"));
            response[0] = "World!";
            rpcContext.Response = response;
            await server.RespondAsync(rpcContext, token);
        }

        private async Task<AvroUnion<GenericRecord, GenericFixed, GenericEnum>> RunGenericClient(GenericClient client, CancellationToken token)
        {
            var parameterType = (RecordSchema)client.Protocol.Types.First(r => r.Name == "Greeting");
            var parameterRecordSchema = new RecordSchema(
                "hello",
                $"{client.Protocol.Namespace}.messages",
                new [] { new FieldSchema("greeting", parameterType) }
            );
            var parameter = new GenericRecord(parameterType);
            parameter[0] = "Hello!";
            var parameterRecord = new GenericRecord(parameterRecordSchema);
            parameterRecord[0] = parameter;
            var rpcContext = await client.RequestAsync("hello", parameterRecord, token);
            return rpcContext.Response;
        }
    }
}
