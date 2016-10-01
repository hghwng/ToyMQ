using System;
using System.Collections.Generic;
using ToyMQ.MessageQueue.Adapter;

namespace ToyMQ.MessageQueue {
    public class AdapterFactory {
        private static List<IAdapterFactory> factories_;

        static AdapterFactory() {
            factories_ = new List<IAdapterFactory>();
            factories_.Add(new TCPAdapterFactory());
        }

        private static IAdapterFactory FindFactory(string url) {
            foreach (var factory in factories_) {
                if (factory.IsProtocolSupported(url)) {
                    return factory;
                }
            }
            throw new ArgumentException("Unsupported protocol");
        }

        public static IAdapterServer CreateServer(string url) {
            var factory = FindFactory(url);
            return factory.CreateServer(url);
        }

        public static IAdapter CreateClient(string url) {
            var factory = FindFactory(url);
            return factory.CreateClient(url);
        }
    }
}
