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

using Forge.Collections;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace CollectionsTests {
    public class QuadTreeTests {
        [JsonObject(MemberSerialization.OptIn, IsReference = true)]
        private class TestMonitor<TItem> : IQuadTreeMonitor<TItem> {
            [JsonProperty]
            public HashSet<TItem> Contained = new HashSet<TItem>();

            public void OnEnter(TItem item) {
                Assert.True(Contained.Add(item), "Duplicate item added to monitor");
            }

            public void OnExit(TItem item) {
                Assert.True(Contained.Remove(item),
                    "Item removed from monitor that was not entered into the monitor");
            }

            public override bool Equals(object obj) {
                var monitor = obj as TestMonitor<TItem>;
                return monitor != null && Contained.Equals(monitor.Contained);
            }

            public override int GetHashCode() {
                return Contained.GetHashCode();
            }
        }

        [Theory, PropertyData("WorldScales")]
        public void MonitorTests(int worldScale) {
            var tree = new QuadTree<object>(worldScale);

            // test a monitor on an initially empty tree, add an object to the tree and ensure that
            // the monitor contains it, update the monitor to a position that doesn't contain the
            // item in the tree, ensure the monitor is empty, update the monitor so that it contains
            // the item again, ensure the monitor contains the item, update the monitor so that it
            // still contains the item & verify, then remove the monitor and ensure it is empty
            {
                var monitor = new TestMonitor<object>();

                // inserting a monitor into an empty tree should result in an empty monitor
                tree.AddMonitor(monitor, new Bound(0, 0, 25));
                Assert.Empty(monitor.Contained);

                // add an object to (0,0)
                var added = new object();
                tree.AddItem(added, new Vector2r());
                Assert.Contains(added, monitor.Contained);

                // update the position of the monitor so that it doesn't contain anything
                tree.UpdateMonitor(monitor, new Bound(0, 0, 25), new Bound(100, 100, 25));
                Assert.Empty(monitor.Contained);

                // update the position of the monitor so that it contains the item
                tree.UpdateMonitor(monitor, new Bound(100, 100, 25), new Bound(5, 5, 25));
                Assert.Contains(added, monitor.Contained);

                // update it back to its original position so that it contains everything
                tree.UpdateMonitor(monitor, new Bound(5, 5, 25), new Bound(0, 0, 25));
                Assert.Contains(added, monitor.Contained);

                // remove the monitor
                tree.RemoveMonitor(monitor, new Bound(0, 0, 25));
                Assert.Empty(monitor.Contained);
            }

            // test a monitor on a non-empty tree
            {
                var monitor = new TestMonitor<object>();

                // the monitor should have the object already in the tree added to it
                tree.AddMonitor(monitor, new Bound(0, 0, 25));
                Assert.NotEmpty(monitor.Contained);

                // add an object to (0,0)
                var added = new object();
                tree.AddItem(added, new Vector2r());
                Assert.Contains(added, monitor.Contained);

                // remove the monitor
                tree.RemoveMonitor(monitor, new Bound(0, 0, 25));
                Assert.Empty(monitor.Contained);
            }

            // test a monitor on a non-empty tree that doesn't contain the objects already in the
            // tree
            {
                var monitor = new TestMonitor<object>();

                // the monitor should remain empty when being added to the tree
                tree.AddMonitor(monitor, new Bound(100, 100, 25));
                Assert.Empty(monitor.Contained);

                // add an object to (0,0); it should not be added to the monitor
                var added = new object();
                tree.AddItem(added, new Vector2r());
                Assert.Empty(monitor.Contained);

                // remove the monitor
                tree.RemoveMonitor(monitor, new Bound(100, 100, 25));
                Assert.Empty(monitor.Contained);
            }
        }

        public static IEnumerable<object[]> WorldScales {
            get {
                yield return new object[] { 1 };
                yield return new object[] { 2 };
                yield return new object[] { 3 };
                yield return new object[] { 10 };
                yield return new object[] { 50 };
                yield return new object[] { 100 };
                yield return new object[] { 1000 };
            }
        }

        [Fact]
        public void SerializeQuadTree() {
            var tree = new QuadTree<string>();

            tree.AddItem("(0, 0)", new Vector2r());
            tree.AddItem("(1, -1)", new Vector2r(1, -1));
            tree.AddMonitor(new TestMonitor<string>(), new Bound(0, 10, 5));

            Console.WriteLine(SerializationHelpers.Serialize(tree));
            var cloned = SerializationHelpers.DeepClone(tree);

            Assert.Equal(tree.Items.Count(), cloned.Items.Count());
            foreach (var item in tree.Items) {
                Assert.Contains(item, cloned.Items);
            }

            Assert.Equal(tree.Monitors.Count(), cloned.Monitors.Count());
            foreach (var monitor in tree.Monitors) {
                Assert.True(tree.Monitors.Contains(monitor));
            }
        }

        [Theory, PropertyData("WorldScales")]
        public void CollectFromQuadTree(int worldScale) {
            var tree = new QuadTree<string>(worldScale);

            var added = new List<string>() {
                "-8,0",
                "-5,0",
                "0,0",
                "5,0",
                "8,0"
            };

            tree.AddItem(added[0], new Vector2r(-8, 0));
            tree.AddItem(added[1], new Vector2r(-5, 0));
            tree.AddItem(added[2], new Vector2r(0, 0));
            tree.AddItem(added[3], new Vector2r(5, 0));
            tree.AddItem(added[4], new Vector2r(8, 0));

            // collect everything (both positive and negative)
            {
                List<string> everything = tree.CollectItems<List<string>>(new Bound(0, 0, 200));
                Assert.Equal(5, everything.Count);
                Assert.Contains(added[0], everything);
                Assert.Contains(added[1], everything);
                Assert.Contains(added[2], everything);
                Assert.Contains(added[3], everything);
                Assert.Contains(added[4], everything);
            }

            // collection a positive region
            {
                var positiveOnly = tree.CollectItems<List<string>>(new Bound(5, 0, 4));
                Assert.Equal(2, positiveOnly.Count);
                Assert.Contains(added[3], positiveOnly);
                Assert.Contains(added[4], positiveOnly);
            }

            // collect a negative region
            {
                var negativeOnly = tree.CollectItems<List<string>>(new Bound(-5, 0, 4));
                Assert.Equal(2, negativeOnly.Count);
                Assert.Contains(added[0], negativeOnly);
                Assert.Contains(added[1], negativeOnly);
            }

            // collect an empty region
            {
                var nothing = tree.CollectItems<List<string>>(new Bound(100, 0, 5));
                Assert.Empty(nothing);
            }
        }
    }
}