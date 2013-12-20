using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            Assert.IsTrue(SerializationHelpers.DeepClone(maybe).Exists);
            Assert.AreEqual(maybe.Value, SerializationHelpers.DeepClone(maybe).Value);

            Assert.IsTrue(SerializationHelpers.DeepClone(Maybe<int>.Empty).IsEmpty);
        }
    }
}