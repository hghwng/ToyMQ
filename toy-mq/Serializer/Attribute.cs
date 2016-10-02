using System;
using System.Reflection;

namespace ToyMQ.Serializer {

    [AttributeUsage(AttributeTargets.All)]
    public class FieldSet : System.Attribute {
        public FieldSet() {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class Field : System.Attribute {
        public int order { get; }

        public Field(int n) { order = n; }
    }
}
