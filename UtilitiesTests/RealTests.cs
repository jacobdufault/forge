using Neon.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilitiesTests {
    [TestClass]
    public class RealTests {
        [TestMethod]
        public void DecimalCreation() {
            Assert.AreEqual(.105f, Real.CreateDecimal(0, 105).AsFloat, 0.01);
            Assert.AreEqual(10.105f, Real.CreateDecimal(10, 105).AsFloat, 0.01);
            Assert.AreEqual(5.105f, Real.CreateDecimal(5, 105).AsFloat, 0.01);
            Assert.AreEqual(20.1f, Real.CreateDecimal(20, 1).AsFloat, 0.01);
            Assert.AreEqual(-150.333, Real.CreateDecimal(-150, 333).AsFloat, 0.01);
        }
    }
}
