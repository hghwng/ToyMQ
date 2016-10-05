using System;
using System.Text;
using System.Reflection;
using ToyMQ.Serializer;

namespace ToyMQ.Serializer {
    public class StringFieldSerializer : IFieldSerializer {
        public Type[] GetSupportedTypes() {
            return new Type[] { typeof(string) };
        }

        public byte[] Serialize(object obj) {
            var bytes = Encoding.UTF32.GetBytes((string)obj);
            var length = FieldSerializer.Serialize(bytes.Length);
            var result = new byte[bytes.Length + length.Length];
            Array.Copy(length, result, length.Length);
            Array.Copy(bytes, 0, result, length.Length, bytes.Length);
            return result;
        }

        public object Deserialize(byte[] data, ref int offset, Type type) {
            int length = (int)FieldSerializer.Deserialize(data, ref offset, typeof(int));
            var result = Encoding.UTF32.GetString(data, offset, length);
            offset += length;
            return result;
        }
    }
}
