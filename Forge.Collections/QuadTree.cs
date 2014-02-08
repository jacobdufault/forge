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

using Forge.Utilities;
using System;
using System.Collections.Generic;

namespace Forge.Collections {
    /// <summary>
    /// Interface for objects which are monitoring a specific region within a QuadTree.
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
            X1 = (bound.X + bound.Radius).AsInt;
            Z1 = (bound.Z + bound.Radius).AsInt;
        }

        public static Rect Merge(Rect a, Rect b) {
            int x0 = Math.Min(a.X0, b.X0);
            int z0 = Math.Min(a.Z0, b.Z0);
            int x1 = Math.Max(a.X1, b.X1);
            int z1 = Math.Max(a.Z1, b.Z1);

            return new Rect(x0, z0, x1, z1);
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

        /// <summary>
        /// Returns true if v is between [min, max).
        /// </summary>
        private static bool InRange(int v, int min, int max) {
            return v >= min && v < max;
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
        /// The items that the node contains
        /// </summary>
        private Bag<StoredItem> _items;

        /// <summary>
        /// The monitors that watch for adds/removes in this node
        /// </summary>
        private Bag<StoredMonitor> _monitors;

        /// <summary>
        /// The area that this node is monitoring
        /// </summary>
        public Rect MonitoredRegion {
            get;
            private set;
        }

        public Node(Rect monitoredRegion) {
            MonitoredRegion = monitoredRegion;
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

        /// <summary>
        /// Adds all of the items inside of this node that are contained within the given region to
        /// the given collection.
        /// </summary>
        /// <param name="region">The area to collect objects from.</param>
        /// <param name="storage">Where to store the collected objects.</param>
        public void CollectInto(Bound region, ICollection<T> storage) {
            for (int i = 0; i < _items.Length; ++i) {
                StoredItem item = _items[i];
                if (region.Contains(item.Position)) {
                    storage.Add(item.Item);
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

    /// <summary>
    /// Implements a QuadTree, which supports spatial monitoring and spatial querying of
    /// positionable objects. The objects can be positioned anywhere, even at negative coordinates.
    /// </summary>
    /// <remarks>
    /// In regards to implementation details, this is not currently a recursive QuadTree. Instead,
    /// the world is divided into a set of chunks which can be queried directly. The size of these
    /// chunks can be controlled by the constructor parameter.
    /// </remarks>
    /// <typeparam name="T">The type of object stored in the QuadTree</typeparam>
    public class QuadTree<TItem> {
        /// <summary>
        /// The width/height of each chunk
        /// </summary>
        private int _worldScale;

        /// <summary>
        /// All of the chunks
        /// </summary>
        private Node<TItem>[,] _chunks;

        public QuadTree(int worldScale = 100) {
            _worldScale = worldScale;
            _chunks = new Node<TItem>[0, 0];
        }

        #region Item API
        /// <summary>
        /// Add a new item to the QuadTree at the given position.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="position">The position of the item.</param>
        public void AddItem(TItem item, Vector2r position) {
            var chunk = GetChunk(position.X.AsInt, position.Z.AsInt);
            chunk.Add(item, position);
        }

        /// <summary>
        /// Update the position of an item. This will notify monitors of position updates.
        /// </summary>
        /// <param name="item">The item to update the position of.</param>
        /// <param name="previous">The old position of the item.</param>
        /// <param name="current">The updated position of the item.</param>
        public void UpdateItem(TItem item, Vector2r previous, Vector2r current) {
            var previousChunk = GetChunk(previous.X.AsInt, previous.Z.AsInt);
            var currentChunk = GetChunk(current.X.AsInt, current.Z.AsInt);

            if (ReferenceEquals(previousChunk, currentChunk)) {
                previousChunk.Update(item, previous, current);
            }
            else {
                previousChunk.Remove(item, previous);
                currentChunk.Add(item, current);
            }
        }

        /// <summary>
        /// Remove an item from the QuadTree.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="position">The position of the item.</param>
        public void RemoveItem(TItem item, Vector2r position) {
            var chunk = GetChunk(position.X.AsInt, position.Z.AsInt);
            chunk.Remove(item, position);
        }

        /// <summary>
        /// Collect all items that are stored inside of the given region.
        /// </summary>
        /// <param name="region">The area to collect entities from.</param>
        /// <param name="storage">Where to store the collected entities.</param>
        /// <returns>All entities that are contained in or intersecting with the given
        /// region.</returns>
        public ICollection<TItem> CollectItems(Bound region, ICollection<TItem> storage = null) {
            if (storage == null) {
                storage = new List<TItem>();
            }

            IterateChunks(region, node => {
                node.CollectInto(region, storage);
            });

            return storage;
        }
        #endregion

        #region Monitor API
        /// <summary>
        /// Inserts the given monitor into the QuadTree. The monitor will be notified of any
        /// entities that enter or leave it. The monitor can be updated or removed.
        /// </summary>
        /// <param name="monitor">The monitor.</param>
        /// <param name="monitoredRegion">The area that the monitor is viewing.</param>
        public void AddMonitor(IQuadTreeMonitor<TItem> monitor, Bound monitoredRegion) {
            IterateChunks(monitoredRegion, node => {
                node.Add(monitor, monitoredRegion);
            });
        }

        /// <summary>
        /// Removes the given monitor from the quad tree. It will receive a series of OnExit calls
        /// during this Remove call.
        /// </summary>
        /// <param name="monitor">The monitor to remove.</param>
        /// <param name="monitoredRegion">The region that the monitor was monitoring.</param>
        public void RemoveMonitor(IQuadTreeMonitor<TItem> monitor, Bound monitoredRegion) {
            IterateChunks(monitoredRegion, node => {
                node.Remove(monitor, monitoredRegion);
            });
        }

        /// <summary>
        /// Update the position of a monitor.
        /// </summary>
        /// <param name="monitor">The monitor whose position has changed.</param>
        /// <param name="previousRegion">The previous area that the monitor was watching.</param>
        /// <param name="currentRegion">The new area that the monitor is watching.</param>
        public void UpdateMonitor(IQuadTreeMonitor<TItem> monitor, Bound previousRegion, Bound currentRegion) {
            // convert the bounds to rectangles
            Rect previousRect = new Rect(previousRegion);
            Rect currentRect = new Rect(currentRegion);

            Rect merged = Rect.Merge(previousRect, currentRect);

            IterateChunks(merged, node => {
                bool previousContains = previousRect.Colliding(node.MonitoredRegion);
                bool currentContains = currentRect.Colliding(node.MonitoredRegion);

                // the monitor is no longer interested in this chunk
                if (previousContains && currentContains == false) {
                    node.Remove(monitor, previousRegion);
                }

                // the monitor is still interested in this chunk
                else if (previousContains && currentContains) {
                    node.Update(monitor, previousRegion, currentRegion);
                }

                // the monitor is now interested in this chunk, but was not before
                else if (previousContains == false && currentContains) {
                    node.Add(monitor, currentRegion);
                }
            });
        }
        #endregion

        #region Chunk Management
        /// <summary>
        /// Helper function to iterate all of the chunks that are contained within the given world
        /// region.
        /// </summary>
        private void IterateChunks(Bound region, Action<Node<TItem>> onChunk) {
            IterateChunks(new Rect(region), onChunk);
        }

        /// <summary>
        /// Helper function to iterate all of the chunks that are contained within the given world
        /// region.
        /// </summary>
        private void IterateChunks(Rect region, Action<Node<TItem>> onChunk) {
            for (int x = region.X0; x < (region.X1 + _worldScale); x += _worldScale) {
                for (int z = region.Z0; z < (region.Z1 + _worldScale); z += _worldScale) {
                    onChunk(GetChunk(x, z));
                }
            }
        }

        /// <summary>
        /// Returns the chunk located at the given x and z world coordinates.
        /// </summary>
        private Node<TItem> GetChunk(int xWorld, int zWorld) {
            int xIndex, zIndex;
            WorldIndexCoordinateTransform.ConvertWorldToIndex(_worldScale,
                xWorld, zWorld, out xIndex, out zIndex);

            if (xIndex >= _chunks.GetLength(0) || zIndex >= _chunks.GetLength(1)) {
                _chunks = GrowArray(_chunks, xIndex + 1, zIndex + 1, GenerateChunk);
            }

            Console.WriteLine("Getting index at ({0}, {1}); length(0)={2}, length(1)={3}",
                xIndex, zIndex, _chunks.GetLength(0), _chunks.GetLength(1));
            return _chunks[xIndex, zIndex];
        }

        /// <summary>
        /// Creates a new chunk located at the given x and z index coordinates.
        /// </summary>
        private Node<TItem> GenerateChunk(int xIndex, int zIndex) {
            int xWorld, zWorld;
            WorldIndexCoordinateTransform.ConvertIndexToWorld(_worldScale,
                xIndex, zIndex, out xWorld, out zWorld);

            Rect rect = new Rect(xWorld, zWorld, xWorld + _worldScale, zWorld + _worldScale);
            return new Node<TItem>(rect);
        }

        /// <summary>
        /// Helper method that grows a 2D array and populates new elements with values created from
        /// the generator.
        /// </summary>
        private static T[,] GrowArray<T>(T[,] original, int newLengthX, int newLengthZ, Func<int, int, T> generator) {
            var newArray = new T[newLengthX, newLengthZ];

            int originalLengthX = original.GetLength(0);
            int originalLengthZ = original.GetLength(1);

            for (int x = 0; x < newLengthX; x++) {
                for (int z = 0; z < newLengthZ; z++) {
                    T stored;
                    if (x >= originalLengthX || z >= originalLengthZ) {
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