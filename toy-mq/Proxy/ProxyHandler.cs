using System;
using System.Reflection;

namespace ToyMQ.Proxy {
    public interface ProxyHandler {
        object HandleCall(MethodBase callee, object[] args);
    }
}
