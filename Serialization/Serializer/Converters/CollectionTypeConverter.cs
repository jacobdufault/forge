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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class CollectionTypeConverter : ITypeConverter {
        private TypeModel _model;
        private ITypeConverter _elementConverter;

        public CollectionTypeConverter(TypeModel model) {
            _model = model;
            _elementConverter = TypeConverterResolver.GetTypeConverter(model.ElementType);
        }

        public static CollectionTypeConverter TryCreate(Type type) {
            TypeModel model = TypeCache.GetTypeModel(type);

            if (model.IsCollection) {
                return new CollectionTypeConverter(model);
            }

            return null;
        }

        public object Import(SerializedData data, ObjectGraphReader graph, object instance) {
            if (instance == null) {
                instance = graph.GetObjectInstance(_model, data);
            }

            IList<SerializedData> items = data.AsList;

            _model.CollectionSizeHint(ref instance, items.Count);
            for (int i = 0; i < items.Count; ++i) {
                object indexedObject = _elementConverter.Import(items[i], graph, null);
                _model.AppendValue(ref instance, indexedObject, i);
            }

            return instance;
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            // If it's an array or a collection, we have special logic for processing
            List<SerializedData> output = new List<SerializedData>();

            // luckily both arrays and collections are enumerable, so we'll use that interface when
            // outputting the object
            IEnumerable enumerator = (IEnumerable)instance;
            foreach (object item in enumerator) {
                // make sure we export under the element type of the list; if we don't, and say, the
                // element type is an interface, and we export under the instance type, then
                // deserialization will not work as expected (because we'll be trying to deserialize
                // an interface).
                output.Add(_elementConverter.Export(item, graph));
            }

            return new SerializedData(output);
        }
    }
}