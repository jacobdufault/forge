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
    internal class ReflectedTypeConverter : BaseTypeConverter {
        private TypeModel _model;

        private ReflectedTypeConverter(TypeModel model) {
            _model = model;
        }

        public static ReflectedTypeConverter TryCreate(Type type) {
            return new ReflectedTypeConverter(TypeCache.GetTypeModel(type));
        }

        protected override object DoImport(SerializedData data, ObjectGraphReader graph, object instance) {
            if (instance == null) {
                instance = graph.GetObjectInstance(_model, data);
            }

            Dictionary<string, SerializedData> serializedDataDict = data.AsDictionary;
            for (int i = 0; i < _model.Properties.Count; ++i) {
                PropertyModel property = _model.Properties[i];

                // deserialize the property
                string name = property.Name;
                Type storageType = property.StorageType;

                // throw if the dictionary is missing a required property
                if (serializedDataDict.ContainsKey(name) == false) {
                    throw new Exception("There is no key " + name + " in serialized data "
                        + serializedDataDict + " when trying to deserialize an instance of " +
                        _model);
                }

                BaseTypeConverter converter = TypeConverterResolver.GetTypeConverter(storageType);
                object deserialized = converter.Import(serializedDataDict[name], graph, null);

                // write it into the instance
                property.Write(instance, deserialized);
            }

            return instance;
        }

        protected override SerializedData DoExport(object instance, ObjectGraphWriter graph) {
            var dict = new Dictionary<string, SerializedData>();

            for (int i = 0; i < _model.Properties.Count; ++i) {
                PropertyModel property = _model.Properties[i];

                // make sure we export under the storage type of the property; if we don't, and say,
                // the property is an interface, and we export under the instance type, then
                // deserialization will not work as expected (because we'll be trying to deserialize
                // an interface).
                BaseTypeConverter converter = TypeConverterResolver.GetTypeConverter(property.StorageType);
                dict[property.Name] = converter.Export(property.Read(instance), graph);
            }

            return new SerializedData(dict);
        }
    }

}