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
    internal interface IFace { }
    internal interface IGenericIFace<T> { }
    internal interface IGenericIFace2<T0, T1> { }
    internal class ImplIFace : IFace { }
    internal class ImplIGenericIFace : IGenericIFace<int> { }
    internal class ImplIGenericIFace<T> : IGenericIFace<T> { }
    internal class ImplIGenericIFace2a : IGenericIFace2<int, int> { }
    internal class ImplIGenericIFace2b<T0> : IGenericIFace2<T0, int> { }
    internal class ImplIGenericIFace2c<T1> : IGenericIFace2<int, T1> { }
    internal class ImplIGenericIFace2d<T0, T1> : IGenericIFace2<T0, T1> { }

    [TestClass]
    public class TypeExtensionTests {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsImplementationOfRequiresGenericTypeDefinitions() {
            typeof(ImplIGenericIFace).IsImplementationOf(typeof(IGenericIFace<int>));
        }

        [TestMethod]
        public void IsImplementationOf() {
            Assert.IsTrue(typeof(ImplIFace).IsImplementationOf(typeof(IFace)));

            Assert.IsTrue(typeof(ImplIGenericIFace).IsImplementationOf(typeof(IGenericIFace<>)));
            Assert.IsFalse(typeof(ImplIGenericIFace).IsImplementationOf(typeof(IFace)));

            Assert.IsTrue(typeof(ImplIGenericIFace<int>).IsImplementationOf(typeof(IGenericIFace<>)));
            Assert.IsFalse(typeof(ImplIGenericIFace<int>).IsImplementationOf(typeof(IFace)));

            Assert.IsTrue(typeof(ImplIGenericIFace2a).IsImplementationOf(typeof(IGenericIFace2<,>)));
            Assert.IsFalse(typeof(ImplIGenericIFace2a).IsImplementationOf(typeof(IFace)));

            Assert.IsTrue(typeof(ImplIGenericIFace2b<int>).IsImplementationOf(typeof(IGenericIFace2<,>)));
            Assert.IsFalse(typeof(ImplIGenericIFace2b<int>).IsImplementationOf(typeof(IFace)));

            Assert.IsTrue(typeof(ImplIGenericIFace2c<int>).IsImplementationOf(typeof(IGenericIFace2<,>)));
            Assert.IsFalse(typeof(ImplIGenericIFace2c<int>).IsImplementationOf(typeof(IFace)));

            Assert.IsTrue(typeof(ImplIGenericIFace2d<int, int>).IsImplementationOf(typeof(IGenericIFace2<,>)));
            Assert.IsFalse(typeof(ImplIGenericIFace2d<int, int>).IsImplementationOf(typeof(IFace)));
        }
    }
}