using System;
using System.Reflection;

namespace ToyMQ.Serializer {
    public class PrimitiveSerializer {
        private static byte[] SerializeInteger(object obj) {
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

        private static object DeserializeInteger(byte[] data, ref int offset, Type type) {
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

        public static byte[] Serialize(Int16 value) { return SerializeInteger(value); }
        public static byte[] Serialize(UInt16 value) { return SerializeInteger(value); }
        public static byte[] Serialize(Int32 value) { return SerializeInteger(value); }
        public static byte[] Serialize(UInt32 value) { return SerializeInteger(value); }
        public static byte[] Serialize(Int64 value) { return SerializeInteger(value); }
        public static byte[] Serialize(UInt64 value) { return SerializeInteger(value); }

        public static void Deserialize(byte[] data, ref int offset, out Int16 value) {
            Int16 tmp = (Int16)DeserializeInteger(data, ref offset, typeof(Int16));
            value = tmp;
        }

        public static void Deserialize(byte[] data, ref int offset, out UInt16 value) {
            UInt16 tmp = (UInt16)DeserializeInteger(data, ref offset, typeof(UInt16));
            value = tmp;
        }

        public static void Deserialize(byte[] data, ref int offset, out Int32 value) {
            Int32 tmp = (Int32)DeserializeInteger(data, ref offset, typeof(Int32));
            value = tmp;
        }

        public static void Deserialize(byte[] data, ref int offset, out UInt32 value) {
            UInt32 tmp = (UInt32)DeserializeInteger(data, ref offset, typeof(UInt32));
            value = tmp;
        }

        public static void Deserialize(byte[] data, ref int offset, out Int64 value) {
            Int64 tmp = (Int64)DeserializeInteger(data, ref offset, typeof(Int64));
            value = tmp;
        }

        public static void Deserialize(byte[] data, ref int offset, out UInt64 value) {
            UInt64 tmp = (UInt64)DeserializeInteger(data, ref offset, typeof(UInt64));
            value = tmp;
        }
    }
}
