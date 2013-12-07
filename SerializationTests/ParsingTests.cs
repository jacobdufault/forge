using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Serialization;
using Neon.Utilities;

namespace Neon.Serialization.Tests {
    [TestClass]
    public class ParsingTests {
        [TestMethod]
        public void TestStrings() {
            string data = @"
""hello""
";
            var result = Parser.Parse(data);
            Assert.AreEqual("hello", result.AsString);
        }

        private void TestRealVariants(string baseNumber, long beforeDecimal, int afterDecimal, int afterDigits) {
            var result = Parser.Parse(baseNumber);
            var expected = Real.CreateDecimal(beforeDecimal, afterDecimal, afterDigits);
            Assert.AreEqual(expected, result.AsReal);

            result = Parser.Parse("-" + baseNumber);
            expected = Real.CreateDecimal(-beforeDecimal, afterDecimal, afterDigits);
            Assert.AreEqual(expected, result.AsReal);

            result = Parser.Parse("+" + baseNumber);
            expected = Real.CreateDecimal(beforeDecimal, afterDecimal, afterDigits);
            Assert.AreEqual(expected, result.AsReal);
        }

        [TestMethod]
        public void TestNumbers() {
            TestRealVariants("12", 12, 0, 0);
            TestRealVariants("15.325", 15, 325, 3);
            TestRealVariants("15.", 15, 0, 0);
            TestRealVariants(".", 0, 0, 0);
            TestRealVariants(".33", 0, 33, 2);
            TestRealVariants(".0005", 0, 0005, 4);
        }

        [TestMethod]
        public void TestBools() {
            var result = Parser.Parse("true");
            Assert.AreEqual(true, result.AsBool);

            result = Parser.Parse("false");
            Assert.AreEqual(false, result.AsBool);
        }

        [TestMethod]
        public void TestObjectDefinitions() {
            {
                var result = Parser.Parse(@"
                {
                    a`0: -1
                    b   `1: -2
                    c`2   : -3
                    d    `3   : -4
                }");

                Assert.AreEqual(-1, result.AsDictionary["a"].AsReal.AsInt);
                Assert.AreEqual(-2, result.AsDictionary["b"].AsReal.AsInt);
                Assert.AreEqual(-3, result.AsDictionary["c"].AsReal.AsInt);
                Assert.AreEqual(-4, result.AsDictionary["d"].AsReal.AsInt);

                Assert.IsTrue(result.AsDictionary["a"].IsObjectDefinition);
                Assert.IsTrue(result.AsDictionary["b"].IsObjectDefinition);
                Assert.IsTrue(result.AsDictionary["c"].IsObjectDefinition);
                Assert.IsTrue(result.AsDictionary["d"].IsObjectDefinition);
            }
        }

        [TestMethod]
        public void TestObjectReferences() {
            {
                var result = Parser.Parse("`1");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(1, result.AsObjectReference);
            }

            {
                var result = Parser.Parse("`123 ");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference);
            }

            {
                var result = Parser.Parse("`123");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference);
            }

            {
                var result = Parser.Parse("`123ab");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference);
            }

            {
                var result = Parser.Parse("`005123  ");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(5123, result.AsObjectReference);
            }

            try {
                Parser.Parse("`");
                Assert.Fail("Should have failed to parse object reference");
            }
            catch (ParseException) { }
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
    objectref: `3
}
");
            Assert.AreEqual(Real.CreateDecimal(3), result.AsDictionary["hi"].AsReal);
            Assert.AreEqual(true, result.AsDictionary["no"].AsBool);
            Assert.AreEqual(Real.CreateDecimal(3), result.AsDictionary["something"].AsDictionary["some"].AsReal);
            Assert.AreEqual(Real.CreateDecimal(32), result.AsDictionary["something"].AsDictionary["ab"].AsReal);
            Assert.AreEqual(3, result.AsDictionary["objectref"].AsObjectReference);
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