using System;
using System.IO;
using ToyMQ.MessageQueue.Adapter;
using System.Runtime.InteropServices;

namespace ToyMQ.MessageQueue.Adapter {
    public class PipeAdapterFactory : IAdapterFactory {
        private string SubString(string s, int start, int end) {
            return s.Substring(start, end - start);
        }

        private void GetPipePathFromUrl(string url, out string serverPipe, out string clientPipe) {
            if (!IsProtocolSupported(url)) throw new ArgumentException("Unsupported protocol");
            int spliter = url.LastIndexOf(':');
            if (spliter == -1) throw new ArgumentException("Malformed path");
            serverPipe = SubString(url, "pipe://".Length, spliter);
            clientPipe = SubString(url, spliter + 1, url.Length);
        }

        public bool IsProtocolSupported(string url) {
            return url.StartsWith("pipe://");
        }

        public IAdapterServer CreateServer(string url) {
            string serverPipe, clientPipe;
            GetPipePathFromUrl(url, out serverPipe, out clientPipe);
            return new PipeAdapterServer(serverPipe, clientPipe);
        }

        public IAdapter CreateClient(string url) {
            string serverPipe, clientPipe;
            GetPipePathFromUrl(url, out serverPipe, out clientPipe);
            FileStream reader = File.Open(serverPipe, FileMode.Open);
            FileStream writer = File.Open(clientPipe, FileMode.Open);
            writer.WriteByte(0);
            return new PipeAdapter(writer, reader);
        }
    }

    public class PipeAdapterServer : IAdapterServer {
        private string serverPipe_, clientPipe_;
        private const int kFifoPermission = 0x180; // 0600

        [DllImport("libc.so.6")]
        private static extern int mkfifo([MarshalAs(UnmanagedType.LPTStr)] string path,
                                         [MarshalAs(UnmanagedType.I4)] int mode);

        public PipeAdapterServer(string serverPipe, string clientPipe) {
            serverPipe_ = serverPipe;
            clientPipe_ = clientPipe;
            if (mkfifo(serverPipe, kFifoPermission) != 0 ||
                mkfifo(clientPipe, kFifoPermission) != 0) {
                throw new SystemException("Cannot create FIFO.");
            }
        }

        public IAdapter WaitForNewClient() {
            var writer = File.Open(serverPipe_, FileMode.Open);
            var reader = File.Open(clientPipe_, FileMode.Open);
            reader.ReadByte();
            return new PipeAdapter(writer, reader);
        }

        ~PipeAdapterServer() {
            File.Delete(serverPipe_);
            File.Delete(clientPipe_);
        }
    }

    public class PipeAdapter : IAdapter {
        FileStream writer_;
        FileStream reader_;

        public int Send(byte[] data) {
            writer_.Write(data, 0, data.Length);
            writer_.Flush();
            return data.Length;
        }

        public int Receive(byte[] data) {
            return reader_.Read(data, 0, data.Length);
        }

        public PipeAdapter(FileStream writer, FileStream reader) {
            writer_ = writer;
            reader_ = reader;
        }
    }
}
