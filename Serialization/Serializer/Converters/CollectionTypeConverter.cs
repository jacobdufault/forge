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

        public object Import(SerializedData data, ObjectGraphReader graph) {
            object instance = graph.GetObjectInstance(_model, data);

            IList<SerializedData> items = data.AsList;

            _model.CollectionSizeHint(ref instance, items.Count);
            for (int i = 0; i < items.Count; ++i) {
                object indexedObject = _elementConverter.Import(items[i], graph);
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