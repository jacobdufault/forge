using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Helper methods for protobuf-net.
    /// </summary>
    internal static class SerializationHelpers {
        /// <summary>
        /// Serializes the given object with its type, so that when it is reconstructed it will be
        /// rebuilt with the correct type.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        public static byte[] SerializeWithType(object obj) {
            using (var ms = new MemoryStream()) {
                Type type = obj.GetType();
                var id = ASCIIEncoding.ASCII.GetBytes(type.FullName + '|');
                ms.Write(id, 0, id.Length);
                Serializer.Serialize(ms, obj);
                var bytes = ms.ToArray();
                return bytes;
            }
        }

        /// <summary>
        /// Reads an object with type information from a previous serialized state (done via
        /// SerializeWithType) ; this method is useful when you don't know the type of object you
        /// are deserializing.
        /// </summary>
        /// <param name="data">The data to deserialize from.</param>
        /// <returns>An object instance of the same type that it was serialized with.</returns>
        public static object DeserializeWithType(byte[] data) {
            StringBuilder stringBuilder = new StringBuilder();
            using (MemoryStream stream = new MemoryStream(data)) {
                while (true) {
                    var currentChar = (char)stream.ReadByte();
                    if (currentChar == '|') {
                        break;
                    }

                    stringBuilder.Append(currentChar);
                }
                string typeName = stringBuilder.ToString();
                Type deserializationType = TypeCache.FindType(typeName);

                return Serializer.NonGeneric.Deserialize(deserializationType, stream);
            }
        }
    }
}