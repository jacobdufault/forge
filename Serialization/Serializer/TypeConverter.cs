using Neon.Serialization.Converters;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    internal interface ITypeConverter {
        /// <summary>
        /// Import an instance of the given type from the given data that was emitted using Export.
        /// </summary>
        /// <param name="data">The data that was previously exported.</param>
        /// <param name="graph">A graph used to get an object instance if instance is null</param>
        /// <param name="instance">An optional preallocated instance to populate.</param>
        /// <returns>An instance of the given data.</returns>
        object Import(SerializedData data, ObjectGraphReader graph, object instance);

        /// <summary>
        /// Exports an instance of the given type to a serialized state.
        /// </summary>
        /// <param name="instance">The instance to export.</param>
        /// <returns>The serialized state.</returns>
        SerializedData Export(object instance, ObjectGraphWriter graph);
    }
}