using Neon.Utility;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Collections {
    /// <summary>
    /// Stores a SparseArray that can be iterated in a time-efficient (but *not* space-efficient) manner.
    /// </summary>
    public class IterableSparseArray<T> : IEnumerable<Tuple<int, T>> where T : class {
        private SparseArray<T> SparseArray;
        private List<Tuple<int, T>> Items;

        public IterableSparseArray(int capacity = 8) {
            SparseArray = new SparseArray<T>(capacity);
            Items = new List<Tuple<int, T>>();
        }

        public T this[int index] {
            get {
                return SparseArray[index];
            }

            set {
                SparseArray[index] = value;
                Items.Add(Tuple.Create(index, value));
            }
        }

        public bool Contains(int index) {
            return this[index] != null;
        }

        public void Clear() {
            SparseArray.Clear();
            Items.Clear();
        }

        public IEnumerator<Tuple<int, T>> GetEnumerator() {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}