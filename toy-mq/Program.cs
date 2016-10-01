using System;
using System.Text;
using ToyMQ.MessageQueue;

namespace ToyMQ {
    class MainClass {
        public static void Main (string[] args) {
            var server = AdapterFactory.CreateServer("tcp://localhost:23333");
            var client = server.WaitForNewClient();
            var str = "Hello world";
            client.Send(Encoding.ASCII.GetBytes(str));

            byte[] recv = new byte[6];
            client.Receive(recv);
            Console.WriteLine(Encoding.ASCII.GetString(recv));
        }
    }
}
