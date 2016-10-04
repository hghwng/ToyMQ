using System;
using System.IO;
using System.Text;

namespace ToyMQ {
    class MainClass {
        public static void Main(string[] args) {
            switch (args[0]) {
                case "mq": {
                    if (args[1] == "server") {
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

                case "proxy": {
                    ToyMQ.Test.ProxyTest.TestProxyfier();
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
