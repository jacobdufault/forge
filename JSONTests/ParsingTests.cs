using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Serialization;
using Neon.Utilities;

namespace JSONTests {
    [TestClass]
    public class ParsingTests {
        [TestMethod]
        public void TestStrings() {
            string json = @"
""hello""
";
            var result = Parser.Parse(json);
            Assert.AreEqual("hello", (string)result);
        }

        private void TestRealVariants(string baseNumber, long beforeDecimal, int afterDecimal) {
            var result = Parser.Parse(baseNumber);
            var expected = Real.CreateDecimal(beforeDecimal, afterDecimal);
            Assert.AreEqual(expected, (Real)result);

            result = Parser.Parse("-" + baseNumber);
            expected = Real.CreateDecimal(-beforeDecimal, afterDecimal);
            Assert.AreEqual(expected, (Real)result);

            result = Parser.Parse("+" + baseNumber);
            expected = Real.CreateDecimal(beforeDecimal, afterDecimal);
            Assert.AreEqual(expected, (Real)result);
        }

        [TestMethod]
        public void TestNumbers() {
            TestRealVariants("12", 12, 0);
            TestRealVariants("15.325", 15, 325);
            TestRealVariants("15.", 15, 0);
            TestRealVariants(".", 0, 0);
            TestRealVariants(".33", 0, 33);
        }

        [TestMethod]
        public void TestBools() {
            var result = Parser.Parse("true");
            Assert.AreEqual(true, (bool)result);

            result = Parser.Parse("false");
            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestObjects() {
            var result = Parser.Parse(@"
{
    hi: 3
    no: true
    something: {
        some: 3
        ab: 32
    }
}
");
            Assert.AreEqual(Real.CreateDecimal(3), (Real)result["hi"]);
            Assert.AreEqual(true, (bool)result["no"]);
            Assert.AreEqual(Real.CreateDecimal(3), (Real)result["something"]["some"]);
            Assert.AreEqual(Real.CreateDecimal(32), (Real)result["something"]["ab"]);
        }

        [TestMethod]
        public void TestArrays() {
            var result = Parser.Parse(@"
[ 1 2 true 5 4 [true false]]
");
            Assert.AreEqual(Real.CreateDecimal(1), (Real)(result[0]));
            Assert.AreEqual(Real.CreateDecimal(2), (Real)(result[1]));
            Assert.AreEqual(true, (bool)(result[2]));
            Assert.AreEqual(Real.CreateDecimal(5), (Real)(result[3]));
            Assert.AreEqual(Real.CreateDecimal(4), (Real)(result[4]));
            Assert.AreEqual(true, (bool)(result[5][0]));
            Assert.AreEqual(false, (bool)(result[5][1]));
        }
    }
}