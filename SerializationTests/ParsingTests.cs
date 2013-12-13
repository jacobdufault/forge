// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
                    a:&d0 -1
                    b:  &d1 -2
                    c:  &d3   -3
                }");

                Assert.AreEqual(-1, result.AsDictionary["a"].AsReal.AsInt);
                Assert.AreEqual(-2, result.AsDictionary["b"].AsReal.AsInt);
                Assert.AreEqual(-3, result.AsDictionary["c"].AsReal.AsInt);

                Assert.IsTrue(result.AsDictionary["a"].IsObjectDefinition);
                Assert.IsTrue(result.AsDictionary["b"].IsObjectDefinition);
                Assert.IsTrue(result.AsDictionary["c"].IsObjectDefinition);

                try {
                    Parser.Parse("&0");
                    Assert.Fail("Parse accepted bad object reference");
                }
                catch (ParseException) {
                }
            }
        }

        [TestMethod]
        public void TestObjectReferences() {
            {
                var result = Parser.Parse("&r1<System.Int32>");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(1, result.AsObjectReference.Id);
                Assert.AreEqual(typeof(System.Int32), result.AsObjectReference.Type);
            }

            {
                var result = Parser.Parse("&r123<System.Int32> ");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference.Id);
                Assert.AreEqual(typeof(System.Int32), result.AsObjectReference.Type);
            }

            {
                var result = Parser.Parse("&r123<System.Int32>");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference.Id);
                Assert.AreEqual(typeof(System.Int32), result.AsObjectReference.Type);
            }

            {
                var result = Parser.Parse("&r123<System.Int32>ab");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference.Id);
                Assert.AreEqual(typeof(System.Int32), result.AsObjectReference.Type);
            }

            {
                var result = Parser.Parse("&r00123<System.Int32> ");
                Assert.IsTrue(result.IsObjectReference);
                Assert.AreEqual(123, result.AsObjectReference.Id);
                Assert.AreEqual(typeof(System.Int32), result.AsObjectReference.Type);
            }

            try {
                Parser.Parse("&");
                Assert.Fail("Should have failed to parse object reference");
            }
            catch (ParseException) { }

            try {
                Parser.Parse("&r");
                Assert.Fail("Should have failed to parse object reference");
            }
            catch (ParseException) { }

            try {
                Parser.Parse("&d");
                Assert.Fail("Should have failed to parse object reference");
            }
            catch (ParseException) { }

            try {
                Parser.Parse("&r132");
                Assert.Fail("Should have failed to parse object reference");
            }
            catch (ParseException) { }

            try {
                Parser.Parse("&r1<badtype>");
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
    objectref: &r3<System.Int32>
}
");
            Assert.AreEqual(Real.CreateDecimal(3), result.AsDictionary["hi"].AsReal);
            Assert.AreEqual(true, result.AsDictionary["no"].AsBool);
            Assert.AreEqual(Real.CreateDecimal(3), result.AsDictionary["something"].AsDictionary["some"].AsReal);
            Assert.AreEqual(Real.CreateDecimal(32), result.AsDictionary["something"].AsDictionary["ab"].AsReal);
            Assert.AreEqual(3, result.AsDictionary["objectref"].AsObjectReference.Id);
            Assert.AreEqual(typeof(System.Int32), result.AsDictionary["objectref"].AsObjectReference.Type);
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