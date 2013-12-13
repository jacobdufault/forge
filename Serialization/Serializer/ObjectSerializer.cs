using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    public static class ObjectSerializer {
        public static T Import<T>(SerializedData data) {
            return (T)Import(typeof(T), data);
        }

        public static object Import(Type type, SerializedData data) {
            ObjectGraphReader graph = new ObjectGraphReader(data.AsDictionary["Graph"]);
            graph.RestoreGraph();

            ITypeConverter converter = TypeConverterResolver.GetTypeConverter(type);
            return converter.Import(data.AsDictionary["Data"], graph);
        }

        public static SerializedData Export<T>(T instance) {
            return Export(typeof(T), instance);
        }

        public static SerializedData Export(Type type, object instance) {
            Dictionary<string, SerializedData> data = new Dictionary<string, SerializedData>();

            ObjectGraphWriter graph = new ObjectGraphWriter();

            ITypeConverter converter = TypeConverterResolver.GetTypeConverter(type);
            data["Data"] = converter.Export(instance, graph);

            data["Graph"] = graph.Export();

            return new SerializedData(data);
        }
    }
}