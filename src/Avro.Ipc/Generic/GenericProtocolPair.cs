﻿using Avro.Generic;
using Avro.IO;
using Avro.Ipc.Utils;
using Avro.Schemas;
using org.apache.avro.ipc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avro.Ipc.Generic
{
    public sealed class GenericProtocolPair
    {
        private static readonly object GUARD = new object();
        private static readonly HashCompare COMPARE = new HashCompare();
        private static readonly IDictionary<MD5, IDictionary<MD5, GenericProtocolPair>> PROTOCOL_PARIS = new Dictionary<MD5, IDictionary<MD5, GenericProtocolPair>>(COMPARE);

        private readonly IDictionary<string, IDatumReader<GenericRecord>> _requestReaders = new Dictionary<string, IDatumReader<GenericRecord>>();
        private readonly IDictionary<string, IDatumWriter<GenericRecord>> _requestWriters = new Dictionary<string, IDatumWriter<GenericRecord>>();
        private readonly IDictionary<string, IDatumReader<object>> _responseReaders = new Dictionary<string, IDatumReader<object>>();
        private readonly IDictionary<string, IDatumWriter<object>> _responseWriters = new Dictionary<string, IDatumWriter<object>>();
        private readonly IDictionary<string, IDatumReader<object>> _errorReaders = new Dictionary<string, IDatumReader<object>>();
        private readonly IDictionary<string, IDatumWriter<object>> _errorWrtiers = new Dictionary<string, IDatumWriter<object>>();

        public static GenericProtocolPair Get(Protocol protocol, Protocol remoteProtocol)
        {
            lock (GUARD)
            {
                if (!PROTOCOL_PARIS.TryGetValue(protocol.MD5, out var genericProtocolPairs))
                {
                    genericProtocolPairs = new Dictionary<MD5, GenericProtocolPair>(COMPARE);
                    PROTOCOL_PARIS.Add(protocol.MD5, genericProtocolPairs);
                }

                if (!genericProtocolPairs.TryGetValue(remoteProtocol.MD5, out var genericProtocolPair))
                {
                    genericProtocolPair = new GenericProtocolPair(protocol, remoteProtocol);
                    genericProtocolPairs.Add(remoteProtocol.MD5, genericProtocolPair);
                }
                return genericProtocolPair;
            }
        }

        public static bool AreSame(MD5 hash, MD5 remoteHash)
        {
            return COMPARE.Equals(hash, remoteHash);
        }

        public static bool Exists(MD5 hash, MD5 remoteHash)
        {
            if (!PROTOCOL_PARIS.TryGetValue(hash, out var genericProtocolPairs))
                return false;
            return genericProtocolPairs.ContainsKey(remoteHash);
        }

        private GenericProtocolPair(Protocol protocol, Protocol remoteProtocol)
        {
            var messagePairs =
                from lm in protocol.Messages
                join rm in remoteProtocol.Messages on lm.Name equals rm.Name
                select new
                {
                    MessageName = lm.Name,
                    LocalMessage = lm,
                    RemoteMessage = rm
                };

            foreach(var messagePair in messagePairs)
            {
                var localRequestParameters =
                    from p in messagePair.LocalMessage.RequestParameters
                    join t in protocol.Types on p.Type equals t.FullName
                    select new RecordSchema.Field(p.Name, t)
                ;

                var remoteRequestParameters =
                    from p in messagePair.RemoteMessage.RequestParameters
                    join t in remoteProtocol.Types on p.Type equals t.FullName
                    select new RecordSchema.Field(p.Name, t)
                ;

                var localRequest = new RecordSchema($"{protocol.FullName}.messages.{messagePair.MessageName}", localRequestParameters);
                var remoteRequest = new RecordSchema($"{remoteProtocol.FullName}.messages.{messagePair.MessageName}", remoteRequestParameters);

                var requestReader = new GenericReader<GenericRecord>(localRequest, remoteRequest);
                var requestWriter = new GenericWriter<GenericRecord>(localRequest);

                _requestReaders.Add(messagePair.MessageName, requestReader);
                _requestWriters.Add(messagePair.MessageName, requestWriter);

                var responseReader = new GenericReader<object>(messagePair.LocalMessage.Response, messagePair.RemoteMessage.Response);
                var responseWriter = new GenericWriter<object>(messagePair.LocalMessage.Response);

                _responseReaders.Add(messagePair.MessageName, responseReader);
                _responseWriters.Add(messagePair.MessageName, responseWriter);

                var errorReader = new GenericReader<object>(messagePair.LocalMessage.Error, messagePair.RemoteMessage.Error);
                var errorWriter = new GenericWriter<object>(messagePair.LocalMessage.Error);

                _errorReaders.Add(messagePair.MessageName, errorReader);
                _errorWrtiers.Add(messagePair.MessageName, errorWriter);
            }
        }

        internal GenericRecord ReadRequest(BinaryDecoder decoder, string message)
        {
            return _requestReaders[message].Read(decoder);
        }

        internal void WriteRequest(BinaryEncoder encoder, string message, GenericRecord record)
        {
            _requestWriters[message].Write(encoder, record);
        }

        internal object ReadResponse(BinaryDecoder decoder, string message)
        {
            return _responseReaders[message].Read(decoder);
        }

        internal void WriteReponse(BinaryEncoder encoder, string message, object response)
        {
            _responseWriters[message].Write(encoder, response);
        }

        internal object ReadError(BinaryDecoder decoder, string message)
        {
            return _errorReaders[message].Read(decoder);
        }

        internal void WriteError(BinaryEncoder encoder, string message, object error)
        {
            _errorWrtiers[message].Write(encoder, error);
        }
    }
}
