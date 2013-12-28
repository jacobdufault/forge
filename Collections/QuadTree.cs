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

using log4net;
using Forge.Collections;
using Forge.Utilities;
using System;

namespace Forge.Collections {
    /*
    struct ThreadGroup {
    }

    class MultithreadingAttribute : Attribute {
        public MultithreadingAttribute() { }
        public MultithreadingAttribute(ThreadGroup group) { }
    }

    class Test {
        static ThreadGroup Group = new ThreadGroup();

        [Multithreading(Group)]
        void MyMethod() {

        }
    }
    */

    /// <summary>
    /// Interface for objects which are monitoring a specific region.
    /// </summary>
    public interface IQuadTreeMonitor<T> {
        /// <summary>
        /// Called when the given item has entered the region.
        /// </summary>
        void OnEnter(T item);

        /// <summary>
        /// Called when an item has left the region.
        /// </summary>
        void OnExit(T item);
    }

    internal struct Rect {
        public int X0, Z0, X1, Z1;

        public Rect(int x0, int z0, int x1, int z1) {
            X0 = x0;
            Z0 = z0;
            X1 = x1;
            Z1 = z1;
        }

        public Rect(Bound bound) {
            X0 = (bound.X - bound.Radius).AsInt;
            Z0 = (bound.Z - bound.Radius).AsInt;
            X1 = (bound.X - bound.Radius + 1).AsInt;
            Z1 = (bound.Z - bound.Radius + 1).AsInt;
        }

        public int Width {
            get {
                return X1 - X0;
            }
        }

        public int Height {
            get {
                return Z1 - Z0;
            }
        }

        private bool InRange(int v, int min, int max) {
            return v >= min && v < max;
        }

        /// <summary>
        /// Returns true if this rect intersects with the parameter rect.
        /// </summary>
        public bool Intersects(Rect rect) {
            bool xIntersects =
                (InRange(rect.X0, X0, X1) && rect.X0 < X0) ||
                (InRange(rect.X1, X0, X1) && rect.X1 > X1);
            bool zIntersects =
                (InRange(rect.Z0, Z0, Z1) && rect.Z0 < Z0) ||
                (InRange(rect.Z1, Z0, Z1) && rect.Z1 > Z1);

            return xIntersects && zIntersects;
        }

        /// <summary>
        /// Returns true if the given point is contained with this rect.
        /// </summary>
        public bool Contains(int x, int z) {
            return InRange(x, X0, X1) && InRange(z, Z0, Z1);
        }

        /// <summary>
        /// Returns true if this rect fully contains the parameter rect.
        /// </summary>
        public bool Contains(Rect rect) {
            bool xContains =
                (InRange(rect.X0, X0, X1) && InRange(rect.X1, X0, X1));
            bool zContains =
                (InRange(rect.Z0, Z0, Z1) && InRange(rect.Z1, Z0, Z1));

            return xContains && zContains;
        }

        /// <summary>
        /// Returns true if this rect is either intersecting with or fully contains the parameter
        /// rect.
        /// </summary>
        public bool Colliding(Rect rect) {
            bool xCollides =
                (InRange(rect.X0, X0, X1)) ||
                (InRange(rect.X1, X0, X1));
            bool zCollides =
                (InRange(rect.Z0, Z0, Z1)) ||
                (InRange(rect.Z1, Z0, Z1));

            return xCollides && zCollides;
        }
    }

    // TODO: the linear searches in this class can be removed by using metadata
    internal class Node<T> {
        private class StoredMonitor {
            public IQuadTreeMonitor<T> Monitor;
            public Bound Region;

            public StoredMonitor(IQuadTreeMonitor<T> monitor, Bound region) {
                Monitor = monitor;
                Region = region;
            }
        }

        private class StoredItem {
            public T Item;
            public Vector2r Position;

            public StoredItem(T item, Vector2r position) {
                Item = item;
                Position = position;
            }
        }

        /// <summary>
        /// The position of the node on the map
        /// </summary>
        //private Rect Position;

        /// <summary>
        /// The subtrees in this node
        /// </summary>
        //private Node<T>[] Children;

        /// <summary>
        /// The items that the node contains
        /// </summary>
        private Bag<StoredItem> _items;

        /// <summary>
        /// The monitors that watch for adds/removes in this node
        /// </summary>
        private Bag<StoredMonitor> _monitors;

        public Node() {
            _items = new Bag<StoredItem>();
            _monitors = new Bag<StoredMonitor>();
        }

        public void Add(T item, Vector2r position) {
            _items.Add(new StoredItem(item, position));

            // call OnEnter for all monitors that are interested in the item
            for (int i = 0; i < _monitors.Length; ++i) {
                StoredMonitor monitor = _monitors[i];
                if (monitor.Region.Contains(position)) {
                    monitor.Monitor.OnEnter(item);
                }
            }
        }

        public void Update(T item, Vector2r previous, Vector2r current) {
            // update the stored item
            for (int i = 0; i < _items.Length; ++i) {
                if (ReferenceEquals(_items[i].Item, item)) {
                    _items[i].Position = current;
                    break;
                }
            }

            // update the monitors
            for (int i = 0; i < _monitors.Length; ++i) {
                StoredMonitor monitor = _monitors[i];

                bool containedPrevious = monitor.Region.Contains(previous);
                bool containedCurrent = monitor.Region.Contains(current);

                // the monitor was previously interested but no longer is
                if (containedPrevious && containedCurrent == false) {
                    monitor.Monitor.OnExit(item);
                }

                // the monitor was not previous interested but now is
                else if (containedPrevious == false && containedCurrent) {
                    monitor.Monitor.OnEnter(item);
                }
            }
        }

        public void Remove(T item, Vector2r position) {
            // remove the stored item
            for (int i = 0; i < _items.Length; ++i) {
                if (ReferenceEquals(_items[i].Item, item)) {
                    _items.Remove(i);
                    break;
                }
            }

            // call OnExit for all monitors that were previously interested in the item
            for (int i = 0; i < _monitors.Length; ++i) {
                StoredMonitor monitor = _monitors[i];
                if (monitor.Region.Contains(position)) {
                    monitor.Monitor.OnExit(item);
                }
            }
        }

        public void Add(IQuadTreeMonitor<T> monitor, Bound monitoredRegion) {
            _monitors.Add(new StoredMonitor(monitor, monitoredRegion));

            // check to see if the monitor is interested in any of our stored items
            for (int i = 0; i < _items.Length; ++i) {
                StoredItem item = _items[i];
                if (monitoredRegion.Contains(item.Position)) {
                    monitor.OnEnter(item.Item);
                }
            }
        }

        public void Update(IQuadTreeMonitor<T> monitor, Bound previousRegion, Bound currentRegion) {
            // update the stored monitor
            for (int i = 0; i < _monitors.Length; ++i) {
                if (ReferenceEquals(_monitors[i].Monitor, monitor)) {
                    _monitors[i].Region = currentRegion;
                    break;
                }
            }

            // update the monitor
            for (int i = 0; i < _items.Length; ++i) {
                StoredItem item = _items[i];

                bool containedPrevious = previousRegion.Contains(item.Position);
                bool containedCurrent = currentRegion.Contains(item.Position);

                // the monitor was previously interested but no longer is
                if (containedPrevious && containedCurrent == false) {
                    monitor.OnExit(item.Item);
                }

                // the monitor was not previous interested but now is
                else if (containedPrevious == false && containedCurrent) {
                    monitor.OnEnter(item.Item);
                }
            }
        }

        public void Remove(IQuadTreeMonitor<T> monitor, Bound monitoredRegion) {
            // remove the stored monitor
            for (int i = 0; i < _monitors.Length; ++i) {
                if (ReferenceEquals(_monitors[i].Monitor, monitor)) {
                    _monitors.Remove(i);
                    break;
                }
            }

            // remove all stored items from the monitor
            for (int i = 0; i < _items.Length; ++i) {
                StoredItem item = _items[i];

                if (monitoredRegion.Contains(item.Position)) {
                    monitor.OnExit(item.Item);
                }
            }
        }
    }

    public class QuadTree<T> {
        internal static ILog Log = LogManager.GetLogger(typeof(QuadTree<T>));

        /// <summary>
        /// The width/height of each chunk
        /// </summary>
        private int _baseChunkSize;

        /// <summary>
        /// All of the chunks
        /// </summary>
        private Node<T>[,] _chunks;

        public QuadTree(int baseChunkSize = 100) {
            _baseChunkSize = baseChunkSize;
            _chunks = new Node<T>[0, 0];
        }

        public void Add(T item, Vector2r position) {
            if (Log.IsDebugEnabled) Log.Debug("Adding " + item + " at " + position);

            Node<T> chunk = GetChunk(position.X.AsInt, position.Z.AsInt);
            chunk.Add(item, position);
        }

        public void Update(T item, Vector2r previous, Vector2r current) {
            if (Log.IsDebugEnabled) Log.Debug("Updating " + item + " from " + previous + " to " + current);

            Node<T> previousChunk = GetChunk(previous.X.AsInt, previous.Z.AsInt);
            Node<T> currentChunk = GetChunk(current.X.AsInt, current.Z.AsInt);

            if (ReferenceEquals(previousChunk, currentChunk)) {
                previousChunk.Update(item, previous, current);
            }
            else {
                previousChunk.Remove(item, previous);
                currentChunk.Add(item, current);
            }
        }

        public void Remove(T item, Vector2r position) {
            if (Log.IsDebugEnabled) Log.Debug("Removing " + item + " at " + position);

            Node<T> chunk = GetChunk(position.X.AsInt, position.Z.AsInt);
            chunk.Remove(item, position);
        }

        public void Insert(IQuadTreeMonitor<T> monitor, Bound monitoredRegion) {
            // convert the bounds to rectangles
            Rect positionRect = new Rect(monitoredRegion);

            // get our min/max game coordinates for iteration over chunks
            int minX = positionRect.X0;
            int minZ = positionRect.Z0;
            int maxX = positionRect.X1;
            int maxZ = positionRect.Z1;

            // make sure that we have enough chunks
            GetChunk(maxX, maxZ);

            // map our game coordinates to chunk indexes
            int minChunkX = MapChunkIndex(minX);
            int minChunkZ = MapChunkIndex(minZ);
            int maxChunkX = MapChunkIndex(maxX);
            int maxChunkZ = MapChunkIndex(maxZ);

            // iterate over our chunks and insert the monitor into each chunks
            for (int chunkX = minChunkX; chunkX <= maxChunkX; ++chunkX) {
                for (int chunkZ = minChunkZ; chunkZ <= minChunkZ; ++chunkZ) {
                    _chunks[chunkX, chunkZ].Add(monitor, monitoredRegion);
                }
            }
        }

        public void Remove(IQuadTreeMonitor<T> monitor, Bound monitoredRegion) {
            // convert the bounds to rectangles
            Rect positionRect = new Rect(monitoredRegion);

            // get our min/max game coordinates for iteration over chunks
            int minX = positionRect.X0;
            int minZ = positionRect.Z0;
            int maxX = positionRect.X1;
            int maxZ = positionRect.Z1;

            // make sure that we have enough chunks
            GetChunk(maxX, maxZ);

            // map our game coordinates to chunk indexes
            int minChunkX = MapChunkIndex(minX);
            int minChunkZ = MapChunkIndex(minZ);
            int maxChunkX = MapChunkIndex(maxX);
            int maxChunkZ = MapChunkIndex(maxZ);

            // iterate over our chunks and insert the monitor into each chunks
            for (int chunkX = minChunkX; chunkX <= maxChunkX; ++chunkX) {
                for (int chunkZ = minChunkZ; chunkZ <= minChunkZ; ++chunkZ) {
                    _chunks[chunkX, chunkZ].Remove(monitor, monitoredRegion);
                }
            }
        }

        public void Update(IQuadTreeMonitor<T> monitor, Bound previousRegion, Bound currentRegion) {
            // convert the bounds to rectangles
            Rect previousRect = new Rect(previousRegion);
            Rect currentRect = new Rect(currentRegion);

            // get our min/max game coordinates for iteration over chunks
            int minX = Math.Max(previousRect.X0, currentRect.X0);
            int minZ = Math.Max(previousRect.Z0, currentRect.Z0);
            int maxX = Math.Max(previousRect.X1, currentRect.X1);
            int maxZ = Math.Max(previousRect.Z1, currentRect.Z1);

            // make sure that we have enough chunks
            GetChunk(maxX, maxZ);

            // map our game coordinates to chunk indexes
            int minChunkX = MapChunkIndex(minX);
            int minChunkZ = MapChunkIndex(minZ);
            int maxChunkX = MapChunkIndex(maxX);
            int maxChunkZ = MapChunkIndex(maxZ);

            // map our rects into chunk indexes
            Rect previousChunkRect = new Rect(MapChunkIndex(previousRect.X0), MapChunkIndex(previousRect.Z0), MapChunkIndex(previousRect.X1), MapChunkIndex(previousRect.Z1));
            Rect currentChunkRect = new Rect(MapChunkIndex(currentRect.X0), MapChunkIndex(currentRect.Z0), MapChunkIndex(currentRect.X1), MapChunkIndex(currentRect.Z1));

            // iterate over our chunks
            for (int chunkX = minChunkX; chunkX <= maxChunkX; ++chunkX) {
                for (int chunkZ = minChunkZ; chunkZ <= minChunkZ; ++chunkZ) {
                    bool previousContains = previousChunkRect.Contains(chunkX, chunkZ);
                    bool currentContains = currentChunkRect.Contains(chunkX, chunkZ);

                    // the monitor is no longer interested in this chunk
                    if (previousContains && currentContains == false) {
                        _chunks[chunkX, chunkZ].Remove(monitor, previousRegion);
                    }

                    // the monitor is still interested in this chunk
                    else if (previousContains && currentContains) {
                        _chunks[chunkX, chunkZ].Update(monitor, previousRegion, currentRegion);
                    }

                    // the monitor is now interested in this chunk, but was not before
                    else if (previousContains == false && currentContains) {
                        _chunks[chunkX, chunkZ].Add(monitor, currentRegion);
                    }
                }
            }
        }

        #region Chunk Generation
        private Node<T> GetChunk(int sourceX, int sourceZ) {
            int x, z;
            GetChunkIndicies(sourceX, sourceZ, out x, out z);

            if (x >= _chunks.GetLength(0) || z >= _chunks.GetLength(1)) {
                _chunks = GrowArray(_chunks, x + 1, z + 1, GenerateChunk);
            }

            //Log.Info("Getting index at ({0}, {1}); length(0)={2}, length(1)={3}", x, z, _chunks.GetLength(0), _chunks.GetLength(1));
            return _chunks[x, z];
        }

        private Node<T> GenerateChunk(int x, int z) {
            /*
            UnmapIndex(ref x);
            UnmapIndex(ref z);

            int x0 = x * _baseChunkSize;
            int z0 = z * _baseChunkSize;
            int x1 = x0 + _baseChunkSize;
            int z1 = z0 + _baseChunkSize;
            Rect rect = new Rect(x0, z0, x1, z1);

            return new Node<T>(rect);
            */
            return new Node<T>();
        }

        private int MapChunkIndex(int source) {
            source /= _baseChunkSize;
            MapIndex(ref source);
            return source;
        }

        private void GetChunkIndicies(int sourceX, int sourceZ, out int x, out int z) {
            x = sourceX / _baseChunkSize;
            z = sourceZ / _baseChunkSize;

            MapIndex(ref x);
            MapIndex(ref z);
        }

        private void MapIndex(ref int v) {
            // - 4 -> 7
            // - 3 -> 5
            // - 2 -> 3
            // - 1 -> 1
            // 0 -> 0 1 -> 2 2 -> 4 3 -> 6 4 -> 8

            if (v >= 0) {
                v = (v * 2);
            }
            else {
                v = (-v * 2) - 1;
            }
        }

        private void UnmapIndex(ref int v) {
            // 0 -> 0 1 -> -1 2 -> 1 3 -> -2 4 -> 2 5 -> -3 6 -> 3 7 -> -4 8 -> 4

            // positive
            if (v % 2 == 0) {
                v = (v / 2);
            }

            // negative
            else {
                v = -(v + 1) / 2;
            }
        }

        private T[,] GrowArray<T>(T[,] original, int newXLength, int newZLength, Func<int, int, T> generator) {
            var newArray = new T[newXLength, newZLength];

            int originalXLength = original.GetLength(0);
            int originalZLength = original.GetLength(1);

            for (int x = 0; x < newXLength; x++) {
                for (int z = 0; z < newZLength; z++) {
                    T stored;
                    if (x >= originalXLength || z >= originalZLength) {
                        stored = generator(x, z);
                    }
                    else {
                        stored = original[x, z];
                    }

                    newArray[x, z] = stored;
                }
            }

            return newArray;
        }
        #endregion
    }
}