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

using Forge.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Forge.Utilities {
    /// <summary>
    /// Provides a view of an arbitrary type that unifies a number of discrete concepts in the CLR.
    /// Arrays and Collection types have special support, but their APIs are unified by the
    /// TypeMetadata so that they can be treated as if they were a regular type.
    /// </summary>
    public sealed class TypeMetadata {
        /// <summary>
        /// Creates a new instance of the type that this metadata points back to.
        /// </summary>
        /// <remarks>
        /// Activator.CreateInstance cannot be used because TypeMetadata can point to an Array.
        /// </remarks>
        public object CreateInstance() {
            if (_isArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ElementType, 0);
            }

            try {
                return Activator.CreateInstance(ReflectedType);
            }
            catch (MissingMethodException e) {
                throw new InvalidOperationException("Unable to create instance of " + ReflectedType.FullName + "; there is no default constructor", e);
            }
            catch (TargetInvocationException e) {
                throw new InvalidOperationException("Constructor of " + ReflectedType.FullName + " threw an exception when creating an instance", e);
            }
            catch (MemberAccessException e) {
                throw new InvalidOperationException("Unable to access constructor of " + ReflectedType.FullName, e);
            }
        }

        /// <summary>
        /// Hint that there will be size number of elements stored in the given collection. This
        /// method is completely optional, but if the collection is an array then it will improve
        /// the performance of AppendValue.
        /// </summary>
        /// <param name="context">The collection type.</param>
        /// <param name="size">The size hint to use for the collection</param>
        public void CollectionSizeHint(ref object context, int size) {
            if (_isArray) {
                Array array = (Array)context;

                // If we don't have storage in the allocated array, then allocate storage.
                if (size > array.Length) {
                    Array newArray = Array.CreateInstance(ElementType, size);
                    Array.Copy(array, newArray, array.Length);
                    context = newArray;
                }
            }
        }

        /// <summary>
        /// Appends a value to the end of the array or collection. If the collection is an array,
        /// then the item is added to array[indexHint]. If it is a collection, then the item is just
        /// appended to the end of the collection.
        /// </summary>
        public void AppendValue(ref object context, object value, int indexHint) {
            if (IsCollection == false) {
                throw new InvalidOperationException("Cannot append a value to a non-collection type");
            }

            if (_isArray) {
                Array array = (Array)context;

                // Ensure that we have storage at the given index
                CollectionSizeHint(ref context, indexHint + 1);

                // Assign the array index
                array.SetValue(value, indexHint);
                context = array;
            }
            else {
                _collectionAddMethod.Invoke(context, new object[] { value });
            }
        }

        /// <summary>
        /// Initializes a new instance of the TypeMetadata class from a type. Use TypeCache to get
        /// instances of TypeMetadata; do not use this constructor directly.
        /// </summary>
        internal TypeMetadata(Type type) {
            ReflectedType = type;

            // determine if we are a collection or array; recall that arrays implement the
            // ICollection interface, however

            _isArray = type.IsArray;
            IsCollection = _isArray || type.IsImplementationOf(typeof(ICollection<>));

            // If we're a collection or array, get the generic type definition so that client code
            // can determine how to deserialize child elements
            if (_isArray == false && IsCollection) {
                Type collectionType = type.GetInterface(typeof(ICollection<>));

                _elementType = collectionType.GetGenericArguments()[0];
                _collectionAddMethod = collectionType.GetMethod("Add");
                if (_collectionAddMethod == null) {
                    throw new InvalidOperationException("Unable to get Add method for type " + collectionType);
                }
            }
            else if (_isArray) {
                _elementType = type.GetElementType();
            }

            // If we're not one of those three types, then we will be using Properties to assign
            // data to ourselves, so we want to lookup said information
            else {
                HashSet<PropertyMetadata> properties = new HashSet<PropertyMetadata>();
                CollectProperties(type, properties);
                _properties = new List<PropertyMetadata>(properties);
            }
        }

        /// <summary>
        /// Recursive method that adds all of the properties and fields from the given type into the
        /// given list. This method recurses up on the inheritance hierarchy.
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
                    if (attribute is NonSerializedAttribute) {
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

                // We don't serialize fields marked with [NonSerialized]
                foreach (var attribute in field.GetCustomAttributes(true)) {
                    if (attribute is NonSerializedAttribute) {
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
        /// The type that this metadata is modeling, ie, the type that the metadata was constructed
        /// off of.
        /// </summary>
        public Type ReflectedType {
            get;
            private set;
        }

        /// <summary>
        /// Iff this metadata maps back to a Collection or an Array type, then this is the type of
        /// element stored inside the array or collection. Otherwise, this method throws an
        /// exception.
        /// </summary>
        public Type ElementType {
            get {
                if (IsCollection == false) {
                    throw new InvalidOperationException("Unable to get the ElementType of a " +
                        "type metadata object that is not a collection");
                }

                return _elementType;
            }
        }
        private Type _elementType;

        /// <summary>
        /// True if the base type is a collection. If true, accessing Properties will throw an
        /// exception.
        /// </summary>
        public bool IsCollection {
            get;
            private set;
        }

        /// <summary>
        /// The cached Add method in ICollection[T]. This only contains a value if IsCollection is
        /// true.
        /// </summary>
        private MethodInfo _collectionAddMethod;

        /// <summary>
        /// True if the base type is an array. If true, accessing Properties will throw an
        /// exception. IsCollection is also true if _isArray is true.
        /// </summary>
        private bool _isArray;

        /// <summary>
        /// The properties on the type. This is used when importing/exporting a type that does not
        /// have a user-defined importer/exporter.
        /// </summary>
        public List<PropertyMetadata> Properties {
            get {
                if (IsCollection) {
                    throw new InvalidOperationException("A type that is a collection or an array " +
                        "does not have properties (for the metadata on type " + ReflectedType + ")");
                }

                return _properties;
            }
        }
        private List<PropertyMetadata> _properties;

        /// <summary>
        /// Attempts to remove the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to remove.</param>
        /// <returns>True if the property was removed, false if it was not found.</returns>
        public bool RemoveProperty(string propertyName) {
            for (int i = 0; i < _properties.Count; ++i) {
                if (_properties[i].Name == propertyName) {
                    _properties.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}