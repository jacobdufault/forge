using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtobufNetSurrogate {
    [Guid("44B84F9E-AC73-49FC-A94A-EBF769956274")]
    internal class CustomGuid {
    }

    //[ProtoContract]
    public class Interface {
        public static string SerializeWithType(object myObject) {
            using (var ms = new MemoryStream()) {
                Type type = myObject.GetType();
                var id = ASCIIEncoding.ASCII.GetBytes(type.FullName + '|');
                ms.Write(id, 0, id.Length);
                Serializer.Serialize(ms, myObject);
                var bytes = ms.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static object DeserializeWithType(string serializedData) {
            StringBuilder stringBuilder = new StringBuilder();
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(serializedData))) {
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

        public static Interface Parse(string content) {
            Assert.Fail("Parse called");
            return (Interface)DeserializeWithType(content);
        }

        public override string ToString() {
            return SerializeWithType(this);
        }
    }

    [ProtoContract]
    internal class Derived1 : Interface {
        [ProtoMember(1)]
        public int A;
    }

    [ProtoContract]
    internal class Derived2 : Interface {
        [ProtoMember(1)]
        public int B;
    }

    [ProtoContract]
    internal class InterfaceSurrogate {
        [ProtoMember(1)]
        private Type _type;

        [ProtoMember(2)]
        private byte[] _data;

        public static implicit operator InterfaceSurrogate(Interface value) {
            if (value == null) return null;

            throw new Exception("Yay");

            using (MemoryStream stream = new MemoryStream()) {
                Serializer.NonGeneric.Serialize(stream, value);
                stream.Position = 0;
                byte[] bytes = stream.ToArray();

                return new InterfaceSurrogate {
                    _type = value.GetType(),
                    _data = bytes
                };
            }
        }

        public static implicit operator Interface(InterfaceSurrogate value) {
            if (value == null) return null;

            using (MemoryStream stream = new MemoryStream(value._data)) {
                object instance = Serializer.NonGeneric.Deserialize(value._type, stream);
                return (Interface)instance;
            }
        }

    }

    [ProtoContract]
    internal class CallbackTest {
        [ProtoAfterDeserialization]
        public virtual void Called() {
            throw new Exception("Yay, this got called");
        }
    }

    [ProtoContract]
    internal class Derived : CallbackTest {
    }

    [TestClass]
    public class SurrogateTests {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void CallbackInheritance() {
            RuntimeTypeModel model = RuntimeTypeModel.Create();

            model.AutoAddMissingTypes = false;
            model.Add(typeof(CallbackTest), true);
            model.Add(typeof(Derived), true);

            model[typeof(CallbackTest)].AddSubType(1, typeof(Derived));

            model.DeepClone(new Derived());
        }

        [TestMethod]
        public void HasGuid() {
            Assert.IsTrue(Attribute.IsDefined(typeof(CustomGuid), typeof(GuidAttribute)));
        }

        /// <summary>
        /// Exports the value using a serialization converter, then reimports it using a new
        /// serialization converter. Returns the reimported value.
        /// </summary>
        private T GetImportedExportedValue<T>(T t0) {
            using (MemoryStream stream = new MemoryStream()) {
                Serializer.Serialize(stream, t0);
                stream.Position = 0;
                return Serializer.Deserialize<T>(stream);
            }
        }

        [TestMethod]
        public void SurrogateTest() {
            //RuntimeTypeModel.Default.Add(typeof(Interface), false).SetSurrogate(typeof(InterfaceSurrogate));
            //RuntimeTypeModel.Default.AllowParseableTypes = true;
            RuntimeTypeModel.Default.Add(typeof(Interface), false).SetSurrogate(typeof(InterfaceSurrogate));

            //RuntimeTypeModel metadata = RuntimeTypeModel.Create();
            //metadata.AllowParseableTypes = true;
            //metadata.AutoAddMissingTypes = true;
            //metadata.Add(typeof(InterfaceSurrogate), true);
            //metadata.Add(typeof(Interface), false).SetSurrogate(typeof(InterfaceSurrogate));

            Serializer.DeepClone<Interface>(new Derived1());
            return;

            Derived1 original1 = new Derived1() {
                A = 1
            };
            Derived2 original2 = new Derived2() {
                B = 2
            };

            Interface imported1 = GetImportedExportedValue<Interface>(original1);
            Interface imported2 = GetImportedExportedValue<Interface>(original2);

            Assert.IsInstanceOfType(imported1, typeof(Derived1));
            Assert.IsInstanceOfType(imported2, typeof(Derived2));

            Assert.AreEqual(original1.A, ((Derived1)imported1).A);
            Assert.AreEqual(original2.B, ((Derived2)imported2).B);
        }
    }
}