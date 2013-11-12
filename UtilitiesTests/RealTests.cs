using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neon.Utilities.Tests {
    [TestClass]
    public class RealTests {
        [TestMethod]
        public void DecimalCreation() {
            Assert.AreEqual(.105f, Real.CreateDecimal(0, 105, 3).AsFloat, 0.01);
            Assert.AreEqual(10.105f, Real.CreateDecimal(10, 105, 3).AsFloat, 0.01);
            Assert.AreEqual(5.105f, Real.CreateDecimal(5, 105, 3).AsFloat, 0.01);
            Assert.AreEqual(20.1f, Real.CreateDecimal(20, 1, 1).AsFloat, 0.01);
            Assert.AreEqual(-150.333, Real.CreateDecimal(-150, 333, 3).AsFloat, 0.01);
            Assert.AreEqual(-150.0005, Real.CreateDecimal(-150, 0005, 4).AsFloat, 0.01);
        }
    }
}