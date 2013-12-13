using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class EnumTypeConverter : ITypeConverter {
        private Type _type;

        private EnumTypeConverter(Type type) {
            _type = type;
        }

        public static EnumTypeConverter TryCreate(Type type) {
            if (type.IsEnum) {
                return new EnumTypeConverter(type);
            }

            return null;
        }

        public object Import(SerializedData data, ObjectGraphReader graph) {
            return Enum.Parse(_type, data.AsString);
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            return new SerializedData(instance.ToString());
        }
    }

}