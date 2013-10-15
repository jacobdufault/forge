using System;
using System.IO;

namespace Neon.Serialization {
    /// <summary>
    /// All objects which, after deserialization, require some sort of processing logic
    /// should extend SerializedObjectWithFixup
    /// </summary>
    public interface FixupAfterDeserialization {
        void Fixup();
    }

    /// <summary>
    /// Utility methods for serializing objects to strings so that they can be conveniently sent over the network.
    /// All objects which need to be serialized should be marked with the [Serialized] attribute. If an object
    /// has a field which should not be serialized, it should be marked with the [NonSerialized] attribute.
    /// </summary>
    /// <remarks>
    /// Serializer#Initialize must be called exactly once at application startup.
    /// </remarks>
    public static class Serializer {
        /// <summary>
        /// Support the given types for serialization.
        /// </summary>
        public static void Initialize(params Type[] types) {
            NetSerializer.Serializer.Initialize(types);
        }

        /// <summary>
        /// Serialize an object to a string.
        /// </summary>
        /// <param name="command">The object to serialize</param>
        /// <returns>A string containing the serialized data. Get the object back with Deserialize</returns>
        public static string Serialize(object instance) {
            using (MemoryStream memory = new MemoryStream()) {
                NetSerializer.Serializer.Serialize(memory, instance);
                return Convert.ToBase64String(memory.ToArray());
            }
        }

        /// <summary>
        /// Convert a serialized object back to an instance.
        /// </summary>
        /// <typeparam name="T">The instance type to serialize back to</typeparam>
        /// <param name="serialized">The serialized instance</param>
        public static T Deserialize<T>(string serialized) {
            byte[] bytes = Convert.FromBase64String(serialized);
            using (MemoryStream stream = new MemoryStream(bytes)) {
                T deserializedObject = NetSerializer.Serializer.Deserialize<T>(stream);
                if (deserializedObject is FixupAfterDeserialization) {
                    ((FixupAfterDeserialization)deserializedObject).Fixup();
                }

                return deserializedObject;
            }
        }
    }
}
