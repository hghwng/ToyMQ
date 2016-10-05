using System;
using System.Reflection;
using ToyMQ.Proxy;
using ToyMQ.Adapter;

namespace ToyMQ.Test {
    public interface DemoInterface {
        int strlen(string s);
        string strcat(string s0, string s1);
        int counter();
    };

    public class DemoImpl : DemoInterface {
        private int i = 0;

        public int strlen(string s) {
            Console.WriteLine(String.Format("strlen({0}) = {1}", s, s.Length));
            return s.Length;
        }

        public string strcat(string s0, string s1) {
            Console.WriteLine(String.Format("strcat({0}, {1}) = {2}", s0, s1, s0 + s1));
            return s0 + s1;
        }

        public int counter() {
            return ++i;
        }

        public DemoImpl() {
            Console.WriteLine("New client");
        }
    };

    public class ProxyTest {
        public static void RunClient() {
            var client = AdapterFactory.CreateClient("tcp://localhost:6666");
            var proxy = new ProxyClient<DemoInterface>(client);

            var obj = proxy.CreateNewObject();
            Console.WriteLine("strlen() = " + obj.strlen("23333"));
            Console.WriteLine("strcat() = " + obj.strcat("123", "456"));
            Console.WriteLine("obj.counter() = " + obj.counter());

            var newobj = proxy.CreateNewObject();
            Console.WriteLine("newobj.counter() = " + newobj.counter());
            Console.WriteLine("obj.counter() = " + obj.counter());
            Console.WriteLine("obj.counter() = " + obj.counter());
            Console.WriteLine("newobj.counter() = " + newobj.counter());
        }

        public static void RunServer() {
            var server = AdapterFactory.CreateServer("tcp://localhost:6666");
            var client = server.WaitForNewClient();

            var proxy = new ProxyServer<DemoImpl>(client);
            proxy.Run();
        }
    }
}
