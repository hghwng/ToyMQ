using System;
using System.Reflection;

namespace ToyMQ.Proxy {
    public interface ProxyHandler {
        object HandleCall(MethodInfo callee, object[] args);
    }
}
