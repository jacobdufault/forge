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