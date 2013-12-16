using Neon.Entities.Implementation.Shared;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Content {
    /// <summary>
    /// Contains a list of arbitrary objects which are serialized with their type.
    /// </summary>
    internal class SerializableContainer {
        static SerializableContainer() {
            RegisterWithTypeModel(RuntimeTypeModel.Default);
        }

        /// <summary>
        /// Helper method to register the container with the given type model.
        /// </summary>
        public static void RegisterWithTypeModel(RuntimeTypeModel model) {
            // if we already registered ourselves, don't register again
            if (model.IsDefined(typeof(SerializableContainer))) {
                return;
            }

            model.Add(typeof(SerializableContainerSurrogate), true);
            model.Add(typeof(SerializableContainer), false).
                SetSurrogate(typeof(SerializableContainerSurrogate));
        }

        public SerializableContainer() {
            Items = new List<object>();
        }

        public SerializableContainer(IEnumerable enumerator) {
            Items = new List<object>(enumerator.Cast<object>());
        }

        public List<TResult> ToList<TResult>() {
            return Cast<TResult>().ToList();
        }

        public IEnumerable<TResult> Cast<TResult>() {
            return Items.Cast<TResult>();
        }

        /// <summary>
        /// The items to serialize.
        /// </summary>
        public List<object> Items = new List<object>();
    }

    [ProtoContract]
    internal class SerializableContainerSurrogate {
        /// <summary>
        /// Serializes the given object with its type, so that when it is reconstructed it will be
        /// rebuilt with the correct type.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        private static byte[] SerializeWithType(object obj) {
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
        private static object DeserializeWithType(byte[] data) {
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

        /// <summary>
        /// The list of items that are in the bucket we serialized.
        /// </summary>
        [ProtoMember(1)]
        private List<byte[]> _items = new List<byte[]>();

        public static implicit operator SerializableContainer(SerializableContainerSurrogate surrogate) {
            if (surrogate == null) return null;

            SerializableContainer container = new SerializableContainer();
            foreach (byte[] item in surrogate._items) {
                container.Items.Add(DeserializeWithType(item));
            }
            return container;
        }

        public static implicit operator SerializableContainerSurrogate(SerializableContainer bucket) {
            if (bucket == null) return null;

            SerializableContainerSurrogate surrogate = new SerializableContainerSurrogate();
            if (bucket.Items != null) {
                foreach (object item in bucket.Items) {
                    surrogate._items.Add(SerializeWithType(item));
                }
            }
            return surrogate;
        }
    }
}