using Neon.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Collections {
    public struct Pair<A, B> {
        public A Item1;
        public B Item2;

        public Pair(A item1, B item2) {
            Item1 = item1;
            Item2 = item2;
        }
    }

    /// <summary>
    /// Stores a SparseArray that can be iterated in a time-efficient (but *not* space-efficient) manner.
    /// </summary>
    public class IterableSparseArray<T> : IEnumerable<Pair<int, T>> where T : class {
        private SparseArray<T> _sparseArray;
        private List<Pair<int, T>> _items;

        public IterableSparseArray(int capacity = 8) {
            _sparseArray = new SparseArray<T>(capacity);
            _items = new List<Pair<int, T>>();
        }

        public T this[int index] {
            get {
                return _sparseArray[index];
            }

            set {
                _sparseArray[index] = value;
                _items.Add(new Pair<int, T>(index, value));
            }
        }

        public bool Contains(int index) {
            return this[index] != null;
        }

        public void Clear() {
            _sparseArray.Clear();
            _items.Clear();
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        /// <returns>If the element was removed</returns>
        /// <remarks>
        /// This function has to do a linear search because of bookkeeping, unlike a SparseArray.
        /// </remarks>
        public bool Remove(int index) {
            bool removed = _sparseArray.Remove(index);
            if (removed) {
                for (int i = 0; i < _items.Count; ++i) {
                    if (_items[i].Item1 == index) {
                        _items.RemoveAt(i);
                        break;
                    }
                }
            }

            return removed;
        }

        public IEnumerator<Pair<int, T>> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}