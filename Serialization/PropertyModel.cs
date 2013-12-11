using System;
using System.Reflection;

namespace Neon.Serialization {
    /// <summary>
    /// A PropertyModel describes a discovered property or field in a TypeModel.
    /// </summary>
    public class PropertyModel {
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
        /// Writes a value to the property that this property model represents, using given object
        /// instance as the context.
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
        /// Reads a value from the property that this property model represents, using the given
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
        /// Initializes a new instance of the PropertyModel class from a property member.
        /// </summary>
        public PropertyModel(PropertyInfo property) {
            MemberInfo = property;
            Name = property.Name;
            StorageType = property.PropertyType;
        }

        /// <summary>
        /// Initializes a new instance of the PropertyModel class from a field member.
        /// </summary>
        public PropertyModel(FieldInfo field) {
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

            // If parameter cannot be cast to PropertyModel return false.
            PropertyModel p = obj as PropertyModel;
            if ((System.Object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (StorageType == p.StorageType) && (Name == p.Name);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this one.
        /// </summary>
        public bool Equals(PropertyModel p) {
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