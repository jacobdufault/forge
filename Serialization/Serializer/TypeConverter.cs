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

using Neon.Serialization.Converters;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    internal abstract class BaseTypeConverter {
        protected abstract object DoImport(SerializedData data, ObjectGraphReader graph, object instance);
        protected abstract SerializedData DoExport(object instance, ObjectGraphWriter graph);

        private bool _runAnnotations;

        protected BaseTypeConverter(bool runAnnotations = true) {
            _runAnnotations = runAnnotations;
        }

        /// <summary>
        /// Import an instance of the given type from the given data that was emitted using Export.
        /// </summary>
        /// <param name="data">The data that was previously exported.</param>
        /// <param name="graph">A graph used to get an object instance if instance is null</param>
        /// <param name="instance">An optional preallocated instance to populate.</param>
        /// <returns>An instance of the given data.</returns>
        public object Import(SerializedData data, ObjectGraphReader graph, object instance) {
            instance = DoImport(data, graph, instance);

            if (_runAnnotations) {
                TypeModel model = TypeCache.GetTypeModel(instance.GetType());
                if (model.IsCollection == false) {
                    foreach (var method in model.AfterImport) {
                        method(instance);
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Exports an instance of the given type to a serialized state.
        /// </summary>
        /// <param name="instance">The instance to export.</param>
        /// <returns>The serialized state.</returns>
        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            return DoExport(instance, graph);
        }

    }
}