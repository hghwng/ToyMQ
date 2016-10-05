using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using ToyMQ.Serializer;

namespace ToyMQ.Serializer {
    public class ArrayFieldSerializer : IFieldSerializer {
        public Type[] GetSupportedTypes() {
            return new Type[] { typeof(object[]) };
        }

        public byte[] Serialize(object objects_) {
            var objects = (object[])objects_;
            var buffer = new List<byte>();
            buffer.AddRange(FieldSerializer.Serialize(objects.Length));
            foreach (var obj in objects) {
                var tmp = FieldSerializer.Serialize(obj);
                buffer.AddRange(FieldSerializer.Serialize(tmp.Length));
                buffer.AddRange(tmp);
            }
            return buffer.ToArray();
        }

        public object Deserialize(byte[] data, ref int offset, Type type) {
            var objectNum = (int)FieldSerializer.Deserialize(data, ref offset, typeof(int));
            var objects = new object[objectNum];
            for (int i = 0; i < objectNum; ++i) {
                var objectSize = (int)FieldSerializer.Deserialize(data, ref offset, typeof(int));
                var bytes = new byte[objectSize];
                Array.Copy(data, offset, bytes, 0, objectSize);
                objects[i] = bytes;
                offset += objectSize;
            }
            return objects;
        }
    }
}
