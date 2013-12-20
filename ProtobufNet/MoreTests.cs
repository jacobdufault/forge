using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtobufNet {
    internal interface Interface { }
    [ProtoContract]
    internal class Class : Interface { }

    [ProtoContract(AsReferenceDefault = true)]
    internal class ReferencedObject {
        public int A;
    }

    [ProtoContract]
    internal class Container {
        [ProtoMember(1)]
        public ReferencedObject RefA;

        [ProtoMember(2)]
        public ReferencedObject RefB;
    }

    [ProtoContract]
    internal class TypeContainer {
        [ProtoMember(1)]
        public Type Type;
    }

    [TestClass]
    public class MoreTests {
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void InterfaceNotContractButDerivedIs() {
            GetImportedExportedValue<Interface>(new Class());
        }

        [TestMethod]
        public void SharedInstanceRestoresAsShared() {
            Container c = new Container();
            c.RefA = new ReferencedObject();
            c.RefB = c.RefA;

            Container imported = GetImportedExportedValue(c);
            imported.RefA.A++;
            Assert.AreEqual(1, imported.RefB.A);
        }

        [TestMethod]
        public void SerializeTypes() {
            TypeContainer tc = new TypeContainer() {
                Type = typeof(int)
            };

            TypeContainer imported = GetImportedExportedValue(tc);
            Assert.AreEqual(typeof(int), imported.Type);
        }

        [ProtoContract]
        private class SimpleType {
            [ProtoMember(1)]
            public int A;
        }

        [ProtoContract]
        private class FailSurrogate<T> {
            public static implicit operator T(FailSurrogate<T> t) {
                throw new InvalidOperationException("Attempt to serialize type " + typeof(T) + " which is not applicable in this context");
            }

            public static implicit operator FailSurrogate<T>(T t) {
                throw new InvalidOperationException("Attempt to serialize type " + typeof(T) + " which is not applicable in this context");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestEmptySerializer() {
            RuntimeTypeModel model = RuntimeTypeModel.Create();
            model.Add(typeof(SimpleType), false).SetSurrogate(typeof(FailSurrogate<SimpleType>));

            model.DeepClone(new SimpleType() {
                A = 1
            });
        }
    }
}