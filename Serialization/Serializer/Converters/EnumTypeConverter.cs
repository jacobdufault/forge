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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class EnumTypeConverter : BaseTypeConverter {
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

        protected override object DoImport(SerializedData data, ObjectGraphReader graph, object instance) {
            if (instance != null) {
                throw new InvalidOperationException("Cannot handle preallocated objects");
            }

            return Enum.Parse(_type, data.AsString);
        }

        protected override SerializedData DoExport(object instance, ObjectGraphWriter graph) {
            return new SerializedData(instance.ToString());
        }
    }

}