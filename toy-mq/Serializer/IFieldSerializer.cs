using System;
using System.Reflection;

namespace ToyMQ.Serializer {
    public interface IFieldSerializer {
        Type[] GetSupportedTypes();
        byte[] Serialize(object obj);
        object Deserialize(byte[] data, ref int offset, Type type);
    }
}
