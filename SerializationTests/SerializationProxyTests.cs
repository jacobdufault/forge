using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Tests {
    internal class ProxiedType {
        public int A;
        public int B;

        public ProxiedType(int a, int b) {
            A = a;
            B = b;
        }
    }

    internal class Proxy : SerializationProxy<ProxiedType> {
        public override ProxiedType Import(SerializedData data) {
            return new ProxiedType(
                data.AsDictionary["myA"].AsReal.AsInt,
                data.AsDictionary["myB"].AsReal.AsInt);
        }

        public override SerializedData Export(ProxiedType instance) {
            Dictionary<string, SerializedData> data = new Dictionary<string, SerializedData>();

            data["myA"] = new SerializedData(instance.A);
            data["myB"] = new SerializedData(instance.B);

            return new SerializedData(data);
        }
    }

    [TestClass]
    public class SerializationProxyTests {
        [TestMethod]
        public void SerializationProxy() {
            ProxiedType original = new ProxiedType(2, 3);
            ProxiedType imported = SerializationHelpers.ImportExport(original);

            Assert.AreEqual(original.A, imported.A);
            Assert.AreEqual(original.B, imported.B);
        }
    }
}