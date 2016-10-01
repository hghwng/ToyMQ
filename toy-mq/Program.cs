using System;
using System.IO;
using System.Text;
using ToyMQ.MessageQueue;

namespace ToyMQ {
    class MainClass {
        public static void Main(string[] args) {
            Console.WriteLine("Creating server");
            var server = AdapterFactory.CreateServer("pipe://s_server:s_client");
            Console.WriteLine("Waiting for client");
            var client = server.WaitForNewClient();
            var str = "Hello world";
            Console.WriteLine("Sending");
            client.Send(Encoding.ASCII.GetBytes(str));

            byte[] recv = new byte[6];
            Console.WriteLine("Receiving");
            client.Receive(recv);
            Console.WriteLine(Encoding.ASCII.GetString(recv));
        }
    }
}
