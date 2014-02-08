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
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace CollectionsTests {
    public class WorldIndexCoordinateTransformTests {
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

        [Theory, PropertyData("WorldScales")]
        public void WorldIndexCoordinateTransformTest(int worldScale) {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            int range = 3;

            int prevIndex = 0;

            for (int world = -worldScale * range; world < worldScale * range; ++world) {

                int index = WorldIndexCoordinateTransform.MapWorldToIndex(worldScale, world);
                int reveresedWorld = WorldIndexCoordinateTransform.MapIndexToWorld(worldScale, index);

                if (index != prevIndex) {
                    Console.WriteLine();
                }
                Console.WriteLine("{0} -> {1} ::: {2}", world, index, reveresedWorld);

                prevIndex = index;

                int prev = 0;
                counts.TryGetValue(index, out prev);
                counts[index] = prev + 1;

                Assert.Equal(index, WorldIndexCoordinateTransform.MapWorldToIndex(worldScale, reveresedWorld));
            }

            Console.WriteLine("===================");
            Console.WriteLine("Counts");
            foreach (var item in counts) {
                Console.WriteLine("index {0} found {1} times", item.Key, item.Value);
                Assert.Equal(worldScale, item.Value);
            }
        }
    }
}