using Neon.Utility;
using System;
using System.Collections.Generic;

namespace Neon.Collections {
    /// <summary>
    /// Interface for objects which are monitoring a specific region.
    /// </summary>
    public interface IQuadTreeMonitor<T> {
        /// <summary>
        /// The region that the monitor is interested in.
        /// </summary>
        /// <remarks>
        /// This value *cannot* change over time; it must remain constant. If the value changes,
        /// then the monitor should be removed and then readded from the _quadTree.
        /// </remarks>
        Bound Region {
            get;
        }

        /// <summary>
        /// Called when the given item has entered the region.
        /// </summary>
        void OnEnter(T item);

        /// <summary>
        /// Called when an item has left the region.
        /// </summary>
        void OnExit(T item);
    }

    /// <summary>
    /// Stores objects by their position, allowing for the user to quickly identify objects which
    /// are nearby.
    /// </summary>
    /// <remarks>
    /// Notably this implementation isn't actually a quad-tree, but instead just a bunch of small
    /// rectangles.
    /// </remarks>
    /// <typeparam name="T">The type of object to store</typeparam>
    public class QuadTree<T> {
        private List<Tuple<T, Bound>>[,] _items;
        private List<IQuadTreeMonitor<T>>[,] _monitors;

        // TODO: remove me
        //private List<IQuadTreeMonitor<T>> _allMonitors = new List<IQuadTreeMonitor<T>>();

        /// <summary>
        /// How big each bucket of items is, where the bucket at (x, y) is defined by Items[x, y].
        /// </summary>
        private int BucketDimensions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketDimensions">How big (in positional units) each bucket of objects should be</param>
        public QuadTree(int bucketDimensions = 100) {
            BucketDimensions = bucketDimensions;

            int initialCapacity = 4;
            InitializeList(out _items, initialCapacity);
            InitializeList(out _monitors, initialCapacity);
        }

        private void InitializeList<G>(out List<G>[,] list, int initialCapacity) {
            list = new List<G>[initialCapacity, initialCapacity];
            for (int i = 0; i < initialCapacity; ++i) {
                for (int j = 0; j < initialCapacity; ++j) {
                    list[i, j] = new List<G>();
                }
            }
        }

        /// <summary>
        /// Updates the position of the given item.
        /// </summary>
        /// <param name="item">The item to update the position of</param>
        /// <param name="previous">The previous location</param>
        /// <param name="updated">The updated location</param>
        public void Update(T item, Bound previous, Bound updated) {
            var prevList = GetListsFor(_items, previous);
            var updatedList = GetListsFor(_items, updated);

            // update the lists that the item is in
            if (prevList != updatedList) {
                foreach (var list in prevList) {
                    for (int i = 0; i < list.Count; ++i) {
                        if (ReferenceEquals(list[i].Item1, item)) {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
                foreach (var list in updatedList) {
                    list.Add(Tuple.Create(item, updated));
                }
            }

            // get the monitors that we might want to notify
            var previousMonitors = CollectMonitors(previous);
            var updatedMonitors = CollectMonitors(updated);

            // get rid of all monitors from previousMonitors that are currently
            // in updatedMonitors; aka, the monitors where the item is no longer
            // present
            var t = new HashSet<IQuadTreeMonitor<T>>(previousMonitors);
            previousMonitors.ExceptWith(updatedMonitors);
            foreach (var monitor in previousMonitors) {
                monitor.OnExit(item);
            }
            previousMonitors = t;

            // remove all monitors from updatedMonitors that are in previousMonitors,
            // aka, the new monitors that the object has entered
            updatedMonitors.ExceptWith(previousMonitors);
            foreach (var monitor in updatedMonitors) {
                monitor.OnEnter(item);
            }
        }

        /// <summary>
        /// Notify the QuadTree that the monitor has changed positions. The current position
        /// is retrieved via calling IQuadTreeMonitor[T].Region
        /// </summary>
        /// <param name="monitor">The monitor to update</param>
        /// <param name="previous">The previous position of the monitor</param>
        public void UpdateMonitor(IQuadTreeMonitor<T> monitor, Bound previous) {
            var prevItems = CollectHashSet(previous);
            var updatedItems = CollectHashSet(monitor.Region);

            // remove all of the items from prev that are currently in updated; aka, prev becomes
            // the set of items that the monitor is no longer interested in
            var t = new HashSet<T>(prevItems);
            prevItems.ExceptWith(updatedItems);
            foreach (var item in prevItems) {
                monitor.OnExit(item);
            }
            prevItems = t;

            // remove all the items from updated that were in prev; aka, updated becomes the set of
            // items that were introduced by changing positions
            updatedItems.ExceptWith(prevItems);
            foreach (var item in updatedItems) {
                monitor.OnEnter(item);
            }
        }

        /// <summary>
        /// Returns a list of all items contained in the given area; this may return some items not actually in the area.
        /// </summary>
        public List<T> Collect(Bound area) {
            return new List<T>(CollectHashSet(area));
        }

        private HashSet<T> CollectHashSet(Bound area) {
            int x0, z0, x1, z1;
            ConvertCoordinatesFromWorldToLocal(area, out x0, out z0, out x1, out z1, ensureValidOuterIndicies: true);

            HashSet<T> result = new HashSet<T>();

            for (int x = x0; x < x1; ++x) {
                for (int z = z0; z < z1; ++z) {
                    var list = _items[x, z];
                    foreach (var item in list) {
                        if (area.Intersects(item.Item2)) {
                            result.Add(item.Item1);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of all items contained in the given area; this may return some items not actually in the area.
        /// </summary>
        private HashSet<IQuadTreeMonitor<T>> CollectMonitors(Bound area) {
            int x0, z0, x1, z1;
            ConvertCoordinatesFromWorldToLocal(area, out x0, out z0, out x1, out z1, ensureValidOuterIndicies: true);

            HashSet<IQuadTreeMonitor<T>> result = new HashSet<IQuadTreeMonitor<T>>();

            for (int x = x0; x < x1; ++x) {
                for (int z = z0; z < z1; ++z) {
                    var list = _monitors[x, z];
                    foreach (var item in list) {
                        if (area.Intersects(item.Region)) {
                            result.Add(item);
                        }
                    }
                }
            }

            // TODO: delete this loop and remove _allMonitors
            /*
            foreach (var monitor in _allMonitors) {
                if (area.Intersects(monitor.Region)) {
                    if (result.Add(monitor)) {
                        // this should **NEVER** trigger
                        Log.Info("Adding {0} to the set of monitors in region {1} in group 2", monitor, area);
                    }
                }
            }
            */

            return result;
        }

        /// <summary>
        /// Registers the given monitor.
        /// </summary>
        public void AddMonitor(IQuadTreeMonitor<T> monitor) {
            foreach (var list in GetListsFor(_monitors, monitor.Region)) {
                list.Add(monitor);
            }

            // add already-existing entities
            foreach (var entity in Collect(monitor.Region)) {
                monitor.OnEnter(entity);
            }

            //_allMonitors.Add(monitor);
        }

        /// <summary>
        /// Removes the given monitor from the tree.
        /// </summary>
        public void RemoveMonitor(IQuadTreeMonitor<T> monitor) {
            foreach (var list in GetListsFor(_monitors, monitor.Region)) {
                list.Remove(monitor);
            }

            // remove entities from the monitor
            foreach (var entity in Collect(monitor.Region)) {
                monitor.OnExit(entity);
            }

            //_allMonitors.Remove(monitor);
        }

        /// <summary>
        /// Adds the given item to the tree at the specified position.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        public void Add(T item, Bound position) {
            foreach (var list in GetListsFor(_items, position)) {
                list.Add(Tuple.Create(item, position));
            }

            // notify the monitors
            foreach (var monitor in CollectMonitors(position)) {
                monitor.OnEnter(item);
            }
        }

        /// <summary>
        /// Removes the item at the given position from the tree.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="position">Its position</param>
        /// <returns>If the item was removed</returns>
        public bool Remove(T item, Bound position) {
            bool found = false;
            foreach (var list in GetListsFor(_items, position)) {
                for (int i = 0; i < list.Count; ++i) {
                    if (ReferenceEquals(list[i].Item1, item)) {
                        found = true;
                        list.RemoveAt(i);
                        break;
                    }
                }
            }

            // notify the monitors
            if (found) {
                foreach (var monitor in CollectMonitors(position)) {
                    monitor.OnExit(item);
                }
            }

            return found;
        }

        /// <summary>
        /// Returns the storage list for the given position.
        /// </summary>
        private IEnumerable<List<G>> GetListsFor<G>(List<G>[,] source, Bound area) {
            int x0, z0, x1, z1;
            ConvertCoordinatesFromWorldToLocal(area, out x0, out z0, out x1, out z1, ensureValidOuterIndicies: false);

            EnsureCapacity(x1, z1);
            for (int x = x0; x < x1; ++x) {
                for (int z = z0; z < z1; ++z) {
                    yield return source[x, z];
                }
            }
        }

        /// <summary>
        /// Converts the coordinates in area down into indicies that can be used safely in the Items array.
        /// </summary>
        /// <param name="ensureValidOuterIndicies">If true, then x1/y1 will be no larger than Items.GetLength(0) or Items.GetLength(1), respectively</param>
        private void ConvertCoordinatesFromWorldToLocal(Bound area, out int x0, out int z0, out int x1, out int z1, bool ensureValidOuterIndicies) {
            Real tx0 = area.X - area.Radius;
            Real tz0 = area.Z - area.Radius;
            Real tx1 = area.X + area.Radius;
            Real tz1 = area.Z + area.Radius;

            x0 = Math.Max(tx0.AsInt / BucketDimensions - 1, 0);
            z0 = Math.Max(tz0.AsInt / BucketDimensions - 1, 0);
            x1 = Math.Min(tx1.AsInt / BucketDimensions + 1, ensureValidOuterIndicies ? _items.GetLength(0) : int.MaxValue);
            z1 = Math.Min(tz1.AsInt / BucketDimensions + 1, ensureValidOuterIndicies ? _items.GetLength(1) : int.MaxValue);
        }

        /// <summary>
        /// Verifies that there is a list at the given (x, z) indicies in the _items and _monitors arrays.
        /// </summary>
        /// <param name="x">The x index</param>
        /// <param name="z">The z index</param>
        private void EnsureCapacity(int x, int z) {
            int newX = -1, newZ = -1;

            // get the newX/newZ size if necessary to grow
            if (x >= _items.GetLength(0)) {
                newX = _items.GetLength(0);
                while (x >= newX)
                    newX *= 2;
            }
            if (z >= _items.GetLength(1)) {
                newZ = _items.GetLength(1);
                while (z >= newZ)
                    newZ *= 2;
            }

            // if we did not set newX or newZ, then we don't have to grow
            if (newX == -1 && newZ == -1) {
                return;
            }

            // newX or newZ != -1; both might not be the correct size; make sure that the sizes
            // make sense
            newX = newX == -1 ? _items.GetLength(0) : newX;
            newZ = newZ == -1 ? _items.GetLength(1) : newZ;

            // reallocate the new array
            var reallocatedItems = new List<Tuple<T, Bound>>[newX, newZ];
            var reallocatedMonitors = new List<IQuadTreeMonitor<T>>[newX, newZ];

            // copy or create elements over
            for (int i = 0; i < reallocatedItems.GetLength(0); ++i) {
                for (int j = 0; j < reallocatedItems.GetLength(1); ++j) {
                    List<Tuple<T, Bound>> valueItem;
                    List<IQuadTreeMonitor<T>> valueMonitor;

                    if (i < _items.GetLength(0) && j < _items.GetLength(1)) {
                        valueItem = _items[i, j];
                        valueMonitor = _monitors[i, j];
                    }
                    else {
                        valueItem = new List<Tuple<T, Bound>>();
                        valueMonitor = new List<IQuadTreeMonitor<T>>();
                    }

                    reallocatedItems[i, j] = valueItem;
                    reallocatedMonitors[i, j] = valueMonitor;
                }
            }

            // update reference
            _items = reallocatedItems;
            _monitors = reallocatedMonitors;
        }
    }
}
