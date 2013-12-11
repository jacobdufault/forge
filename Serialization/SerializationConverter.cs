using Neon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Serialization {
    internal interface ITypeConverter {
        /// <summary>
        /// Is the given type supported by this type converter?
        /// </summary>
        /// <param name="type">The type to examine</param>
        /// <returns>True if this converter can import and export the given type, otherwise
        /// false.</returns>
        bool CanConvert(TypeMetadata type);

        /// <summary>
        /// Import an instance of the given type from the given data that was emitted using Export.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <param name="data">The data that was previously exported.</param>
        /// <param name="instance">An optional preallocated instance to populate.</param>
        /// <returns>An instance of the given data.</returns>
        object Import(TypeMetadata type, SerializedData data, object instance = null);

        /// <summary>
        /// Exports an instance of the given type to a serialized state.
        /// </summary>
        /// <param name="type">The type of object to export the instance as.</param>
        /// <param name="instance">The instance to export.</param>
        /// <returns>The serialized state.</returns>
        SerializedData Export(TypeMetadata type, object instance);
    }

    internal class PrimitiveTypeConverter : ITypeConverter {

        public bool CanConvert(TypeMetadata type) {
            Type t = type.ReflectedType;
            return
                t == typeof(byte) ||
                t == typeof(short) ||
                t == typeof(int) ||
                t == typeof(Real) ||
                t == typeof(bool) ||
                t == typeof(string) ||
                t == typeof(SerializedData);
        }

        public object Import(TypeMetadata type, SerializedData data, object instance) {
            if (instance != null) {
                throw new InvalidOperationException("PrimitiveTypeConverter cannot handle " +
                    "preallocated object instances");
            }

            Type t = type.ReflectedType;

            if (t == typeof(int)) {
                return (int)data.AsReal.AsInt;
            }
            if (t == typeof(Real)) {
                return data.AsReal;
            }
            if (t == typeof(bool)) {
                return data.AsBool;
            }
            if (t == typeof(string)) {
                return data.AsString;
            }
            if (t == typeof(SerializedData)) {
                return data;
            }

            if (t == typeof(byte)) {
                return (byte)data.AsReal.AsInt;
            }
            if (t == typeof(short)) {
                return (short)data.AsReal.AsInt;
            }

            throw new InvalidOperationException("Bad type");
        }

        public SerializedData Export(TypeMetadata type, object instance) {
            Type t = type.ReflectedType;

            if (t == typeof(int)) {
                return new SerializedData(Real.CreateDecimal((int)instance));
            }
            if (t == typeof(Real)) {
                return new SerializedData((Real)instance);
            }
            if (t == typeof(bool)) {
                return new SerializedData((bool)instance);
            }
            if (t == typeof(string)) {
                return new SerializedData((string)instance);
            }
            if (t == typeof(SerializedData)) {
                return (SerializedData)instance;
            }

            if (t == typeof(byte)) {
                return new SerializedData(Real.CreateDecimal((byte)instance));
            }
            if (t == typeof(short)) {
                return new SerializedData(Real.CreateDecimal((short)instance));
            }

            throw new InvalidOperationException("Bad type");
        }
    }

    internal class CyclicTypeConverter : ITypeConverter {
        public SerializationGraphExporter ExportGraph;
        public SerializationGraphImporter ImportGraph;
        public bool Enabled;
        private SerializationConverter _converter;

        public CyclicTypeConverter(SerializationConverter converter) {
            _converter = converter;
        }

        public bool CanConvert(TypeMetadata type) {
            return Enabled && type.SupportsCyclicReferences;
        }

        public object Import(TypeMetadata type, SerializedData data, object instance) {
            if (ImportGraph == null) {
                throw new InvalidOperationException("Cycle support required in serialization " +
                    "graph; use GraphImport instead of Import");
            }
            if (instance != null) {
                _converter.Import(type.ReflectedType, data, instance, disableCyclic: true);
                return instance;
            }

            return ImportGraph.GetObjectInstance(type.ReflectedType, data.AsObjectReference);
        }

        public SerializedData Export(TypeMetadata type, object instance) {
            if (ExportGraph == null) {
                throw new InvalidOperationException("Cycle support required in serialization " +
                    "graph; use GraphExport instead of Export");
            }

            return ExportGraph.GetReferenceForObject(type.ReflectedType, instance);
        }
    }

    internal class EnumTypeConverter : ITypeConverter {
        public bool CanConvert(TypeMetadata type) {
            return type.ReflectedType.IsEnum;
        }

        public object Import(TypeMetadata type, SerializedData data, object instance) {
            if (instance != null) {
                throw new InvalidOperationException("EnumTypeConverter cannot handle " +
                    "preallocated object instances");
            }

            return Enum.Parse(type.ReflectedType, data.AsString);
        }

        public SerializedData Export(TypeMetadata type, object instance) {
            return new SerializedData(instance.ToString());
        }
    }

    internal class InheritanceTypeConverter : ITypeConverter {
        private SerializationConverter _converter;

        public InheritanceTypeConverter(SerializationConverter converter) {
            _converter = converter;
        }

        public bool CanConvert(TypeMetadata type) {
            return type.SupportsInheritance;
        }

        public object Import(TypeMetadata type, SerializedData data, object instance) {
            Type instanceType = TypeCache.FindType(data.AsDictionary["InstanceType"].AsString);
            return _converter.Import(instanceType, data.AsDictionary["Data"], instance);
        }

        public SerializedData Export(TypeMetadata type, object instance) {
            SerializedData data = SerializedData.CreateDictionary();

            // Export the type so we know what type to import as.
            data.AsDictionary["InstanceType"] = new SerializedData(instance.GetType().FullName);

            // we want to make sure we export under the direct instance type, otherwise we'll go
            // into an infinite loop of reexporting the interface metadata.
            data.AsDictionary["Data"] = _converter.Export(instance.GetType(), instance);

            return data;
        }
    }

    internal class ArrayOrCollectionTypeConverter : ITypeConverter {
        private SerializationConverter _converter;

        public ArrayOrCollectionTypeConverter(SerializationConverter converter) {
            _converter = converter;
        }

        public bool CanConvert(TypeMetadata type) {
            return type.IsArray || type.IsCollection;
        }

        public object Import(TypeMetadata type, SerializedData data, object instance) {
            if (instance == null) {
                instance = type.CreateInstance();
            }

            IList<SerializedData> items = data.AsList;
            for (int i = 0; i < items.Count; ++i) {
                object indexedObject = _converter.Import(type.ElementType, items[i]);
                type.AppendValue(ref instance, indexedObject, i);
            }

            return instance;
        }

        public SerializedData Export(TypeMetadata type, object instance) {
            // If it's an array or a collection, we have special logic for processing
            List<SerializedData> output = new List<SerializedData>();

            Type collectionElementType = type.ElementType;

            // luckily both arrays and collections are enumerable, so we'll use that interface when
            // outputting the object
            IEnumerable enumerator = (IEnumerable)instance;
            foreach (object item in enumerator) {
                // make sure we export under the element type of the list; if we don't, and say, the
                // element type is an interface, and we export under the instance type, then
                // deserialization will not work as expected (because we'll be trying to deserialize
                // an interface).
                output.Add(_converter.Export(collectionElementType, item));
            }

            return new SerializedData(output);
        }
    }

    internal class ReflectedTypeConverter : ITypeConverter {
        private SerializationConverter _converter;

        public ReflectedTypeConverter(SerializationConverter converter) {
            _converter = converter;
        }

        public bool CanConvert(TypeMetadata type) {
            return true;
        }

        public object Import(TypeMetadata type, SerializedData data, object instance) {
            if (instance == null) {
                instance = type.CreateInstance();
            }

            Dictionary<string, SerializedData> serializedDataDict = data.AsDictionary;
            for (int i = 0; i < type.Properties.Count; ++i) {
                PropertyMetadata propertyMetadata = type.Properties[i];

                // deserialize the property
                string name = propertyMetadata.Name;
                Type storageType = propertyMetadata.StorageType;

                // throw if the dictionary is missing a required property
                if (serializedDataDict.ContainsKey(name) == false) {
                    throw new Exception("There is no key " + name + " in serialized data "
                        + serializedDataDict + " when trying to deserialize an instance of " +
                        type);
                }

                object deserialized = _converter.Import(storageType, serializedDataDict[name]);

                // write it into the instance
                propertyMetadata.Write(instance, deserialized);
            }

            return instance;
        }

        public SerializedData Export(TypeMetadata type, object instance) {
            var dict = new Dictionary<string, SerializedData>();

            for (int i = 0; i < type.Properties.Count; ++i) {
                PropertyMetadata propertyMetadata = type.Properties[i];

                // make sure we export under the storage type of the property; if we don't, and say,
                // the property is an interface, and we export under the instance type, then
                // deserialization will not work as expected (because we'll be trying to deserialize
                // an interface).
                dict[propertyMetadata.Name] = _converter.Export(propertyMetadata.StorageType,
                    propertyMetadata.Read(instance));
            }

            return new SerializedData(dict);
        }
    }

    /// <summary>
    /// Converts a given serialized value into an instance of an object.
    /// </summary>
    /// <param name="serializedData">The serialized value to convert.</param>
    public delegate object Importer(SerializedData serializedData);

    /// <summary>
    /// Deserialize a given object instance into a serialized value.
    /// </summary>
    /// <param name="instance">The object instance to serialize.</param>
    public delegate SerializedData Exporter(object instance);

    /// <summary>
    /// Converts types to and from SerializedDatas.
    /// </summary>
    public class SerializationConverter {
        private List<ITypeConverter> _converters;
        private CyclicTypeConverter _cyclicTypeConverter;

        /// <summary>
        /// Initializes a new instance of the SerializationConverter class. Adds default importers
        /// and exporters by default; the default converters are used for converting the primitive
        /// types that SerializedData maps directly to.
        /// </summary>
        public SerializationConverter() {
            _cyclicTypeConverter = new CyclicTypeConverter(this);

            _converters = new List<ITypeConverter>() {
                new PrimitiveTypeConverter(),
                new EnumTypeConverter(),
                _cyclicTypeConverter,
                new InheritanceTypeConverter(this),
                new ArrayOrCollectionTypeConverter(this),
                new ReflectedTypeConverter(this)
            };
        }

        /// <summary>
        /// Converts a general object instance into SerializedData.
        /// </summary>
        /// <remarks>
        /// Though the magic of generic type inference, using this method works nicely and correctly
        /// supports interfaces when the known instance type is not concrete. For example,
        /// Export((iface)inst) will dispatch to Export(typeof(iface), inst) instead of
        /// Export(inst.GetType(), inst). The second one will not import correctly, as the client is
        /// only going to know that the base type of serialized object, not the type itself. In
        /// essence, using a generic limits the information of the Export function to that of the
        /// Import function.
        /// </remarks>
        public SerializedData Export<T>(T instance) {
            return Export(typeof(T), instance);
        }

        /// <summary>
        /// Converts a general object instance into SerializedData. The object instance may
        /// internally store a set of circular references.
        /// </summary>
        /// <param name="type">The type of object to export.</param>
        /// <param name="instance">The instance itself.</param>
        /// <returns>A serialized data instance that can fully restore the given object graph as
        /// defined by the given object instance.</returns>
        public SerializedData ExportGraph(Type type, object instance) {
            if (_cyclicTypeConverter.Enabled == false) {
                try {
                    _cyclicTypeConverter.Enabled = true;
                    _cyclicTypeConverter.ExportGraph = new SerializationGraphExporter();

                    SerializedData data = Export(type, instance);

                    SerializedData result = SerializedData.CreateDictionary();
                    result.AsDictionary["PrimaryData"] = data;

                    //_cyclicTypeConverter.Enabled = false;
                    result.AsDictionary["SupportGraph"] = _cyclicTypeConverter.ExportGraph.Export(this);

                    return result;
                }
                finally {
                    _cyclicTypeConverter.Enabled = false;
                    _cyclicTypeConverter.ExportGraph = null;
                }
            }

            return Export(type, instance);
        }

        public object ImportGraph(Type type, SerializedData data) {
            if (_cyclicTypeConverter.Enabled == false) {
                try {
                    _cyclicTypeConverter.Enabled = true;
                    _cyclicTypeConverter.ImportGraph = new SerializationGraphImporter(data.AsDictionary["SupportGraph"]);

                    object result = Import(type, data.AsDictionary["PrimaryData"]);

                    //_cyclicTypeConverter.Enabled = false;
                    _cyclicTypeConverter.ImportGraph.RestoreGraph(this);

                    return result;
                }
                finally {
                    _cyclicTypeConverter.Enabled = false;
                    _cyclicTypeConverter.ImportGraph = null;
                }
            }

            return Import(type, data);
        }

        /// <summary>
        /// Converts a general object instance into SerializedData. Assuming serialization is
        /// working as expected and custom importers/exporters are written correctly, calling Import
        /// on the return value with given will result in an object that is identical to instance.
        /// </summary>
        /// <param name="instanceType">The type to use when we export instance. instance *must* be
        /// an instance of instanceType, though instanceType can be anywhere on the hierarchy for
        /// instance (for example, it could be typeof(object).</param>
        /// <param name="instance">The object instance to export. If it is not null, then it must be
        /// an instance of instanceType</param>
        public SerializedData Export(Type instanceType, object instance, bool disableCyclicExport = false) {
            Log<SerializationConverter>.Info("Exporting " + instance + " with type " + instanceType);

            // special case for null values
            if (instance == null) {
                return new SerializedData();
            }

            TypeMetadata metadata = TypeCache.GetMetadata(instanceType);
            for (int i = 0; i < _converters.Count; ++i) {
                ITypeConverter converter = _converters[i];
                if (disableCyclicExport && converter is CyclicTypeConverter) {
                    continue;
                }

                if (converter.CanConvert(metadata)) {
                    return converter.Export(metadata, instance);
                }
            }

            throw new InvalidOperationException("No converter for type = " + instanceType);
        }

        /// <summary>
        /// Wrapper around Import(Type, SerializedData).
        /// </summary>
        public T Import<T>(SerializedData serializedData) {
            return (T)Import(typeof(T), serializedData);
        }

        /// <summary>
        /// Converts SerializedData into a general object instance. Assuming serialization is
        /// working as expected and custom importers/exporters are written correctly, calling Export
        /// on the return value with given will result in an object that is identical to
        /// serializedData.
        /// </summary>
        /// <param name="type">The type of data to import.</param>
        /// <param name="serializedData">The data to use when importing the object.</param>
        /// <param name="instance">An optional instance of the given type to allocate data
        /// into.</param>
        public object Import(Type type, SerializedData serializedData, object instance = null, bool disableCyclic = false) {
            Log<SerializationConverter>.Info("Importing " + serializedData.PrettyPrinted +
                " with type " + type);

            // special case for null values
            if (serializedData.IsNull) {
                return null;
            }

            TypeMetadata metadata = TypeCache.GetMetadata(type);
            for (int i = 0; i < _converters.Count; ++i) {
                ITypeConverter converter = _converters[i];
                if (disableCyclic && converter is CyclicTypeConverter) {
                    continue;
                }

                if (converter.CanConvert(metadata)) {
                    return converter.Import(metadata, serializedData, instance);
                }
            }

            throw new InvalidOperationException("No converter for type = " + type);
        }
    }
}