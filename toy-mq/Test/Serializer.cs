using System;
using System.Linq;
using ToyMQ.Serializer;

namespace ToyMQ.Test {
    [FieldSet]
    public class Inner {
        [Field(0)] public int t = 30;
    }

    [FieldSet]
    public class SerializerTest {
        [Field(0)] public int x = 20;
        [Field(1)] public long y = 10;
        [Field(2)] public Inner i = new Inner();

        public static void Test() {
            var obj = new SerializerTest();
            var bytes = FieldSerializer.Serialize(obj);
            var obj1 = FieldSerializer.Deserialize<SerializerTest>(bytes);
            var bytes1 = FieldSerializer.Serialize(obj1);
            Console.WriteLine("obj equal? " + (obj.x == obj1.x && obj.y == obj1.y
                                               && obj.i.t == obj1.i.t));
            bool bytesEqual = true;
            if (bytes.Length == bytes1.Length) {
                for (int i = 0; i < bytes.Length; ++i) {
                    if (bytes[i] != bytes1[i])
                        bytesEqual = false;
                }
            } else {
                bytesEqual = false;
            }

            Console.WriteLine("bytes equal? " + bytesEqual);
        }
    }
}
