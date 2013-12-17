using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Utilities.Tests {
    [TestClass]
    public class MaybeTests {
        [TestMethod]
        public void MaybeClone() {
            Maybe<int> maybe = Maybe.Just(3);

            Assert.IsTrue(Serializer.DeepClone(maybe).Exists);
            Assert.AreEqual(maybe.Value, Serializer.DeepClone(maybe).Value);

            Assert.IsTrue(Serializer.DeepClone(Maybe<int>.Empty).IsEmpty);
        }
    }
}