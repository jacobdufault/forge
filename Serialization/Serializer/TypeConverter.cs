using Neon.Serialization.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    internal static class TypeConverterResolver {
        private static Dictionary<Type, ITypeConverter> _cachedCyclicConverters = new Dictionary<Type, ITypeConverter>();
        private static Dictionary<Type, ITypeConverter> _cachedNonCyclicConverters = new Dictionary<Type, ITypeConverter>();

        private static ITypeConverter CreateTypeConverter(Type type, bool allowCyclic) {
            ITypeConverter converter = null;
            if (converter == null) converter = ProxyTypeConverter.TryCreate(type);
            if (converter == null) converter = PrimitiveTypeConverter.TryCreate(type);
            if (converter == null) converter = EnumTypeConverter.TryCreate(type);
            if (allowCyclic && converter == null) converter = CyclicTypeConverter.TryCreate(type);
            if (converter == null) converter = InheritanceTypeConverter.TryCreate(type);
            if (converter == null) converter = CollectionTypeConverter.TryCreate(type);
            if (converter == null) converter = ReflectedTypeConverter.TryCreate(type);

            return converter;
        }

        public static ITypeConverter GetTypeConverter(Type type, bool allowCyclic = true) {
            if (allowCyclic) {
                ITypeConverter converter;
                if (_cachedCyclicConverters.TryGetValue(type, out converter) == false) {
                    converter = CreateTypeConverter(type, allowCyclic);
                    _cachedCyclicConverters[type] = converter;
                }

                return converter;
            }

            else {
                ITypeConverter converter;
                if (_cachedNonCyclicConverters.TryGetValue(type, out converter) == false) {
                    converter = CreateTypeConverter(type, allowCyclic);
                    _cachedNonCyclicConverters[type] = converter;
                }

                return converter;
            }
        }
    }

    internal interface ITypeConverter {
        /// <summary>
        /// Import an instance of the given type from the given data that was emitted using Export.
        /// </summary>
        /// <param name="data">The data that was previously exported.</param>
        /// <param name="instance">An optional preallocated instance to populate.</param>
        /// <returns>An instance of the given data.</returns>
        object Import(SerializedData data, ObjectGraphReader graph);

        /// <summary>
        /// Exports an instance of the given type to a serialized state.
        /// </summary>
        /// <param name="instance">The instance to export.</param>
        /// <returns>The serialized state.</returns>
        SerializedData Export(object instance, ObjectGraphWriter graph);
    }
}