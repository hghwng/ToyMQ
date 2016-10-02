using System;
using System.IO;
using System.Text;
using ToyMQ.MessageQueue;
using ToyMQ.Serializer;

namespace ToyMQ {
    class MainClass {
        public static void Main(string[] args) {
            switch (args[0]) {
                case "mq": {
                    if (args[0] == "server") {
                        ToyMQ.Test.MessageQueue.RunServer();
                    } else {
                        ToyMQ.Test.MessageQueue.RunClient();
                    }
                    break;
                }

                case "serial": {
                    ToyMQ.Test.SerializerTest.Test();
                    break;
                }

                default: {
                    Console.WriteLine("Invalid test");
                    break;
                }
            }
        }
    }
}
