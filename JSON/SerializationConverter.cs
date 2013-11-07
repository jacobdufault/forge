using Neon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Neon.Serialization {
    /// <summary>
    /// Metadata for an annotated item inside of an object.
    /// </summary>
    internal class PropertyMetadata {
        public MemberInfo Info;
        public bool IsField;
        public bool IsPrivate;
        public string Name;

        public void Write(object context, object value) {
            if (Info is PropertyInfo) {
                ((PropertyInfo)Info).SetValue(context, value, new object[] { });
            }

            else {
                ((FieldInfo)Info).SetValue(context, value);
            }
        }

        public object Read(object context) {
            if (Info is PropertyInfo) {
                return ((PropertyInfo)Info).GetValue(context, new object[] { });
            }

            else {
                return ((FieldInfo)Info).GetValue(context);
            }
        }

        public Type StorageType;

        public PropertyMetadata(PropertyInfo property) {
            IsPrivate = false;
            IsField = false;
            Info = property;
            Name = property.Name;
            StorageType = property.PropertyType;
        }

        public PropertyMetadata(FieldInfo field) {
            IsPrivate = field.IsPrivate;
            IsField = true;
            Info = field;
            Name = field.Name;
            StorageType = field.FieldType;
        }
    }

    /// <summary>
    /// Metadata for an type instance.
    /// </summary>
    internal class TypeMetadata {
        public object CreateInstance() {
            if (IsArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ElementType, 0);
            }

            return Activator.CreateInstance(_baseType);
        }

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

        public TypeMetadata(Type type) {
            _baseType = type;

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
        public bool IsDictionary;

        /// <summary>
        /// True if the base type is a list. If true, accessing Properties will throw an exception.
        /// </summary>
        public bool IsList;

        /// <summary>
        /// True if the base type is an array. If true, accessing Properties will throw an
        /// exception.
        /// </summary>
        public bool IsArray;

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
        private Dictionary<Type, Importer> _importers = new Dictionary<Type, Importer>();
        private Dictionary<Type, Exporter> _exporters = new Dictionary<Type, Exporter>();
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
        /// <param name="destinationType"></param>
        /// <param name="converter"></param>
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

        public T Import<T>(SerializedData serializedData) {
            return (T)Import(typeof(T), serializedData);
        }

        public object Import(Type type, SerializedData serializedData) {
            // If there is a user-defined importer for the given type, then use it instead of doing
            // automated reflection.
            if (_importers.ContainsKey(type)) {
                return _importers[type](serializedData);
            }

            // There is no user-defined importer function. We'll have to use reflection to populate
            // the fields of the object, and hope that we did a good enough job.

            TypeMetadata metadata = GetMetadata(type);
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