using System;
using System.Net;
using System.Net.Sockets;
using ToyMQ.MessageQueue.Adapter;


namespace ToyMQ.MessageQueue.Adapter {
    public class TCPAdapterFactory : IAdapterFactory {
        private bool VerifyUri(Uri uri) {
            return uri.Scheme.ToLower() == "tcp";
        }

        private EndPoint GetEndPointFromUri(Uri uri) {
            if (!VerifyUri(uri)) throw new ArgumentException("Unsupported protocol");
            var address = Dns.GetHostEntry(uri.Host).AddressList[0];
            return new IPEndPoint(address, uri.Port);
        }

        public bool IsProtocolSupported(string url) {
            return VerifyUri(new Uri(url));
        }

        public IAdapterServer CreateServer(string url) {
            return new TCPAdapterServer(GetEndPointFromUri(new Uri(url)));
        }

        public IAdapter CreateClient(string url) {
            var endpoint = GetEndPointFromUri(new Uri(url));
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            return new TCPAdapter(socket);
        }
    }

    public class TCPAdapterServer : IAdapterServer {
        private Socket socket_;

        public IAdapter WaitForNewClient() {
            return new TCPAdapter(socket_.Accept());
        }

        public TCPAdapterServer(EndPoint endpoint) {
            socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket_.Bind(endpoint);

            const int kMaxPendingConnection = 20;
            socket_.Listen(kMaxPendingConnection);
        }
    }

    public class TCPAdapter : IAdapter {
        private Socket socket_;

        public int Send(byte[] data) {
            return socket_.Send(data);
        }

        public int Receive(byte[] data) {
            return socket_.Receive(data);
        }

        public TCPAdapter(Socket socket) {
            socket_ = socket;
        }
    }
}
