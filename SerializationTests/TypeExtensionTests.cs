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