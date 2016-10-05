using System;
using System.Collections.Generic;
using ToyMQ.Adapter;
using ToyMQ.Serializer;
using ToyMQ.MessageQueue;

namespace ToyMQ.Proxy {
    public class ProxyServer<T> where T : new() {
        private IAdapter client_;
        private List<T> objects_;

        private Message HandleNew() {
            // New object request
            int id = objects_.Count;
            objects_.Add(new T());

            var reply = new Message();
            reply.Envelopes.Add(FieldSerializer.Serialize(id));
            return reply;
        }

        private Message HandleCall(Message request) {
            var id = FieldSerializer.Deserialize<int>(request.Envelopes[0]);
            var name = FieldSerializer.Deserialize<string>(request.Envelopes[1]);
            var argsInByte = (object[])FieldSerializer.Deserialize<object[]>(request.Envelopes[2]);

            var obj = objects_[id];
            var fn = typeof(T).GetMethod(name);

            var parameters = fn.GetParameters();
            var args = new object[argsInByte.Length];
            for (int i = 0; i < parameters.Length; ++i) {
                int offset = 0;
                args[i] = FieldSerializer.Deserialize((byte[])(argsInByte[i]), ref offset, parameters[i].ParameterType);
            }

            var result = fn.Invoke(obj, args);
            var reply = new Message();
            reply.Envelopes.Add(FieldSerializer.Serialize(result));
            return reply;
        }

        public ProxyServer(IAdapter client) {
            objects_ = new List<T>();
            client_ = client;
        }

        public void Run() {
            while (true) {
                try {
                    var request = Message.ReceiveFrom(client_);
                    if (request.Envelopes.Count == 0) {
                        HandleNew().SendTo(client_);
                    } else {
                        HandleCall(request).SendTo(client_);
                    }
                } catch (ConnectionClosedException) {
                    return;
                }
            }
        }
    }
}
