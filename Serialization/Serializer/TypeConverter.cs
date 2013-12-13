using Neon.Serialization.Converters;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    internal static class TypeConverterResolver {
        private static Dictionary<Type, ITypeConverter> _cachedAllowAllConverters = new Dictionary<Type, ITypeConverter>();
        private static Dictionary<Type, ITypeConverter> _cachedNonCyclicConverters = new Dictionary<Type, ITypeConverter>();
        private static Dictionary<Type, ITypeConverter> _cachedNonInheritanceConverters = new Dictionary<Type, ITypeConverter>();

        private static ITypeConverter CreateTypeConverter(Dictionary<Type, ITypeConverter> cache,
            Type type, bool allowCyclic, bool allowInheritance) {
            ITypeConverter converter = null;

            if (cache.TryGetValue(type, out converter) == false) {
                if (converter == null) converter = ProxyTypeConverter.TryCreate(type);
                if (converter == null) converter = PrimitiveTypeConverter.TryCreate(type);
                if (converter == null) converter = EnumTypeConverter.TryCreate(type);
                if (allowCyclic && converter == null) converter = CyclicTypeConverter.TryCreate(type);
                if (allowInheritance && converter == null) converter = InheritanceTypeConverter.TryCreate(type);
                if (converter == null) converter = CollectionTypeConverter.TryCreate(type);
                if (converter == null) converter = ReflectedTypeConverter.TryCreate(type);

                cache[type] = converter;
            }

            return converter;
        }

        public static ITypeConverter GetTypeConverter(Type type, bool allowCyclic = true, bool allowInheritance = true) {
            if (allowCyclic && allowInheritance) {
                return CreateTypeConverter(_cachedAllowAllConverters, type, allowCyclic, allowInheritance);
            }

            else if (allowCyclic == false) {
                return CreateTypeConverter(_cachedNonCyclicConverters, type, allowCyclic, allowInheritance);
            }

            else if (allowInheritance == false) {
                return CreateTypeConverter(_cachedNonInheritanceConverters, type, allowCyclic, allowInheritance);
            }

            throw new InvalidOperationException("Cannot have both allowCyclic and allowInheritance as false");
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