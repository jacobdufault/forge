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

namespace Neon.Serialization {
    public static class ObjectSerializer {
        public static T Import<T>(SerializedData data) {
            return (T)Import(typeof(T), data);
        }

        public static object Import(Type type, SerializedData data) {
            ObjectGraphReader graph = new ObjectGraphReader(data.AsDictionary["Graph"]);
            graph.RestoreGraph();

            // if the actual exported data is an object reference, then we don't want to deserialize
            // the object again, so we just return it directly from the graph
            if (data.AsDictionary["Data"].IsObjectReference) {
                return graph.GetObjectInstance(TypeCache.GetTypeModel(type), data.AsDictionary["Data"]);
            }

            // otherwise we need to actually deserialize the object
            BaseTypeConverter converter = TypeConverterResolver.GetTypeConverter(type);
            return converter.Import(data.AsDictionary["Data"], graph, null);
        }

        public static SerializedData Export<T>(T instance) {
            return Export(typeof(T), instance);
        }

        public static SerializedData Export(Type type, object instance) {
            Dictionary<string, SerializedData> data = new Dictionary<string, SerializedData>();

            ObjectGraphWriter graph = new ObjectGraphWriter();

            BaseTypeConverter converter = TypeConverterResolver.GetTypeConverter(type);
            data["Data"] = converter.Export(instance, graph);

            data["Graph"] = graph.Export();

            return new SerializedData(data);
        }
    }
}