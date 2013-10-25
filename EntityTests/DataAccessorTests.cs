using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;

namespace EntityTests {
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
            DataAccessor accessor = new DataAccessor(typeof(Data));
        }

    }
}
