using System;
using System.Reflection;
using ToyMQ.Proxy;

namespace ToyMQ.Test {
    public interface MyFace {
        int PrintFn(string s);
    };

    public class MyHandler : ProxyHandler {
        public object HandleCall(MethodInfo callee, object[] args) {
            return null;
        }
    };

    public class ProxyTest {
        public static void TestProxyfier() {
            var proxifier = new Proxifier(typeof(MyFace));
            var ret = (MyFace)proxifier.GetObject(new MyHandler());
            ret.PrintFn("2333");
        }
    }
}
