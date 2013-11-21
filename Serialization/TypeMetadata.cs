using Neon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

// This file contains a number of classes which make reflection easier. These are used heavily by
// the SerializationConverter, but are exposed publicly so that all code can use the same reflection
// base for serialization related purposes.

namespace Neon.Serialization {
    /// <summary>
    /// Metadata for an annotated item inside of an object. This abstracts PropertyInfo and
    /// FieldInfo into a common interface that supports writing and reading.
    /// </summary>
    public class PropertyMetadata {
        /// <summary>
        /// The member info that we read to and write from.
        /// </summary>
        public MemberInfo MemberInfo {
            get;
            private set;
        }

        /// <summary>
        /// The cached name of the property/field.
        /// </summary>
        public string Name;

        /// <summary>
        /// Writes a value to the given object instance using the cached member info stored inside
        /// of this metadata structure.
        /// </summary>
        public void Write(object context, object value) {
            if (MemberInfo is PropertyInfo) {
                ((PropertyInfo)MemberInfo).SetValue(context, value, new object[] { });
            }

            else {
                ((FieldInfo)MemberInfo).SetValue(context, value);
            }
        }

        /// <summary>
        /// Reads a value from the given object instance using the cached member info stored inside
        /// of this metadata structure.
        /// </summary>
        public object Read(object context) {
            if (MemberInfo is PropertyInfo) {
                return ((PropertyInfo)MemberInfo).GetValue(context, new object[] { });
            }

            else {
                return ((FieldInfo)MemberInfo).GetValue(context);
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
            MemberInfo = property;
            Name = property.Name;
            StorageType = property.PropertyType;
        }

        /// <summary>
        /// Initializes a new instance of the PropertyMetadata class from a field member.
        /// </summary>
        public PropertyMetadata(FieldInfo field) {
            MemberInfo = field;
            Name = field.Name;
            StorageType = field.FieldType;
        }

        public override bool Equals(System.Object obj) {
            // If parameter is null return false.
            if (obj == null) {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            PropertyMetadata p = obj as PropertyMetadata;
            if ((System.Object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (StorageType == p.StorageType) && (Name == p.Name);
        }

        public bool Equals(PropertyMetadata p) {
            // If parameter is null return false:
            if ((object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (StorageType == p.StorageType) && (Name == p.Name);
        }

        public override int GetHashCode() {
            return StorageType.GetHashCode() ^ Name.GetHashCode();
        }
    }

    /// <summary>
    /// Metadata for an type instance. The type can point to a regular type, an array, a list, or a
    /// dictionary.
    /// </summary>
    public class TypeMetadata {
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

            return Activator.CreateInstance(ReflectedType);
        }

        /// <summary>
        /// Assuming that this object is a dictionary, this writes a key/value pair to the
        /// dictionary.
        /// </summary>
        /// <remarks>
        /// This throws an exception if the context (as a dictionary) already has a value for the
        /// given key.
        /// </remarks>
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
            ReflectedType = type;

            // Iterate over all attributes in the type to check for the requirement of a custom
            // converter
            foreach (var attribute in ReflectedType.GetCustomAttributes(inherit: true)) {
                if (attribute is SerializationRequireCustomConverterAttribute) {
                    RequiresCustomConverter = true;
                }
            }

            // Determine if the type needs to support inheritance
            SupportsInheritance = type.IsInterface || type.IsAbstract;
            if (!SupportsInheritance) {
                foreach (var attribute in ReflectedType.GetCustomAttributes(inherit: false)) {
                    if (attribute is SerializationSupportInheritance) {
                        SupportsInheritance = true;
                    }
                }
            }

            // But do not support it if inheritance is explicitly denied
            if (SupportsInheritance) {
                foreach (var attribute in ReflectedType.GetCustomAttributes(inherit: true)) {
                    if (attribute is SerializationNoAutoInheritance) {
                        SupportsInheritance = false;
                    }
                }
            }

            if (RequiresCustomConverter && SupportsInheritance) {
                throw new InvalidOperationException("A type cannot both support inheritance and " +
                    "have a custom converter; inheritance support consumes the converter");
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
                HashSet<PropertyMetadata> properties = new HashSet<PropertyMetadata>();
                CollectProperties(type, properties);
                _properties = new List<PropertyMetadata>(properties);
            }
        }

        /// <summary>
        /// Recursive method that adds all of the properties and fields from the given type into the
        /// given list.
        /// </summary>
        /// <param name="type">The type to process. This method will recurse up the type's
        /// inheritance hierarchy</param>
        /// <param name="properties">The list of properties that should be appended to</param>
        private static void CollectProperties(Type type, HashSet<PropertyMetadata> properties) {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            foreach (PropertyInfo property in type.GetProperties(flags)) {
                // We don't serialize delegates
                if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) {
                    Log<TypeMetadata>.Info("Ignoring property {0}.{1} because it is a delegate",
                        type.FullName, property.Name);
                    continue;
                }

                // We don't serialize properties marked with [NonSerialized] or [NotSerializable]
                foreach (var attribute in property.GetCustomAttributes(true)) {
                    if (attribute is NonSerializedAttribute || attribute is NotSerializableAttribute) {
                        Log<TypeMetadata>.Info("Ignoring property {0}.{1} because it has a " +
                            "NonSerialized or a NotSerializable attribute", type.FullName,
                            property.Name);
                        goto loop_end;
                    }
                }

                // If the property cannot be both read and written to, we don't serialize it
                if (property.CanRead == false || property.CanWrite == false) {
                    Log<TypeMetadata>.Info("Ignoring property {0}.{1} because it cannot both be " +
                        "read from and written to", type.FullName, property.Name);
                    continue;
                }

                // If the property is named "Item", it might be the this[int] indexer, which in that
                // case we don't serialize it We cannot just compare with "Item" because of explicit
                // interfaces, where the name of the property will be the full method name.
                if (property.Name.EndsWith("Item")) {
                    ParameterInfo[] parameters = property.GetIndexParameters();
                    if (parameters.Length == 1) {
                        goto loop_end;
                    }
                }

                properties.Add(new PropertyMetadata(property));

            loop_end: { }
            }

            foreach (FieldInfo field in type.GetFields(flags)) {
                // We don't serialize delegates
                if (typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                    Log<TypeMetadata>.Info("Ignoring field {0}.{1} because it is a delegate",
                        type.FullName, field.Name);
                    continue;
                }

                // We don't serialize non-serializable properties
                if (field.IsNotSerialized) {
                    Log<TypeMetadata>.Info("Ignoring field {0}.{1} because it is marked " +
                        "NoNSerialized", type.FullName, field.Name);
                    continue;
                }

                // We don't serialize fields marked with [NonSerialized] or [NotSerializable]
                foreach (var attribute in field.GetCustomAttributes(true)) {
                    if (attribute is NonSerializedAttribute || attribute is NotSerializableAttribute) {
                        Log<TypeMetadata>.Info("Ignoring field {0}.{1} because it has a " +
                            "NonSerialized or a NotSerializable attribute", type.FullName,
                            field.Name);
                        goto loop_end;
                    }
                }

                // We don't serialize compiler generated fields (an example would be a backing field
                // for an automatically generated property).
                if (field.IsDefined(typeof(CompilerGeneratedAttribute), false)) {
                    continue;
                }

                properties.Add(new PropertyMetadata(field));

            loop_end: { }
            }

            if (type.BaseType != null) {
                CollectProperties(type.BaseType, properties);
            }
        }

        /// <summary>
        /// If this type requires a custom converter, then this will be true. Generic reflection
        /// should not be done if this is true.
        /// </summary>
        public bool RequiresCustomConverter {
            get;
            private set;
        }

        /// <summary>
        /// If this type needs to support inheritance, then this will be true.
        /// </summary>
        public bool SupportsInheritance {
            get;
            private set;
        }

        /// <summary>
        /// The type that this metadata is modeling.
        /// </summary>
        public Type ReflectedType {
            get;
            private set;
        }

        /// <summary>
        /// Iff this metadata maps back to a List or an Array type, then this is the type of element
        /// stored inside the array. If this metadata maps back to a Dictionary type, this this is
        /// the type of element stored inside of the value slot. Otherwise, this method throws an
        /// exception.
        /// </summary>
        public Type ElementType {
            get {
                if (IsArray == false && IsList == false && IsDictionary == false) {
                    throw new InvalidOperationException("Unable to get the ListElementType of a " +
                        "non-list/non-dictionary metadata object");
                }

                return _elementType;
            }
        }
        private Type _elementType;

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
        public List<PropertyMetadata> Properties {
            get {
                if (IsDictionary || IsList || IsArray) {
                    throw new InvalidOperationException("A type that is a dictionary or list " +
                        "does not have properties");
                }

                return _properties;
            }
        }
        private List<PropertyMetadata> _properties;
    }
}