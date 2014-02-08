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

namespace Forge.Collections {
    /// <summary>
    /// Contains some utility functions that translate between two coordinate systems, called world
    /// and index. The world coordinate system is larger than the index coordinate system, ie, every
    /// 100 units in the world coordinate system maps to 1 unit in the index coordinate system.
    /// Further, the world coordinate system contains all 4 quadrants (+-x, +-z), but the index
    /// coordinate system only contains +x and +z.
    /// </summary>
    internal static class WorldIndexCoordinateTransform {
        /// <summary>
        /// Converts the given game coordinates (which can be negative) into coordinates on a
        /// smaller map with only positive coordinates.
        /// </summary>
        /// <param name="worldScale">The scale difference between the world map and the index map; a
        /// value of 100 here means that 100 units of the world map correspond to 1 unit the index
        /// map.</param>
        /// <param name="xWorld">The x coordinate on the world.</param>
        /// <param name="zWorld">The z coordinate on the world.</param>
        /// <param name="xIndex">The x coordinate on the index map.</param>
        /// <param name="zIndex">The z coordinate on the index map.</param>
        public static void ConvertWorldToIndex(int worldScale, int xWorld, int zWorld, out int xIndex,
            out int zIndex) {

            xIndex = MapWorldToIndex(worldScale, xWorld);
            zIndex = MapWorldToIndex(worldScale, zWorld);
        }

        /// <summary>
        /// Converts the given world coordinate into an index coordinate.
        /// </summary>
        /// <param name="worldScale">The scale difference between the world map and the index map; a
        /// value of 100 here means that 100 units of the world map correspond to 1 unit the index
        /// map.</param>
        /// <param name="world">The world coordinate (can be negative).</param>
        /// <returns>The index coordinate that is associated with the given world
        /// coordinate.</returns>
        public static int MapWorldToIndex(int worldScale, int world) {
            if (world >= 0) {
                return (world / worldScale) * 2;
            }

            else {
                return (-(world + 1) / worldScale) * 2 + 1;
            }
        }

        /// <summary>
        /// Converts the given index coordinates into the most closely associated world coordinates.
        /// </summary>
        /// <param name="worldScale">The scale difference between the world map and the index map; a
        /// value of 100 here means that 100 units of the world map correspond to 1 unit the index
        /// map.</param>
        /// <param name="xIndex">The x index coordinate.</param>
        /// <param name="zIndex">The z index coordinate.</param>
        /// <param name="xWorld">The x world coordinate.</param>
        /// <param name="zWorld">The z world coordinate.</param>
        public static void ConvertIndexToWorld(int worldScale, int xIndex, int zIndex, out int xWorld,
            out int zWorld) {

            xWorld = MapIndexToWorld(worldScale, xIndex);
            zWorld = MapIndexToWorld(worldScale, zIndex);
        }

        /// <summary>
        /// Converts the given index coordinate into its most closely associated world coordinate.
        /// </summary>
        /// <param name="worldScale">The scale difference between the world map and the index map; a
        /// value of 100 here means that 100 units of the world map correspond to 1 unit the index
        /// map.</param>
        /// <param name="index">The index coordinate (must be >= 0).</param>
        /// <returns>A world coordinate that will map back to the given index coordinate.</returns>
        public static int MapIndexToWorld(int worldScale, int index) {
            // positive world coordinate
            if (index % 2 == 0) {
                return (index / 2) * worldScale;
            }

            // negative world coordinate
            else {
                return (-(index + 1) / 2) * worldScale;
            }
        }
    }
}