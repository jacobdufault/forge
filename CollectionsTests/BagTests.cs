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
using Forge.Collections;
using System.Collections.Generic;
using Xunit;

namespace CollectionsTests {
    public class BagTests {
        private IEnumerable<Bag<int>> GetBags() {
            yield return new Bag<int>() { };

            yield return new Bag<int>() {
                1, 2, 3, 4, 5, 6
            };
        }

        [Fact]
        public void Insert() {
            const int length = 30;

            // simple insert of 0 to 29
            {
                Bag<int> b = new Bag<int>();
                for (int i = 0; i < length; ++i) {
                    ((IList<int>)b).Insert(i, i);
                }

                for (int i = 0; i < length; ++i) {
                    Assert.Equal(i, b[i]);
                }
            }

            // more complex insert of 0 to 59
            {
                Bag<int> b = new Bag<int>();

                // insert even numbers
                for (int i = 0; i < length + 1; ++i) {
                    b.Add(i * 2);
                }

                // insert odd numbers
                for (int i = 0; i < length; ++i) {
                    ((IList<int>)b).Insert((i * 2) + 1, (i * 2) + 1);
                }

                // ensure it is equal
                for (int i = 0; i < length * 2; ++i) {
                    Assert.Equal(i, b[i]);
                }
            }
        }

        [Fact]
        public void RemoveAt() {
            const int length = 60;

            // add 0 through length and then remove from the front
            {
                Bag<int> b = new Bag<int>();
                for (int i = 0; i < length; ++i) {
                    b.Add(i);
                }

                ((IList<int>)b).RemoveAt(0);
                Assert.Equal(length - 1, b.Length);

                for (int i = 1; i < length; ++i) {
                    Assert.Equal(i, b[i - 1]);
                }
            }
        }
    }
}