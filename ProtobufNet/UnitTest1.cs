using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System.Xml;
using System.Text;
using System.IO;
using ProtoBuf.Meta;
using System.Reflection;
using Neon.Collections;
using System.Collections;
using System.Collections.Generic;

namespace ProtobufNet {
    [ProtoContract(AsReferenceDefault = true)]
    internal class Recursive {
        [ProtoMember(1)]
        public Recursive Ref;

        [ProtoMember(2)]
        public int A;
    }

    public static class RuntimeTypeModelExtensions {
        public static T Deserialize<T>(this RuntimeTypeModel model, Stream source) {
            return (T)model.Deserialize(source, null, typeof(T));
        }

        //public static T Deserialize<T>(this RuntimeTypeModel metadata, Stream source, T value) {
        //    return (T)metadata.Deserialize(source, value, typeof(T));
        //}
    }

    internal interface IInterface { }
    internal class Derived1 : IInterface {
        public int A;
    }
    internal class Derived2 : IInterface {
        public int B;
    }
    [ProtoContract]
    internal class Derived3 : IInterface {
    }
    [ProtoContract]
    internal class Derived4 : IInterface {
    }

    [TestClass]
    public class UnitTest1 {
        public interface IMessage { }
        public interface IEvent : IMessage { }
        public class DogBarkedEvent : IEvent {
            public string NameOfDog { get; set; }
            public int Times { get; set; }
        }

        [TestMethod]
        public void RoundTripAnUnknownMessage() {
            IMessage msg = new DogBarkedEvent {
                NameOfDog = "Woofy",
                Times = 5
            }, copy;
            var model = TypeModel.Create();
            model.Add(typeof(DogBarkedEvent), false).Add("NameOfDog", "Times");
            model.Add(typeof(IMessage), false).AddSubType(1, typeof(DogBarkedEvent));

            using (var ms = new MemoryStream()) {
                model.Serialize(ms, msg);
                ms.Position = 0;
                copy = (IMessage)model.Deserialize(ms, null, typeof(IMessage));
            }
            // check the data is all there
            Assert.IsInstanceOfType(copy, typeof(DogBarkedEvent));
            var typed = (DogBarkedEvent)copy;
            var orig = (DogBarkedEvent)msg;
            Assert.AreEqual(orig.Times, typed.Times);
            Assert.AreEqual(orig.NameOfDog, typed.NameOfDog);
        }

        public byte[] SerializeWithType(object myObject) {
            using (var ms = new MemoryStream()) {
                Type type = myObject.GetType();
                var id = ASCIIEncoding.ASCII.GetBytes(type.FullName + '|');
                ms.Write(id, 0, id.Length);
                Serializer.Serialize(ms, myObject);
                var bytes = ms.ToArray();
                return bytes;
            }
        }

        public object DeserializeWithType(byte[] serializedData) {
            StringBuilder stringBuilder = new StringBuilder();
            using (MemoryStream stream = new MemoryStream(serializedData)) {
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

        /*
        [TestMethod]
        public void TestDynamicInheritance() {
            Assert.IsInstanceOfType(DeserializeWithType(SerializeWithType(new Derived3())), typeof(Derived3));
            Assert.IsInstanceOfType(DeserializeWithType(SerializeWithType(new Derived4())), typeof(Derived4));
            Assert.IsInstanceOfType(DeserializeWithType(SerializeWithType(3)), typeof(int));

            MetaType a = RuntimeTypeModel.Default[typeof(int)];
        }
        */

        [TestMethod]
        public void TestInheritance() {
            RuntimeTypeModel model = RuntimeTypeModel.Create();
            model.AutoAddMissingTypes = false;

            model.Add(typeof(Derived1), true);
            model.Add(typeof(Derived2), true);

            model.Add(typeof(IInterface), false)
                .AddSubType(1, typeof(Derived1))
                .AddSubType(2, typeof(Derived2));

            MemoryStream stream = new MemoryStream();

            {
                Derived1 d1 = new Derived1();

                model.Serialize(stream, d1);
                stream.Seek(0, SeekOrigin.Begin);
            }

            {
                IInterface d1 = model.Deserialize<Derived1>(stream);
                Assert.IsInstanceOfType(d1, typeof(Derived1));
            }
        }

        [TestMethod]
        public void TestRecursiveStructures() {
            RuntimeTypeModel model = RuntimeTypeModel.Create();
            model.AutoAddMissingTypes = false;
            model.Add(typeof(Recursive), true);

            MemoryStream stream = new MemoryStream();

            {
                Recursive r1 = new Recursive();
                Recursive r2 = new Recursive();
                r1.Ref = r2;
                r1.A = 1;
                r2.Ref = r1;
                r2.A = 2;

                model.Serialize(stream, r1);
                stream.Seek(0, SeekOrigin.Begin);
            }

            {
                Recursive r = model.Deserialize<Recursive>(stream);
                Assert.AreEqual(1, r.A);
                Assert.AreEqual(2, r.Ref.A);
            }

        }

        [TestMethod]
        public void BagTest() {
            RuntimeTypeModel model = RuntimeTypeModel.Create();
            model.AutoAddMissingTypes = true;

            Bag<int> bag = new Bag<int>();
            bag.Add(1);
            bag.Add(2);
            bag.Add(3);

            Bag<int> updated = GetImportedExportedValue(bag, model);

            Assert.AreEqual(bag.Length, updated.Length);
            Assert.IsTrue(updated.Contains(1));
            Assert.IsTrue(updated.Contains(2));
            Assert.IsTrue(updated.Contains(3));

            Assert.IsFalse(updated.Contains(4));
            Assert.IsFalse(updated.Contains(5));
            Assert.IsFalse(updated.Contains(6));
            Assert.IsFalse(updated.Contains(0));
            Assert.IsFalse(updated.Contains(-1));
        }

        [TestMethod]
        public void TestSortedDictionary() {
            SortedDictionary<int, int> dict = new SortedDictionary<int, int>();
            dict[1] = 1;
            dict[2] = 2;
            dict[3] = 3;
            dict[4] = 4;

            var imported = GetImportedExportedValue(dict, RuntimeTypeModel.Default);
            CollectionAssert.AreEqual(dict, imported);
        }

        [ProtoContract]
        private class ListContainer {
            [ProtoMember(1)]
            public List<int> List = new List<int>();

            /*
            [ProtoAfterDeserialization]
            public void Verify() {
                if (List == null) List = new List<int>();
            }
            */
        }

        [TestMethod]
        public void TestInstantiatedList() {
            ListContainer container = new ListContainer();
            container.List = new List<int>();

            ListContainer updated = GetImportedExportedValue(container, RuntimeTypeModel.Default);
            Assert.IsNotNull(updated.List);
        }

        [ProtoContract]
        private struct MyStruct {
            [ProtoMember(1)]
            public int A;
            [ProtoMember(2)]
            public int B;
            [ProtoMember(3)]
            public List<int> List;

            [ProtoAfterDeserialization]
            private void Verify() {
                if (List == null) {
                    List = new List<int>();
                }
            }
        }

        [TestMethod]
        public void TestStructs() {
            MyStruct original;
            original.A = 1;
            original.B = 2;
            original.List = new List<int>() { 3 };

            MyStruct updated = GetImportedExportedValue(original, RuntimeTypeModel.Default);

            Assert.AreEqual(original.A, updated.A);
            Assert.AreEqual(original.B, updated.B);
            CollectionAssert.AreEqual(original.List, updated.List);
        }

        /// <summary>
        /// Exports the value using a serialization converter, then reimports it using a new
        /// serialization converter. Returns the reimported value.
        /// </summary>
        private T GetImportedExportedValue<T>(T t0, RuntimeTypeModel model) {
            using (MemoryStream stream = new MemoryStream()) {
                model.Serialize(stream, t0);
                stream.Position = 0;
                return (T)model.Deserialize(stream, null, typeof(T));
            }
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// Assert.AreEqual.
        /// </summary>
        private void RunImportExportTest<T>(T t0, RuntimeTypeModel model) {
            T imported = GetImportedExportedValue(t0, model);
            Assert.AreEqual(t0, imported);
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// CollectionAssert.AreEqual.
        /// </summary>
        private void RunCollectionImportExportTest<TCollection>(TCollection collection,
            RuntimeTypeModel model)
            where TCollection : ICollection {
            TCollection imported = GetImportedExportedValue(collection, model);
            CollectionAssert.AreEqual(collection, imported);
        }
    }

    /// <summary>
    /// Caches type name to type lookups. Type lookups occur in all loaded assemblies.
    /// </summary>
    public static class TypeCache {
        /// <summary>
        /// Cache from fully qualified type name to type instances.
        /// </summary>
        private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Cache from types to its associated metadata.
        /// </summary>
        private static Dictionary<Type, TypeModel> _cachedTypeModels = new Dictionary<Type, TypeModel>();

        /// <summary>
        /// Find a type with the given name. An exception is thrown if no type with the given name
        /// can be found. This method searches all currently loaded assemblies for the given type.
        /// </summary>
        /// <param name="name">The fully qualified name of the type.</param>
        public static Type FindType(string name) {
            // see if the type is in the cache; if it is, then just return it
            {
                Type type;
                if (_cachedTypes.TryGetValue(name, out type)) {
                    return type;
                }
            }

            // cache lookup failed; search all loaded assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType(name);
                if (type != null) {
                    _cachedTypes[name] = type;
                    return type;
                }
            }

            // couldn't find the type; throw an exception
            throw new Exception(string.Format("Unable to find the type for {0}", name));
        }
    }
}