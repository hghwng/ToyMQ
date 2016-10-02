using System;
using System.Reflection;
using System.Collections.Generic;
using ToyMQ.Serializer;

namespace ToyMQ.Serializer {
    public class FieldSerializer {
        private static Dictionary<Type, IFieldSerializer> fieldSerializers_;

        static FieldSerializer() {
            fieldSerializers_ = new Dictionary<Type, IFieldSerializer>();
            Register(new PrimitiveFieldSerializer());
        }

        public static void Register(IFieldSerializer serializer) {
            foreach (var type in serializer.GetSupportedTypes()) {
                fieldSerializers_.Add(type, serializer);
            }
        }

        private static bool IsFieldSetType(Type type) {
            MemberInfo info = type;
            return info.GetCustomAttribute<FieldSet>() != null;
        }

        private static List<FieldInfo> GetFieldList(Type type) {
            var fields = new List<Tuple<int, FieldInfo>>();
            foreach (var field in type.GetFields()) {
                // Only process fields that annotated with "[Field(order)]"
                var attribute = field.GetCustomAttribute<Field>(true);
                if (attribute == null) continue;

                // Ensure the field is:
                //   - one of the builtin types (have serializer already registered)
                //   - or the FieldSet type
                var fieldType = field.FieldType;
                if (!fieldSerializers_.ContainsKey(fieldType) && !IsFieldSetType(fieldType)) {
                    throw new NotSupportedException("Unserializable field " + field.Name);
                }

                fields.Add(new Tuple<int, FieldInfo>(attribute.order, field));
            }

            // Ensure the order is continuous
            fields.Sort((x, y) => { return x.Item1.CompareTo(y.Item1); });
            var result = new List<FieldInfo>();
            for (int i = 0; i < fields.Count; ++i) {
                if (i != fields[i].Item1) {
                    Console.WriteLine("" + i + "_" + fields[i].Item1);
                    throw new ArgumentOutOfRangeException("Field " + fields[i].Item2.Name + " out of order");
                }
                result.Add(fields[i].Item2);
            }
            return result;
        }

        public static byte[] Serialize(object obj) {
            Type objectType = obj.GetType();

            // Try builtin types first
            if (fieldSerializers_.ContainsKey(objectType)) {
                return fieldSerializers_[objectType].Serialize(obj);
            }

            // FieldSet type
            var result = new List<byte>();
            if (!IsFieldSetType(objectType)) throw new NotSupportedException("Unserializable data type");
            foreach (var field in GetFieldList(objectType)) {
                  result.AddRange(Serialize(field.GetValue(obj)));
            }

            return result.ToArray();
        }

        public static object Deserialize(byte[] data, ref int offset, Type type) {
            object obj = Activator.CreateInstance(type);
            if (fieldSerializers_.ContainsKey(type)) {
                return fieldSerializers_[type].Deserialize(data, ref offset, type);
            }

            foreach (var field in GetFieldList(type)) {
                field.SetValue(obj, Deserialize(data, ref offset, field.FieldType));
            }

            return obj;
        }

        public static T Deserialize<T>(byte[] data) {
            var offset = 0;
            return (T)Deserialize(data, ref offset, typeof(T));
        }

        public static T Deserialize<T>(byte[] data, ref int offset) {
            return (T)Deserialize(data, ref offset, typeof(T));
        }
    }
}
