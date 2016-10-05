using System;
using System.Net;
using System.Net.Sockets;


namespace ToyMQ.Adapter {
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
            return url.StartsWith("tcp://");
        }

        public IAdapterServer CreateServer(string url) {
            return new TCPAdapterServer(GetEndPointFromUri(new Uri(url)));
        }

        public IAdapter CreateClient(string url) {
            var endpoint = GetEndPointFromUri(new Uri(url));
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
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
            socket_ = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine(endpoint);
            socket_.Bind(endpoint);

            const int kMaxPendingConnection = 20;
            socket_.Listen(kMaxPendingConnection);
        }
    }

    public class ConnectionClosedException : Exception {
    }

    public class TCPAdapter : IAdapter {
        private Socket socket_;

        public int Send(byte[] data) {
            try {
                return socket_.Send(data);
            } catch (SocketException) {
                throw new ConnectionClosedException();
            }
        }

        public int Receive(byte[] data) {
            try {
              return socket_.Receive(data);
            } catch (SocketException) {
                throw new ConnectionClosedException();
            }
        }

        public TCPAdapter(Socket socket) {
            socket_ = socket;
        }
    }
}
