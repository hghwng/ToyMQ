using System;
using System.Text;
using System.Collections.Generic;
using ToyMQ.Adapter;
using ToyMQ.MessageQueue;

namespace ToyMQ.Test {
    public class MessageQueue {
        private static void PrintMessage(Message msg) {
            foreach (var envelope in msg.Envelopes) {
                Console.WriteLine("    " + Encoding.ASCII.GetString(envelope));
            }
        }

        public static void RunClient() {
            Console.WriteLine("Client is running");
            var server = AdapterFactory.CreateClient("tcp://localhost:23333");
            PrintMessage(Message.ReceiveFrom(server));
            Console.WriteLine("Hello from server");

            var msg = new Message();
            msg.Envelopes.Add(Encoding.ASCII.GetBytes("TestClient"));
            Console.WriteLine("Sending reply");
            msg.SendTo(server);

            Console.WriteLine("Receiving bye");
            PrintMessage(Message.ReceiveFrom(server));
        }

        public static void RunServer() {
            Console.WriteLine("Server is running");
            var server = AdapterFactory.CreateServer("tcp://localhost:23333");

            var msgHello = new Message();
            msgHello.Envelopes.Add(Encoding.ASCII.GetBytes("From server:"));
            msgHello.Envelopes.Add(Encoding.ASCII.GetBytes("What's your name?"));

            var msgBye = new Message();
            msgBye.Envelopes.Add(Encoding.ASCII.GetBytes("Goodbye:"));
            msgBye.Envelopes.Add(null);

            while (true) {
                var client = server.WaitForNewClient();
                Console.WriteLine("New client");
                msgHello.SendTo(client);

                var reply = Message.ReceiveFrom(client);
                msgBye.Envelopes[1] = reply.Envelopes[0];
                msgBye.SendTo(client);
                Console.WriteLine("End of reply");
            }
        }
    }
}
