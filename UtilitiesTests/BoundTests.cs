using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Utility;

namespace UtilitiesTests {
    [TestClass]
    public class BoundTests {
        [TestMethod]
        public void FullyContained() {
            Bound b1 = new Bound(0, 0, 30);
            Bound b2 = new Bound(0, 0, 1);

            Assert.IsTrue(b1.Intersects(b2));
            Assert.IsTrue(b2.Intersects(b1));
        }

        [TestMethod]
        public void AwayFromOrigin() {
            Bound b1 = new Bound(500, 0, 10);
            Bound b2 = new Bound(495, 0, 1);

            Assert.IsTrue(b1.Intersects(b2));
            Assert.IsTrue(b2.Intersects(b1));
        }

        [TestMethod]
        public void Intersecting() {
            Bound b1 = new Bound(0, 0, 5);
            Bound b2 = new Bound(0, 4, 3);

            Assert.IsTrue(b1.Intersects(b2));
            Assert.IsTrue(b2.Intersects(b1));
        }

        [TestMethod]
        public void IntersectingEdge() {
            Bound b1 = new Bound(0, 0, 5);
            Bound b2 = new Bound(0, 5, 1);

            Assert.IsTrue(b1.Intersects(b2));
            Assert.IsTrue(b2.Intersects(b1));
        }

        [TestMethod]
        public void NotIntersecting() {
            Bound b1 = new Bound(0, 0, 1);
            Bound b2 = new Bound(10, 10, 1);

            Assert.IsFalse(b1.Intersects(b2));
            Assert.IsFalse(b2.Intersects(b1));
        }

        [TestMethod]
        public void ZeroRadius() {
            Bound b1 = new Bound(0, 0, 1);
            Bound b2 = new Bound(0, 0, 0);

            Assert.IsTrue(b1.Intersects(b2));
            Assert.IsTrue(b2.Intersects(b1));
        }
    }
}
