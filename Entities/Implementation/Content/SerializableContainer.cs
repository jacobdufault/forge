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
        /// The list of items that are in the bucket we serialized.
        /// </summary>
        [ProtoMember(1)]
        private List<byte[]> _items = new List<byte[]>();

        public static implicit operator SerializableContainer(SerializableContainerSurrogate surrogate) {
            if (surrogate == null) return null;

            SerializableContainer container = new SerializableContainer();
            foreach (byte[] item in surrogate._items) {
                container.Items.Add(SerializationHelpers.DeserializeWithType(item));
            }
            return container;
        }

        public static implicit operator SerializableContainerSurrogate(SerializableContainer bucket) {
            if (bucket == null) return null;

            SerializableContainerSurrogate surrogate = new SerializableContainerSurrogate();
            if (bucket.Items != null) {
                foreach (object item in bucket.Items) {
                    surrogate._items.Add(SerializationHelpers.SerializeWithType(item));
                }
            }
            return surrogate;
        }
    }
}