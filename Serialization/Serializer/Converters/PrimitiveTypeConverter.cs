// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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

        public object Import(SerializedData data, ObjectGraphReader graph, object instance) {
            if (instance != null) {
                throw new InvalidOperationException("Cannot handle preallocated objects");
            }

            return _importer(data);
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            return _exporter(instance);
        }
    }
}