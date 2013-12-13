using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class ReflectedTypeConverter : ITypeConverter {
        private TypeModel _model;

        private ReflectedTypeConverter(Type type) {
            _model = TypeCache.GetTypeModel(type);
        }

        public static ReflectedTypeConverter TryCreate(Type type) {
            return new ReflectedTypeConverter(type);
        }

        public object Import(SerializedData data, ObjectGraphReader graph, object instance) {
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

                ITypeConverter converter = TypeConverterResolver.GetTypeConverter(storageType);
                object deserialized = converter.Import(serializedDataDict[name], graph, null);

                // write it into the instance
                property.Write(instance, deserialized);
            }

            return instance;
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            var dict = new Dictionary<string, SerializedData>();

            for (int i = 0; i < _model.Properties.Count; ++i) {
                PropertyModel property = _model.Properties[i];

                // make sure we export under the storage type of the property; if we don't, and say,
                // the property is an interface, and we export under the instance type, then
                // deserialization will not work as expected (because we'll be trying to deserialize
                // an interface).
                ITypeConverter converter = TypeConverterResolver.GetTypeConverter(property.StorageType);
                dict[property.Name] = converter.Export(property.Read(instance), graph);
            }

            return new SerializedData(dict);
        }
    }

}