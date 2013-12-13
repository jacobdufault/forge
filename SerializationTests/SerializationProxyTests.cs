// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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