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

using System;
using System.Reflection;

namespace Forge.Utilities {
    /// <summary>
    /// A PropertyMetadata describes a discovered property or field in a TypeMetadata.
    /// </summary>
    public sealed class PropertyMetadata {
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
        /// Writes a value to the property that this property metadata represents, using given
        /// object instance as the context.
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
        /// Reads a value from the property that this property metadata represents, using the given
        /// object instance as the context.
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

        /// <summary>
        /// Determines whether the specified object is equal to this one.
        /// </summary>
        public override bool Equals(System.Object obj) {
            // If parameter is null return false.
            if (obj == null) {
                return false;
            }

            // If parameter cannot be cast to PropertyMetadata return false.
            PropertyMetadata p = obj as PropertyMetadata;
            if ((System.Object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (StorageType == p.StorageType) && (Name == p.Name);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this one.
        /// </summary>
        public bool Equals(PropertyMetadata p) {
            // If parameter is null return false:
            if ((object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (StorageType == p.StorageType) && (Name == p.Name);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.</returns>
        public override int GetHashCode() {
            return StorageType.GetHashCode() ^ Name.GetHashCode();
        }
    }
}