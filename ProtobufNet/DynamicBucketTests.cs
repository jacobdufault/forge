using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Utilities;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtobufNetDynamicBucket {
    /// <summary>
    /// Contains a list of arbitrary objects which are serialized with their type.
    /// </summary>
    internal class SerializableContainer {
        /// <summary>
        /// Helper method to register the container with the given type metadata.
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
                Type deserializationType = Type.GetType(typeName);

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

    public interface Interface { }
    [ProtoContract]
    public class Derived1 : Interface {
        [ProtoMember(1)]
        public int A;
    }
    [ProtoContract]
    public class Derived2 : Interface {
        [ProtoMember(1)]
        public int B;
    }

    [ProtoContract]
    public class GeneralObjectContainer {
        public List<object> Items = new List<object>();

        [ProtoMember(1)]
        private SerializableContainer _container;

        [ProtoBeforeSerialization]
        private void PopulateContainer() {
            _container = new SerializableContainer() {
                Items = new List<object>(Items)
            };
        }

        [ProtoAfterDeserialization]
        private void RestoreContainer() {
            Items = new List<object>(_container.Items);
        }
    }

    [TestClass]
    public class DynamicBucketTests {
        [TestMethod]
        public void EmptySerializableContainerIsNotNull() {
            SerializableContainer.RegisterWithTypeModel(RuntimeTypeModel.Default);

            SerializableContainer container = new SerializableContainer();
            SerializableContainer cloned = Serializer.DeepClone(container);
            Assert.IsNotNull(cloned.Items);
        }

        [TestMethod]
        public void DynamicBucketTest() {
            SerializableContainer.RegisterWithTypeModel(RuntimeTypeModel.Default);

            SerializableContainer container = new SerializableContainer();
            container.Items.Add(new Derived1() {
                A = 1
            });
            container.Items.Add(new Derived2() {
                B = 2
            });

            SerializableContainer clone = Serializer.DeepClone<SerializableContainer>(container);
            Assert.AreEqual(2, clone.Items.Count);
            Assert.IsInstanceOfType(clone.Items[0], typeof(Derived1));
            Assert.IsInstanceOfType(clone.Items[1], typeof(Derived2));
            Assert.AreEqual(((Derived1)container.Items[0]).A, ((Derived1)clone.Items[0]).A);
            Assert.AreEqual(((Derived2)container.Items[1]).B, ((Derived2)clone.Items[1]).B);
        }

        [TestMethod]
        public void TestConvertToContainer() {
            SerializableContainer.RegisterWithTypeModel(RuntimeTypeModel.Default);

            GeneralObjectContainer o = new GeneralObjectContainer();
            o.Items.Add(new Derived1() {
                A = 1
            });
            o.Items.Add(new Derived2() {
                B = 2
            });

            GeneralObjectContainer cloned = Serializer.DeepClone(o);
            Assert.AreEqual(2, cloned.Items.Count);
            Assert.IsInstanceOfType(cloned.Items[0], typeof(Derived1));
            Assert.IsInstanceOfType(cloned.Items[1], typeof(Derived2));
            Assert.AreEqual(((Derived1)o.Items[0]).A, ((Derived1)cloned.Items[0]).A);
            Assert.AreEqual(((Derived2)o.Items[1]).B, ((Derived2)cloned.Items[1]).B);
        }

        [ProtoContract]
        public class MyTuple<T> {
            [ProtoMember(1)]
            public T Item1;
        }

        [TestMethod]
        public void TestConvertToContainedContainer() {
            SerializableContainer.RegisterWithTypeModel(RuntimeTypeModel.Default);

            GeneralObjectContainer o = new GeneralObjectContainer();
            o.Items.Add(new Derived1() {
                A = 1
            });
            o.Items.Add(new Derived2() {
                B = 2
            });

            MyTuple<GeneralObjectContainer> t = new MyTuple<GeneralObjectContainer>() {
                Item1 = o
            };

            GeneralObjectContainer cloned = Serializer.DeepClone(t).Item1;
            Assert.AreEqual(2, cloned.Items.Count);
            Assert.IsInstanceOfType(cloned.Items[0], typeof(Derived1));
            Assert.IsInstanceOfType(cloned.Items[1], typeof(Derived2));
            Assert.AreEqual(((Derived1)o.Items[0]).A, ((Derived1)cloned.Items[0]).A);
            Assert.AreEqual(((Derived2)o.Items[1]).B, ((Derived2)cloned.Items[1]).B);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SerializeList() {
            SerializableContainer.RegisterWithTypeModel(RuntimeTypeModel.Default);

            List<object> items = new List<object>();
            items.Add(new Derived1() {
                A = 1
            });
            items.Add(new Derived2() {
                B = 2
            });

            List<object> cloned = Serializer.DeepClone(items);
            Assert.AreEqual(2, cloned.Count);
            Assert.IsInstanceOfType(cloned[0], typeof(Derived1));
            Assert.IsInstanceOfType(cloned[1], typeof(Derived2));
            Assert.AreEqual(((Derived1)items[0]).A, ((Derived1)cloned[0]).A);
            Assert.AreEqual(((Derived2)items[1]).B, ((Derived2)cloned[1]).B);
        }
    }
}