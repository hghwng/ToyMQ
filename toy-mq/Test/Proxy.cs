using System;
using System.Reflection;
using ToyMQ.Proxy;

namespace ToyMQ.Test {
    public interface MyFace {
        int PrintFn(string s);
    };

    public class MyHandler : ProxyHandler {
        public object HandleCall(MethodBase callee, object[] args) {
            return 2333;
        }
    };

    public class ProxyTest {
        public static void TestProxyfier() {
            var ret = Proxifier.BuildObject<MyFace>(new MyHandler());
            ret.PrintFn("2333");
        }
    }
}
