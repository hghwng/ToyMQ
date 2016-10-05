using System;
using System.Text;
using System.Reflection;
using ToyMQ.Proxy;
using ToyMQ.Adapter;
using ToyMQ.MessageQueue;
using ToyMQ.Serializer;

namespace ToyMQ.Proxy {
    public class ProxyClientHandler : ProxyHandler {
        private IAdapter server_;
        private Type type_;
        private int id_;

        public static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public object HandleCall(MethodBase callee, object[] args) {
            var argsWithoutThis = new object[args.Length - 1];
            for (int i = 1; i < args.Length; ++i) argsWithoutThis[i - 1] = args[i];

            var request = new Message();
            request.Envelopes.Add(FieldSerializer.Serialize(id_));
            request.Envelopes.Add(FieldSerializer.Serialize(callee.Name));
            request.Envelopes.Add(FieldSerializer.Serialize(argsWithoutThis));
            request.SendTo(server_);

            var reply = Message.ReceiveFrom(server_);
            var replyType = type_.GetMethod(callee.Name).ReturnType;
            var offset = 0;

            return FieldSerializer.Deserialize(reply.Envelopes[0], ref offset, replyType);
        }

        public ProxyClientHandler(IAdapter server, int id, Type type) {
            server_ = server;
            id_ = id;
            type_ = type;
        }
    };

    public class ProxyClient<T> {
        private IAdapter server_;
        private Proxifier proxifier_;

        public ProxyClient(IAdapter server) {
            server_ = server;
            proxifier_ = new Proxifier(typeof(T));
        }

        public T CreateNewObject() {
            // Send create request
            var request = new Message();
            request.SendTo(server_);

            // Receive object id
            var reply = Message.ReceiveFrom(server_);
            var id = FieldSerializer.Deserialize<int>(reply.Envelopes[0]);

            var handler = new ProxyClientHandler(server_, id, typeof(T));
            return (T)proxifier_.ProxyTo(handler);
        }
    }
}
