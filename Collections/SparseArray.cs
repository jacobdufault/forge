using Neon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Collections {
    /// <summary>
    /// Stores a list of items where the are gaps between items; not every index in the array
    /// contains an element.
    /// </summary>
    public class SparseArray<T> : IDictionary<int, T> {
        /// <summary>
        /// The internal array of elements inside of the array.
        /// </summary>
        private Maybe<T>[] _items;

        /// <summary>
        /// Creates a new SparseArray with the default capacity.
        /// </summary>
        public SparseArray()
            : this(8) {
        }

        /// <summary>
        /// Creates a new SparseArray with the given capacity.
        /// </summary>
        /// <param name="capacity">The capacity to initialize with</param>
        public SparseArray(int capacity) {
            _items = new Maybe<T>[capacity];
        }

        /// <summary>
        /// Gets or sets the element at the given index.
        /// </summary>
        /// <param name="index">The index to set</param>
        /// <returns>If getting, then the value at the given index.</returns>
        public T this[int index] {
            get {
                if (index >= _items.Length || _items[index].IsEmpty) {
                    return default(T);
                }

                return _items[index].Value;
            }

            set {
                EnsureIndex(index);
                _items[index] = Maybe.Just(value);
            }
        }

        /// <summary>
        /// Clears out all elements inside of the SparseArray.
        /// </summary>
        public void Clear() {
            Array.Clear(_items, 0, _items.Length);
        }

        /// <summary>
        /// Removes the element at the given index.
        /// </summary>
        /// <param name="index">The index of the element to remove</param>
        /// <returns>If an element was removed</returns>
        public bool Remove(int index) {
            bool removed = false;

            if (index >= 0 && index < _items.Length) {
                removed = _items[index].Exists;
                _items[index] = Maybe<T>.Empty;
            }

            return removed;
        }

        /// <summary>
        /// Checks to see if there is an element at the given index.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns>True if there is a contained element, false otherwise.</returns>
        public bool Contains(int index) {
            if (index >= _items.Length || _items[index].IsEmpty) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures that index is a valid index into the Elements array.
        /// </summary>
        /// <param name="index">The index to verify.</param>
        private void EnsureIndex(int index) {
            if (index >= _items.Length) {
                int newSize = _items.Length;
                while (index >= newSize) {
                    newSize *= 2;
                }

                Array.Resize(ref _items, newSize);
            }
        }

        void IDictionary<int, T>.Add(int key, T value) {
            this[key] = value;
        }

        bool IDictionary<int, T>.ContainsKey(int key) {
            return Contains(key);
        }

        ICollection<int> IDictionary<int, T>.Keys {
            get {
                return (from pair in this
                        select pair.Key).ToList();
            }
        }

        bool IDictionary<int, T>.TryGetValue(int key, out T value) {
            value = this[key];
            return Contains(key);
        }

        ICollection<T> IDictionary<int, T>.Values {
            get {
                return (from pair in this
                        select pair.Value).ToList();
            }
        }

        void ICollection<KeyValuePair<int, T>>.Add(KeyValuePair<int, T> item) {
            this[item.Key] = item.Value;
        }

        bool ICollection<KeyValuePair<int, T>>.Contains(KeyValuePair<int, T> item) {
            foreach (KeyValuePair<int, T> pair in this) {
                if (pair.Value.Equals(item)) {
                    return true;
                }
            }

            return false;
        }

        void ICollection<KeyValuePair<int, T>>.CopyTo(KeyValuePair<int, T>[] array, int arrayIndex) {
            int index = arrayIndex;
            foreach (KeyValuePair<int, T> pair in this) {
                if (arrayIndex >= array.Length) {
                    break;
                }

                array[index++] = pair;
            }
        }

        int ICollection<KeyValuePair<int, T>>.Count {
            get {
                // just iterate through all the item and count them... slow an inefficient, but it
                // works for now
                int count = 0;
                foreach (var item in this) {
                    ++count;
                }
                return count;
            }
        }

        bool ICollection<KeyValuePair<int, T>>.IsReadOnly {
            get { return false; }
        }

        bool ICollection<KeyValuePair<int, T>>.Remove(KeyValuePair<int, T> item) {
            // this is different than Remove(int), because we have to make sure that the value at
            // the given index is the same as the item in item.Value

            if (item.Key >= 0 && item.Key < _items.Length) {
                Maybe<T> current = _items[item.Key];
                if (current.Exists && current.Value.Equals(item.Value)) {
                    _items[item.Key] = Maybe<T>.Empty;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<KeyValuePair<int, T>> GetEnumerator() {
            // TODO: If performance reveals this as hot spot, implement a jump-list for iteration,
            //       where we store the indexes for regions of contiguous values. For example, if
            //       there are values at indexes [0,1,2,5,6,8,11], then the jump-list is [0,5,8,11].

            for (int i = 0; i < _items.Length; ++i) {
                if (_items[i].Exists) {
                    T value = _items[i].Value;
                    yield return new KeyValuePair<int, T>(i, value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}