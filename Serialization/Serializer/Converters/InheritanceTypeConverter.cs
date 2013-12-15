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
    internal class InheritanceTypeConverter : BaseTypeConverter {
        private InheritanceTypeConverter()
            : base(false) {
        }

        public static InheritanceTypeConverter TryCreate(Type type) {
            TypeModel model = TypeCache.GetTypeModel(type);

            if (model.SupportsInheritance) {
                return new InheritanceTypeConverter();
            }

            return null;
        }

        protected override object DoImport(SerializedData data, ObjectGraphReader graph, object instance) {
            Type instanceType = TypeCache.FindType(data.AsDictionary["InstanceType"].AsString);
            BaseTypeConverter converter = TypeConverterResolver.GetNextConverter<InheritanceTypeConverter>(instanceType);
            return converter.Import(data.AsDictionary["Data"], graph, instance);
        }

        protected override SerializedData DoExport(object instance, ObjectGraphWriter graph) {
            Type instanceType = instance.GetType();
            Dictionary<string, SerializedData> data = new Dictionary<string, SerializedData>();

            // Export the type so we know what type to import as.
            data["InstanceType"] = new SerializedData(instanceType.FullName);

            // we want to make sure we export under the direct instance type, otherwise we'll go
            // into an infinite loop of reexporting the interface metadata.
            BaseTypeConverter converter = TypeConverterResolver.GetNextConverter<InheritanceTypeConverter>(instanceType);
            data["Data"] = converter.Export(instance, graph);

            return new SerializedData(data);
        }
    }
}