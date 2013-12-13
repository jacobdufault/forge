using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class InheritanceTypeConverter : ITypeConverter {
        private InheritanceTypeConverter() {
        }

        public static InheritanceTypeConverter TryCreate(Type type) {
            TypeModel model = TypeCache.GetTypeModel(type);

            if (model.SupportsInheritance) {
                return new InheritanceTypeConverter();
            }

            return null;
        }

        public object Import(SerializedData data, ObjectGraphReader graph) {
            Type instanceType = TypeCache.FindType(data.AsDictionary["InstanceType"].AsString);
            ITypeConverter converter = TypeConverterResolver.GetTypeConverter(instanceType, allowInheritance: false);

            return converter.Import(data.AsDictionary["Data"], graph);
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            Type instanceType = instance.GetType();
            Dictionary<string, SerializedData> data = new Dictionary<string, SerializedData>();

            // Export the type so we know what type to import as.
            data["InstanceType"] = new SerializedData(instanceType.FullName);

            // we want to make sure we export under the direct instance type, otherwise we'll go
            // into an infinite loop of reexporting the interface metadata.
            ITypeConverter converter = TypeConverterResolver.GetTypeConverter(instanceType, allowInheritance: false);
            data["Data"] = converter.Export(instance, graph);

            return new SerializedData(data);
        }
    }
}