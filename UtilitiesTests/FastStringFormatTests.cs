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

namespace Forge.Utilities.Tests {
    [TestClass]
    public class FastStringFormatTests {
        [TestMethod]
        public void OneArgument() {
            Assert.AreEqual("1", FastStringFormat.Format("{0}", "1"));
            Assert.AreEqual(" 1", FastStringFormat.Format(" {0}", "1"));
            Assert.AreEqual(" 1 ", FastStringFormat.Format(" {0} ", "1"));

            Assert.AreEqual("11", FastStringFormat.Format("{0}{0}", "1"));
            Assert.AreEqual("1 1", FastStringFormat.Format("{0} {0}", "1"));
        }

        [TestMethod]
        public void NArguments() {
            Assert.AreEqual("0", FastStringFormat.Format("{0}", "0"));
            Assert.AreEqual("01", FastStringFormat.Format("{0}{1}", "0", "1"));
            Assert.AreEqual("012", FastStringFormat.Format("{0}{1}{2}", "0", "1", "2"));
            Assert.AreEqual("0123", FastStringFormat.Format("{0}{1}{2}{3}", "0", "1", "2", "3"));
            Assert.AreEqual("01234", FastStringFormat.Format("{0}{1}{2}{3}{4}", "0", "1", "2", "3", "4"));
            Assert.AreEqual("012345", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}", "0", "1", "2", "3", "4", "5"));
            Assert.AreEqual("0123456", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}", "0", "1", "2", "3", "4", "5", "6"));
            Assert.AreEqual("01234567", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}", "0", "1", "2", "3", "4", "5", "6", "7"));
            Assert.AreEqual("012345678", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "0", "1", "2", "3", "4", "5", "6", "7", "8"));
            Assert.AreEqual("0123456789", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"));
        }
    }
}