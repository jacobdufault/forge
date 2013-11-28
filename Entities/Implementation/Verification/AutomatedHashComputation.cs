using Neon.FileSaving;
using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Neon.Entities.Implementation.Verification {
    /// <summary>
    /// Helper type that provides hash codes that are calculated from the serialization metamodel.
    /// </summary>
    internal class AutomatedHashComputation {
        /// <summary>
        /// Calculates a hash code based on the serialization reflection metamodel for the given
        /// object instance.
        /// </summary>
        public static int GetHash(object instance) {
            Type instanceType = instance.GetType();

            // if it is a primitive type, just return .NET's hash code
            if (instanceType.IsPrimitive || instanceType == typeof(string)) {
                return instance.GetHashCode();
            }

            // otherwise lets create a hash code from the metadata
            TypeMetadata metadata = TypeCache.GetMetadata(instanceType);

            int hash = 17;

            if (metadata.IsArray || metadata.IsCollection) {
                IEnumerable enumerable = (IEnumerable)instance;
                foreach (var item in enumerable) {
                    hash = (hash * 31) + GetHash(item);
                }
            }

            else {
                foreach (var property in metadata.Properties) {
                    object field = property.Read(instance);
                    hash = (hash * 31) + GetHash(field);
                }
            }

            return hash;
        }
    }
}