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
using Xunit.Extensions;

namespace Forge.Utilities.Tests {
    public class RealTests {
        [Fact]
        public void DecimalCreation() {
            Assert.Equal(.105f, Real.CreateDecimal(0, 105, 3).AsFloat, 3);
            Assert.Equal(10.105f, Real.CreateDecimal(10, 105, 3).AsFloat, 3);
            Assert.Equal(5.105f, Real.CreateDecimal(5, 105, 3).AsFloat, 3);
            Assert.Equal(20.1f, Real.CreateDecimal(20, 1, 1).AsFloat, 3);
            Assert.Equal(-150.333, Real.CreateDecimal(-150, 333, 3).AsFloat, 3);
            Assert.Equal(-150.0005, Real.CreateDecimal(-150, 0005, 4).AsFloat, 3);
        }

        [Theory]
        [InlineData(0, 105, 3)]
        [InlineData(10, 105, 3)]
        [InlineData(5, 105, 3)]
        [InlineData(20, 1, 1)]
        [InlineData(-150, 333, 3)]
        [InlineData(-150, 0005, 4)]
        public void SerailizeReal(long beforeDecimal, int afterDecimal, int afterDigits) {
            Real real = Real.CreateDecimal(beforeDecimal, afterDecimal, afterDigits);
            Assert.Equal(real, SerializationHelpers.DeepClone(real));
        }
    }
}