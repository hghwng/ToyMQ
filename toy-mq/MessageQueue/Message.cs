using System;
using System.Collections.Generic;
using ToyMQ.MessageQueue.Adapter;
using ToyMQ.Serializer;

namespace ToyMQ.MessageQueue {
    public class Message {
        public List<byte[]> Envelopes;

        private int GetTotalSize() {
            int size = sizeof(int) * (Envelopes.Count + 1);
            foreach (var envelope in Envelopes) size += envelope.Length;
            return size;
        }

        public byte[] ToBytes() {
            var result = new byte[GetTotalSize()];
            var offset = 0;
            byte[] buffer;

            buffer = FieldSerializer.Serialize(Envelopes.Count);
            buffer.CopyTo(result, offset);
            offset += buffer.Length;

            foreach (var envelope in Envelopes) {
                buffer = FieldSerializer.Serialize(envelope.Length);
                buffer.CopyTo(result, offset);
                offset += buffer.Length;
            }

            foreach (var envelope in Envelopes) {
                envelope.CopyTo(result, offset);
                offset += envelope.Length;
            }

            return result;
        }

        public void SendTo(IAdapter adapter) {
            adapter.Send(ToBytes());
        }

        public Message() {
            Envelopes = new List<byte[]>();
        }

        public Message(byte[] data) {
            Envelopes = new List<byte[]>();
            var offset = 0;

            var envelopeCount = FieldSerializer.Deserialize<UInt32>(data, ref offset);
            for (int i = 0; i < envelopeCount; ++i) {
                var envelopeLength = FieldSerializer.Deserialize<int>(data, ref offset);
                Envelopes.Add(new byte[envelopeLength]);
            }

            for (int i = 0; i < envelopeCount; ++i) {
                Array.Copy(data, offset, Envelopes[i], 0, Envelopes[i].Length);
                offset += Envelopes[i].Length;
            }
        }

        public static Message ReceiveFrom(IAdapter adapter) {
            var msg = new Message();
            var buffer = new byte[4];

            adapter.Receive(buffer);
            var envelopeCount = FieldSerializer.Deserialize<UInt32>(buffer);
            for (int i = 0; i < envelopeCount; ++i) {
                adapter.Receive(buffer);
                var envelopeLength = FieldSerializer.Deserialize<UInt32>(buffer);
                msg.Envelopes.Add(new byte[envelopeLength]);
            }

            for (int i = 0; i < envelopeCount; ++i) {
                adapter.Receive(msg.Envelopes[i]);
            }

            return msg;
        }
    }
}
