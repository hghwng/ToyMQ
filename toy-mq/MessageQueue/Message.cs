using System.Collections.Generic;
using ToyMQ.MessageQueue.Adapter;

namespace ToyMQ.MessageQueue {
    public class Message {
        private List<int> envelopeLengths_;
        private List<byte> data_;

        public Message() {
            envelopeLengths_ = new List<int>();
        }

        public void AddEnvelope(byte[] data) {
            envelopeLengths_.Add(data.Length);
            data_.AddRange(data);
        }

        public byte[] ToBytes() {
            // TODO
        }

        public void SendTo(IAdapter adapter) {
            // TODO
        }

        public Message(byte[] data) {
            // TODO
        }

        public static Message ReceiveFrom(IAdapter adapter) {
            // TODO
        }
    }
}
