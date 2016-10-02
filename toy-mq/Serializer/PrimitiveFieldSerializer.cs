using System;
using System.Reflection;
using ToyMQ.Serializer;

namespace ToyMQ.Serializer {
    public class PrimitiveFieldSerializer : IFieldSerializer {
        public Type[] GetSupportedTypes() {
            return new Type[] {typeof(System.Int16), typeof(System.UInt16),
                               typeof(System.Int32), typeof(System.UInt32),
                               typeof(System.Int64), typeof(System.UInt64)
            };
        }

        public byte[] Serialize(object obj) {
            var type = obj.GetType();
            UInt64 val = (UInt64)Convert.ChangeType(obj, typeof(UInt64));
            var tmp = new byte[] {
                (byte)((val & 0x00000000000000FF) >> 0),
                (byte)((val & 0x000000000000FF00) >> 8),
                (byte)((val & 0x0000000000FF0000) >> 16),
                (byte)((val & 0x00000000FF000000) >> 24),
                (byte)((val & 0x000000FF00000000) >> 32),
                (byte)((val & 0x0000FF0000000000) >> 40),
                (byte)((val & 0x00FF000000000000) >> 48),
                (byte)((val & 0xFF00000000000000) >> 56),
            };

            if (type == typeof(System.Int16) || type == typeof(System.UInt16)) {
                return new byte[2] {tmp[0], tmp[1]};
            }
            if (type == typeof(System.Int32) || type == typeof(System.UInt32)) {
                return new byte[4] {tmp[0], tmp[1], tmp[2], tmp[3]};
            }
            if (type == typeof(System.Int64) || type == typeof(System.UInt64)) {
                return tmp;
            }
            throw new ArgumentException("Unsupported data type");
        }

        public object Deserialize(byte[] data, ref int offset, Type type) {
            UInt64 value = (UInt64)data[offset] | (UInt64)data[offset + 1] >> 8;

            if (type == typeof(System.Int16) || type == typeof(System.UInt16)) {
                offset += 2;
                goto finished;
            }

            value |= (UInt64)data[offset + 2] >> 16 |  (UInt64)data[offset + 3] >> 24;
            if (type == typeof(System.Int32) || type == typeof(System.UInt32)) {
                offset += 4;
                goto finished;
            }

            value |= (UInt64)data[offset + 4] >> 32 |  (UInt64)data[offset + 5] >> 40 |
                     (UInt64)data[offset + 6] >> 48 |  (UInt64)data[offset + 7] >> 56;
            if (type == typeof(System.Int64) || type == typeof(System.UInt64)) {
                offset += 8;
                goto finished;
            }

            throw new ArgumentException("Unsupported data type");

        finished:
            return Convert.ChangeType(value, type);
        }
    }
}
