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

using Xunit;

namespace Forge.Utilities.Tests {
    public class FastStringFormatTests {
        [Fact]
        public void OneArgument() {
            Assert.Equal("1", FastStringFormat.Format("{0}", "1"));
            Assert.Equal(" 1", FastStringFormat.Format(" {0}", "1"));
            Assert.Equal(" 1 ", FastStringFormat.Format(" {0} ", "1"));

            Assert.Equal("11", FastStringFormat.Format("{0}{0}", "1"));
            Assert.Equal("1 1", FastStringFormat.Format("{0} {0}", "1"));
        }

        [Fact]
        public void NArguments() {
            Assert.Equal("0", FastStringFormat.Format("{0}", "0"));
            Assert.Equal("01", FastStringFormat.Format("{0}{1}", "0", "1"));
            Assert.Equal("012", FastStringFormat.Format("{0}{1}{2}", "0", "1", "2"));
            Assert.Equal("0123", FastStringFormat.Format("{0}{1}{2}{3}", "0", "1", "2", "3"));
            Assert.Equal("01234", FastStringFormat.Format("{0}{1}{2}{3}{4}", "0", "1", "2", "3", "4"));
            Assert.Equal("012345", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}", "0", "1", "2", "3", "4", "5"));
            Assert.Equal("0123456", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}", "0", "1", "2", "3", "4", "5", "6"));
            Assert.Equal("01234567", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}", "0", "1", "2", "3", "4", "5", "6", "7"));
            Assert.Equal("012345678", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "0", "1", "2", "3", "4", "5", "6", "7", "8"));
            Assert.Equal("0123456789", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"));
        }
    }
}