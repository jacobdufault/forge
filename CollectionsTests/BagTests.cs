using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Collections;
using System.Collections.Generic;

namespace CollectionsTests {
    [TestClass]
    public class BagTests {
        private IEnumerable<Bag<int>> GetBags() {
            yield return new Bag<int>() { };

            yield return new Bag<int>() {
                1, 2, 3, 4, 5, 6
            };
        }

        [TestMethod]
        public void Insert() {
            const int length = 30;

            // simple insert of 0 to 29
            {
                Bag<int> b = new Bag<int>();
                for (int i = 0; i < length; ++i) {
                    ((IList<int>)b).Insert(i, i);
                }

                for (int i = 0; i < length; ++i) {
                    Assert.AreEqual(i, b[i]);
                }
            }

            // more complex insert of 0 to 59
            {
                Bag<int> b = new Bag<int>();

                // insert even numbers
                for (int i = 0; i < length; ++i) {
                    b.Add(i * 2);
                }

                // insert odd numbers
                for (int i = 0; i < length; ++i) {
                    ((IList<int>)b).Insert((i * 2) + 1, (i * 2) + 1);
                }

                // ensure it is equal
                for (int i = 0; i < length * 2; ++i) {
                    Assert.AreEqual(i, b[i]);
                }
            }
        }

        [TestMethod]
        public void RemoveAt() {
            const int length = 60;

            // add 0 through length and then remove from the front
            {
                Bag<int> b = new Bag<int>();
                for (int i = 0; i < length; ++i) {
                    b.Add(i);
                }

                ((IList<int>)b).RemoveAt(0);
                Assert.AreEqual(length - 1, b.Length);

                for (int i = 1; i < length; ++i) {
                    Assert.AreEqual(i, b[i-1]);
                }
            }
        }
    }
}
