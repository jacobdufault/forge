using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Neon.Entities.Tests {
    [TestClass]
    public class DataAccessorTests {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DataAccessorRejectsSupertypeType() {
            DataAccessor accessor = new DataAccessor(typeof(Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DataAccessorRejectsDataType() {
            DataAccessor accessor = new DataAccessor(typeof(IData));
        }

    }
}