using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtobufNetS2 {
    internal class Interface { }
    internal class Derived1 : Interface { }
    internal class Derived2 : Interface { }

    [ProtoContract]
    internal class InterfaceDataStorage {
        public static implicit operator Derived1(InterfaceDataStorage surrogate) {
            return new Derived1();
        }

        public static implicit operator InterfaceDataStorage(Derived1 derived) {
            return new InterfaceDataStorage();
        }

        public static implicit operator Derived2(InterfaceDataStorage surrogate) {
            return new Derived2();
        }

        public static implicit operator InterfaceDataStorage(Derived2 derived) {
            return new InterfaceDataStorage();
        }
    }

    [TestClass]
    public class SurrogateTests2 {
        [TestMethod]
        public void InterfaceSurrogate() {
            RuntimeTypeModel model = RuntimeTypeModel.Create();
            model.Add(typeof(Interface), false).SetSurrogate(typeof(InterfaceDataStorage));
            model.Add(typeof(Derived1), false).SetSurrogate(typeof(InterfaceDataStorage));
            model.Add(typeof(Derived2), false).SetSurrogate(typeof(InterfaceDataStorage));

            Assert.IsInstanceOfType(model.DeepClone(new Derived1()), typeof(Derived2));
            Assert.IsInstanceOfType(model.DeepClone(new Derived2()), typeof(Derived2));
        }
    }
}