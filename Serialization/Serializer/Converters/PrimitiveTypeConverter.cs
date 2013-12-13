using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class PrimitiveTypeConverter : ITypeConverter {
        private Func<SerializedData, object> _importer;
        private Func<object, SerializedData> _exporter;

        private PrimitiveTypeConverter(Func<SerializedData, object> importer,
            Func<object, SerializedData> exporter) {
            _importer = importer;
            _exporter = exporter;
        }

        private static PrimitiveTypeConverter CreateTypeConverter<T>(Func<SerializedData, T> importer,
            Func<T, SerializedData> exporter) {
            return new PrimitiveTypeConverter(
                data => importer(data),
                instance => exporter((T)instance));
        }

        public static PrimitiveTypeConverter TryCreate(Type type) {
            if (type == typeof(byte)) {
                return CreateTypeConverter(
                    data => (byte)data.AsReal.AsInt,
                    instance => new SerializedData(Real.CreateDecimal(instance))
                );
            }

            if (type == typeof(short)) {
                return CreateTypeConverter(
                    data => (short)data.AsReal.AsInt,
                    instance => new SerializedData(Real.CreateDecimal(instance))
                );
            }

            if (type == typeof(int)) {
                return CreateTypeConverter(
                    data => data.AsReal.AsInt,
                    instance => new SerializedData(Real.CreateDecimal(instance))
                );
            }

            if (type == typeof(Real)) {
                return CreateTypeConverter(
                    data => data.AsReal,
                    instance => new SerializedData(instance)
                );
            }

            if (type == typeof(bool)) {
                return CreateTypeConverter(
                    data => data.AsBool,
                    instance => new SerializedData(instance)
                );
            }

            if (type == typeof(string)) {
                return CreateTypeConverter(
                    data => data.AsString,
                    instance => new SerializedData(instance)
                );
            }

            if (type == typeof(SerializedData)) {
                return CreateTypeConverter(
                    data => data,
                    instance => instance
                );
            }

            return null;
        }

        public object Import(SerializedData data, ObjectGraphReader graph) {
            return _importer(data);
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            return _exporter(instance);
        }
    }
}