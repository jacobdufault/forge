using Neon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Neon.Serialization {
    /// <summary>
    /// Exception thrown when a type that was imported/exported requires a custom converter, but one
    /// was not registered.
    /// </summary>
    public sealed class RequiresCustomConverterException : Exception {
        private static string CreateMessage(Type type, bool importing) {
            return "The given type " + type + " requires a custom " +
                (importing ? "importer" : "exporter") + " (based on annotations), but one was " +
                "not found.";
        }

        internal RequiresCustomConverterException(Type type, bool importing)
            : base(CreateMessage(type, importing)) {
        }
    }

    /// <summary>
    /// Metadata for an annotated item inside of an object. This abstracts PropertyInfo and
    /// FieldInfo into a common interface that supports writing and reading.
    /// </summary>
    internal class PropertyMetadata {
        /// <summary>
        /// The member info that we read to and write from.
        /// </summary>
        private MemberInfo _info;

        /// <summary>
        /// The cached name of the property/field.
        /// </summary>
        public string Name;

        /// <summary>
        /// Writes a value to the given object instance using the cached member info stored inside
        /// of this metadata structure.
        /// </summary>
        public void Write(object context, object value) {
            if (_info is PropertyInfo) {
                ((PropertyInfo)_info).SetValue(context, value, new object[] { });
            }

            else {
                ((FieldInfo)_info).SetValue(context, value);
            }
        }

        /// <summary>
        /// Reads a value from the given object instance using the cached member info stored inside
        /// of this metadata structure.
        /// </summary>
        public object Read(object context) {
            if (_info is PropertyInfo) {
                return ((PropertyInfo)_info).GetValue(context, new object[] { });
            }

            else {
                return ((FieldInfo)_info).GetValue(context);
            }
        }

        /// <summary>
        /// The type of value that is stored inside of the property. For example, for an int field,
        /// StorageType will be typeof(int).
        /// </summary>
        public Type StorageType;

        /// <summary>
        /// Initializes a new instance of the PropertyMetadata class from a property member.
        /// </summary>
        public PropertyMetadata(PropertyInfo property) {
            _info = property;
            Name = property.Name;
            StorageType = property.PropertyType;
        }

        /// <summary>
        /// Initializes a new instance of the PropertyMetadata class from a field member.
        /// </summary>
        public PropertyMetadata(FieldInfo field) {
            _info = field;
            Name = field.Name;
            StorageType = field.FieldType;
        }
    }

    /// <summary>
    /// Metadata for an type instance.
    /// </summary>
    internal class TypeMetadata {
        /// <summary>
        /// Creates a new instance of the type that this metadata points back to.
        /// </summary>
        /// <remarks>
        /// Activator.CreateInstance cannot be used because TypeMetadata can point to an Array.
        /// </remarks>
        public object CreateInstance() {
            if (IsArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ElementType, 0);
            }

            return Activator.CreateInstance(_baseType);
        }

        /// <summary>
        /// Assuming that this object is a dictionary, this writes a key/value pair to the
        /// dictionary.
        /// </summary>
        public void AssignKeyValue(object context, object key, object value) {
            if (IsDictionary) {
                IDictionary dictionary = (IDictionary)context;

                if (dictionary.Contains(key)) {
                    throw new InvalidOperationException("The dictionary already contains a " +
                        "value for the given key \"" + key + "\"");
                }

                dictionary[key] = value;
            }
            else {
                throw new InvalidOperationException("Cannot assign a key/value slot to a " +
                    "non-dictionary type");
            }
        }

        /// <summary>
        /// If the context is an array, then the given value is inserted at the given indexHint. If
        /// the context is a list, then the given value is inserted at the end of the list.
        /// </summary>
        /// <remarks>
        /// The array does not have storage available at the given index, then it will be resized so
        /// that it contains exactly the right about of storage to contain indexHint.
        /// </remarks>
        public void AssignListSlot(ref object context, object value, int indexHint) {
            if (IsArray) {
                Array array = (Array)context;

                // If we don't have storage in the allocated array, then allocate storage.
                if (indexHint >= array.Length) {
                    Array newArray = Array.CreateInstance(ElementType, indexHint + 1);
                    Array.Copy(array, newArray, array.Length);
                    array = newArray;
                }

                // Assign the array index
                array.SetValue(value, indexHint);
                context = array;
            }
            else if (IsList) {
                IList list = (IList)context;
                list.Add(value);
            }
            else {
                throw new InvalidOperationException("Cannot assign a list slot to a non-list type");
            }
        }

        /// <summary>
        /// Initializes a new instance of the TypeMetadata class from a type.
        /// </summary>
        public TypeMetadata(Type type) {
            _baseType = type;

            // Iterate over all attributes in the type to check for the requirement of a custom
            // converter
            foreach (var attribute in _baseType.GetCustomAttributes(true)) {
                if (attribute is RequireCustomConverterAttribute) {
                    RequireCustomConverter = true;
                }
            }

            // determine if we are a dictionary, list, or array
            IsDictionary = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

            IsArray = type.IsArray;
            IsList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            if (IsArray && IsList) {
                throw new InvalidOperationException("Type that is both a list an and array is " +
                    "not supported by the serialization metadata model");
            }

            // If we're a list or array, get the generic type definition so that client code can
            // determine how to deserialize child elements
            if (IsList) {
                _elementType = type.GetGenericArguments()[0];
            }
            else if (IsArray) {
                _elementType = type.GetElementType();
            }
            else if (IsDictionary) {
                _elementType = type.GetGenericArguments()[1];
            }

            // If we're not one of those three types, then we will be using Properties to assign
            // data to ourselves, so we want to lookup said information
            if (IsDictionary == false && IsArray == false && IsList == false) {
                // Additional flags used to widen the property and field search, ie, we want to
                // include private fields as well
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.FlattenHierarchy;

                _properties = new List<PropertyMetadata>();
                foreach (PropertyInfo property in type.GetProperties(bindingFlags)) {
                    // We only populate properties that can be both read and written to
                    if (property.CanRead && property.CanWrite) {
                        _properties.Add(new PropertyMetadata(property));
                    }
                }

                foreach (FieldInfo field in type.GetFields(bindingFlags)) {
                    // We serialize all fields, except those annotated with [NonSerialized].
                    if (field.IsNotSerialized == false) {
                        _properties.Add(new PropertyMetadata(field));
                    }
                }
            }
        }

        /// <summary>
        /// If this type requires a custom converter, then this will be true. Generic reflection
        /// should not be done if this is true.
        /// </summary>
        public bool RequireCustomConverter {
            get;
            private set;
        }

        /// <summary>
        /// The type that this metadata is modeling.
        /// </summary>
        private Type _baseType;

        /// <summary>
        /// Iff this metadata maps back to a List or an Array type, then this is the type of element
        /// stored inside the array. If this metadata maps back to a Dictionary type, this this is
        /// the type of element stored inside of the value slot.
        /// </summary>
        private Type _elementType;
        public Type ElementType {
            get {
                if (IsArray == false && IsList == false && IsDictionary == false) {
                    throw new Exception("Unable to get the ListElementType of a " +
                        "non-list/non-dictionary metadata object");
                }

                return _elementType;
            }
        }

        /// <summary>
        /// True if the base type is a dictionary. If true, accessing Properties will throw an
        /// exception.
        /// </summary>
        public bool IsDictionary {
            get;
            private set;
        }

        /// <summary>
        /// True if the base type is a list. If true, accessing Properties will throw an exception.
        /// </summary>
        public bool IsList {
            get;
            private set;
        }

        /// <summary>
        /// True if the base type is an array. If true, accessing Properties will throw an
        /// exception.
        /// </summary>
        public bool IsArray {
            get;
            private set;
        }

        /// <summary>
        /// The properties on the type. This is used when importing/exporting a type that does not
        /// have a user-defined importer/exporter.
        /// </summary>
        private List<PropertyMetadata> _properties;
        public List<PropertyMetadata> Properties {
            get {
                if (IsDictionary || IsList || IsArray) {
                    throw new InvalidOperationException("A type that is a dictionary or list does not have properties");
                }

                return _properties;
            }
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
        /// <summary>
        /// Custom importers that convert SerializedData directly into object instances.
        /// </summary>
        private Dictionary<Type, Importer> _importers = new Dictionary<Type, Importer>();

        /// <summary>
        /// Custom exporters that convert object instances directly into SerializedData.
        /// </summary>
        private Dictionary<Type, Exporter> _exporters = new Dictionary<Type, Exporter>();

        /// <summary>
        /// Cached reflection information for types which do not have both a custom importer and a
        /// custom exporter.
        /// </summary>
        private Dictionary<Type, TypeMetadata> _typeMetadata = new Dictionary<Type, TypeMetadata>();

        /// <summary>
        /// Get the type metadata for the given type. The the metadata does not already exist inside
        /// the cache, then it is created.
        /// </summary>
        private TypeMetadata GetMetadata(Type type) {
            // Try and find it in the cache
            TypeMetadata metadata;
            if (_typeMetadata.TryGetValue(type, out metadata)) {
                return metadata;
            }

            // Its not in the cache; create it
            metadata = new TypeMetadata(type);
            _typeMetadata[type] = metadata;
            return metadata;
        }

        /// <summary>
        /// Initializes a new instance of the SerializationConverter class. Adds default importers
        /// and exporters by default; the default converters are used for converting the primitive
        /// types that SerializedData maps directly to.
        /// </summary>
        public SerializationConverter(bool addDefaultConverters = true) {
            // Register default converters
            if (addDefaultConverters) {
                // use implicit conversion operators importers convert a serialized value to a
                // non-serialized value
                AddImporter(typeof(byte), value => value.AsReal.AsInt);
                AddImporter(typeof(short), value => value.AsReal.AsInt);
                AddImporter(typeof(int), value => value.AsReal.AsInt);
                AddImporter(typeof(Real), value => value.AsReal);
                AddImporter(typeof(bool), value => value.AsBool);
                AddImporter(typeof(string), value => value.AsString);
                AddImporter(typeof(SerializedData), value => value);

                // use implicit conversion operators exporters convert the input type to a
                // serialized value
                AddExporter(typeof(byte), value => new SerializedData((Real)value));
                AddExporter(typeof(short), value => new SerializedData((Real)value));
                AddExporter(typeof(int), value => new SerializedData(Real.CreateDecimal((long)(int)value)));
                AddExporter(typeof(Real), value => new SerializedData((Real)value));
                AddExporter(typeof(bool), value => new SerializedData((bool)value));
                AddExporter(typeof(string), value => new SerializedData((string)value));
                AddExporter(typeof(SerializedData), value => (SerializedData)value);
            }
        }

        /// <summary>
        /// Registers a converter that will convert SerializedData instances to their respective
        /// destination types.
        /// </summary>
        public void AddImporter(Type destinationType, Importer importer) {
            if (_importers.ContainsKey(destinationType)) {
                throw new InvalidOperationException("There is already a registered importer for type " + destinationType);
            }

            _importers[destinationType] = importer;
        }

        /// <summary>
        /// Adds an exporter that takes instances of data and converts them to serialized data.
        /// </summary>
        /// <param name="serializedType">The instance type that this exporter can serialize.</param>
        /// <param name="exporter">The exporter itself.</param>
        public void AddExporter(Type serializedType, Exporter exporter) {
            if (_exporters.ContainsKey(serializedType)) {
                throw new InvalidOperationException("There is already a registered exporter for type " + serializedType);
            }

            _exporters[serializedType] = exporter;
        }

        /// <summary>
        /// Converts a general object instance into SerializedData. Assuming serialization is
        /// working as expected and custom importers/exporters are written correctly, calling Import
        /// on the return value with given will result in an object that is identical to instance.
        /// </summary>
        public SerializedData Export(object instance) {
            if (instance == null) {
                throw new ArgumentException("Cannot export a null object");
            }

            Type exportedType = instance.GetType();

            // If there is a user-defined exporter for the given type, then use it instead of doing
            // automated reflection.
            if (_exporters.ContainsKey(exportedType)) {
                return _exporters[exportedType](instance);
            }

            // There is no user-defined exporting function. We'll have to use reflection to populate
            // the fields of the serialized value and hope that we did a good enough job.

            TypeMetadata metadata = GetMetadata(exportedType);

            // Oops, the type requires a custom converter. We can't process this.
            if (metadata.RequireCustomConverter) {
                throw new RequiresCustomConverterException(exportedType, importing: false);
            }

            // If it's an array or a list, we have special logic for processing
            if (metadata.IsArray || metadata.IsList) {
                List<SerializedData> output = new List<SerializedData>();

                // luckily both arrays and lists are enumerable, so we'll use that interface when
                // outputting the object
                IEnumerable enumerator = (IEnumerable)instance;
                foreach (object item in enumerator) {
                    output.Add(Export(item));
                }

                return new SerializedData(output);
            }

            // If the object is a dictionary, then we populate it with all fields in the serialized
            // value
            else if (metadata.IsDictionary) {
                var dict = new Dictionary<string, SerializedData>();

                var enumerable = (IEnumerable<KeyValuePair<string, SerializedData>>)instance;
                foreach (var item in enumerable) {
                    dict[item.Key] = Export(item.Value);
                }

                return new SerializedData(dict);
            }

            // This is not an array or list; populate from reflected properties
            else {
                var dict = new Dictionary<string, SerializedData>();

                for (int i = 0; i < metadata.Properties.Count; ++i) {
                    PropertyMetadata propertyMetadata = metadata.Properties[i];
                    dict[propertyMetadata.Name] = Export(propertyMetadata.Read(instance));
                }

                return new SerializedData(dict);
            }
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
        public object Import(Type type, SerializedData serializedData) {
            // If there is a user-defined importer for the given type, then use it instead of doing
            // automated reflection.
            if (_importers.ContainsKey(type)) {
                return _importers[type](serializedData);
            }

            // There is no user-defined importer function. We'll have to use reflection to populate
            // the fields of the object, and hope that we did a good enough job.

            TypeMetadata metadata = GetMetadata(type);

            // Oops, the type requires a custom converter. We can't process this.
            if (metadata.RequireCustomConverter) {
                throw new RequiresCustomConverterException(type, importing: true);
            }

            object instance = metadata.CreateInstance();

            // If it's an array or a list, we have special logic for processing
            if (metadata.IsArray || metadata.IsList) {
                IList<SerializedData> items = serializedData.AsList;

                for (int i = 0; i < items.Count; ++i) {
                    object indexedObject = Import(metadata.ElementType, items[i]);
                    metadata.AssignListSlot(ref instance, indexedObject, i);
                }
            }

            // If the object is a dictionary, then we populate it with all fields in the serialized
            // value
            else if (metadata.IsDictionary) {
                IDictionary<string, SerializedData> dict = serializedData.AsDictionary;
                foreach (var entry in dict) {
                    string key = entry.Key;
                    object value = Import(metadata.ElementType, entry.Value);
                    metadata.AssignKeyValue(instance, key, value);
                }
            }

            // This is not an array or list; populate from reflected properties
            else {
                Dictionary<string, SerializedData> serializedDataDict = serializedData.AsDictionary;
                for (int i = 0; i < metadata.Properties.Count; ++i) {
                    PropertyMetadata propertyMetadata = metadata.Properties[i];

                    // deserialize the property
                    string name = propertyMetadata.Name;
                    Type storageType = propertyMetadata.StorageType;
                    object deserialized = Import(storageType, serializedDataDict[name]);

                    // write it into the instance
                    propertyMetadata.Write(instance, deserialized);
                }
            }

            return instance;
        }
    }
}