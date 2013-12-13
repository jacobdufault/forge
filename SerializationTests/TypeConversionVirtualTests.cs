using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Tests {
    [SerializationSupportInheritance]
    internal class VirtualBase {
        public virtual int A {
            get;
            set;
        }
    }

    internal class InstanceVirtualBase : VirtualBase {
        public override int A {
            get;
            set;
        }
    }

    internal abstract class AbstractBase {
        public abstract int A {
            get;
            set;
        }
    }

    internal class InstanceAbstractBase : AbstractBase {
        public override int A {
            get;
            set;
        }
    }

    [TestClass]
    public class TypeConversionVirtualTests {
        [TestMethod]
        public void TestVirtualProperties() {
            VirtualBase instance = new InstanceVirtualBase();
            instance.A = 3;

            SerializedData exported = ObjectSerializer.Export(instance);
            VirtualBase imported = ObjectSerializer.Import<VirtualBase>(exported);

            Assert.AreEqual(instance.A, imported.A);
        }

        [TestMethod]
        public void TestAbstractProperties() {
            AbstractBase instance = new InstanceAbstractBase();
            instance.A = 3;

            SerializedData exported = ObjectSerializer.Export(instance);
            AbstractBase imported = ObjectSerializer.Import<AbstractBase>(exported);

            Assert.AreEqual(instance.A, imported.A);
        }

    }
}