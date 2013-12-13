using Neon.Serialization.Converters;
using System;
using System.Collections.Generic;

namespace Neon.Serialization {
    internal static class TypeConverterResolver {
        /// <summary>
        /// Type converter that doesn't do anything but act as the start of the type conversion
        /// fetch chain.
        /// </summary>
        private class FakeTypeConverter : ITypeConverter {
            public object Import(SerializedData data, ObjectGraphReader graph, object instance) {
                throw new NotImplementedException();
            }

            public SerializedData Export(object instance, ObjectGraphWriter graph) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Attempts to lookup a type converter for the given type and the given current converter
        /// in the cache. Creates the secondary dictionary if it does not already exist for the
        /// given type.
        /// </summary>
        private static bool TryConverterLookup<TCurrentConverter>(Type type, out ITypeConverter converter) {
            Dictionary<Type, ITypeConverter> converters;
            if (_cachedConverters.TryGetValue(type, out converters) == false) {
                _cachedConverters[type] = new Dictionary<Type, ITypeConverter>();
                converter = null;
                return false;
            }

            return converters.TryGetValue(typeof(TCurrentConverter), out converter);
        }

        /// <summary>
        /// Cached type converters.
        /// </summary>
        /// <remarks>
        /// cache[baseType][currentConverterType] = nextConverter
        /// </remarks>
        private static Dictionary<Type, Dictionary<Type, ITypeConverter>> _cachedConverters =
            new Dictionary<Type, Dictionary<Type, ITypeConverter>>();

        /// <summary>
        /// Returns the first usable type converter for the given type. There maybe be multiple type
        /// converters that can be used; to retrieve the next one, call GetNextConverter[].
        /// </summary>
        /// <param name="type">The type to fetch the converter for.</param>
        /// <returns>A type converter.</returns>
        public static ITypeConverter GetTypeConverter(Type type) {
            return GetNextConverter<FakeTypeConverter>(type);
        }

        /// <summary>
        /// Returns the next type converter for the given type, assuming that the current one is
        /// use.
        /// </summary>
        /// <typeparam name="TCurrentConverter">The type of the current converter.</typeparam>
        /// <param name="type">The type to fetch the next converter for.</param>
        /// <returns>The next type converter for the given type.</returns>
        public static ITypeConverter GetNextConverter<TCurrentConverter>(Type type)
            where TCurrentConverter : ITypeConverter {

            // remarks: if this method is slow, we can use a DataMap type system to speed it up

            ITypeConverter converter;

            if (TryConverterLookup<TCurrentConverter>(type, out converter) == false) {
                bool allow = typeof(TCurrentConverter) == typeof(FakeTypeConverter);
                if (allow) converter = ProxyTypeConverter.TryCreate(type);

                allow = allow || typeof(TCurrentConverter) == typeof(ProxyTypeConverter);
                if (allow && converter == null) converter = PrimitiveTypeConverter.TryCreate(type);

                allow = allow || typeof(TCurrentConverter) == typeof(PrimitiveTypeConverter);
                if (allow && converter == null) converter = EnumTypeConverter.TryCreate(type);

                allow = allow || typeof(TCurrentConverter) == typeof(EnumTypeConverter);
                if (allow && converter == null) converter = CyclicTypeConverter.TryCreate(type);

                allow = allow || typeof(TCurrentConverter) == typeof(CyclicTypeConverter);
                if (allow && converter == null) converter = InheritanceTypeConverter.TryCreate(type);

                allow = allow || typeof(TCurrentConverter) == typeof(InheritanceTypeConverter);
                if (allow && converter == null) converter = CollectionTypeConverter.TryCreate(type);

                allow = allow || typeof(TCurrentConverter) == typeof(CollectionTypeConverter);
                if (allow && converter == null) converter = ReflectedTypeConverter.TryCreate(type);

                if (allow == false) {
                    throw new InvalidOperationException("Unsupported ITypeConverter type passed to GetNextConverter");
                }

                _cachedConverters[type][typeof(TCurrentConverter)] = converter;
            }

            return converter;
        }
    }
}