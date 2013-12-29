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

using System;
using Xunit;

namespace Forge.Utilities.Tests {
    public class BoundTests {
        [Fact]
        public void FullyContained() {
            Bound b1 = new Bound(0, 0, 30);
            Bound b2 = new Bound(0, 0, 1);

            Assert.True(b1.Intersects(b2));
            Assert.True(b2.Intersects(b1));
        }

        [Fact]
        public void AwayFromOrigin() {
            Bound b1 = new Bound(500, 0, 10);
            Bound b2 = new Bound(495, 0, 1);

            Assert.True(b1.Intersects(b2));
            Assert.True(b2.Intersects(b1));
        }

        [Fact]
        public void Intersecting() {
            Bound b1 = new Bound(0, 0, 5);
            Bound b2 = new Bound(0, 4, 3);

            Assert.True(b1.Intersects(b2));
            Assert.True(b2.Intersects(b1));
        }

        [Fact]
        public void IntersectingEdge() {
            Bound b1 = new Bound(0, 0, 5);
            Bound b2 = new Bound(0, 5, 1);

            Assert.True(b1.Intersects(b2));
            Assert.True(b2.Intersects(b1));
        }

        [Fact]
        public void NotIntersecting() {
            Bound b1 = new Bound(0, 0, 1);
            Bound b2 = new Bound(10, 10, 1);

            Assert.False(b1.Intersects(b2));
            Assert.False(b2.Intersects(b1));
        }

        [Fact]
        public void ZeroRadius() {
            Assert.Throws<ArgumentException>(() => new Bound(0, 0, 0));
        }
    }
}