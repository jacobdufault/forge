using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Utilities.Tests {
    public static class SerializationHelpers {
        public static T DeepClone<T>(T instance) {
            string json = JsonConvert.SerializeObject(instance, typeof(T), Formatting.Indented, null);
            return (T)JsonConvert.DeserializeObject(json, typeof(T), settings: null);
        }
    }

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