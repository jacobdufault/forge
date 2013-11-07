using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Serialization;
using Neon.Utilities;

namespace Neon.Serialization.Tests {
    [TestClass]
    public class ParsingTests {
        [TestMethod]
        public void TestStrings() {
            string json = @"
""hello""
";
            var result = Parser.Parse(json);
            Assert.AreEqual("hello", result.AsString);
        }

        private void TestRealVariants(string baseNumber, long beforeDecimal, int afterDecimal) {
            var result = Parser.Parse(baseNumber);
            var expected = Real.CreateDecimal(beforeDecimal, afterDecimal);
            Assert.AreEqual(expected, result.AsReal);

            result = Parser.Parse("-" + baseNumber);
            expected = Real.CreateDecimal(-beforeDecimal, afterDecimal);
            Assert.AreEqual(expected, result.AsReal);

            result = Parser.Parse("+" + baseNumber);
            expected = Real.CreateDecimal(beforeDecimal, afterDecimal);
            Assert.AreEqual(expected, result.AsReal);
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
            Assert.AreEqual(true, result.AsBool);

            result = Parser.Parse("false");
            Assert.AreEqual(false, result.AsBool);
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
            Assert.AreEqual(Real.CreateDecimal(3), result.AsDictionary["hi"].AsReal);
            Assert.AreEqual(true, result.AsDictionary["no"].AsBool);
            Assert.AreEqual(Real.CreateDecimal(3), result.AsDictionary["something"].AsDictionary["some"].AsReal);
            Assert.AreEqual(Real.CreateDecimal(32), result.AsDictionary["something"].AsDictionary["ab"].AsReal);
        }

        [TestMethod]
        public void TestArrays() {
            var result = Parser.Parse(@"
[ 1 2 true 5 4 [true false]]
");
            Assert.AreEqual(Real.CreateDecimal(1), result.AsList[0].AsReal);
            Assert.AreEqual(Real.CreateDecimal(2), result.AsList[1].AsReal);
            Assert.AreEqual(true, result.AsList[2].AsBool);
            Assert.AreEqual(Real.CreateDecimal(5), result.AsList[3].AsReal);
            Assert.AreEqual(Real.CreateDecimal(4), result.AsList[4].AsReal);
            Assert.AreEqual(true, result.AsList[5].AsList[0].AsBool);
            Assert.AreEqual(false, result.AsList[5].AsList[1].AsBool);
        }
    }
}