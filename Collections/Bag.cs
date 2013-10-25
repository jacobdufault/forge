using Neon.Utilities;
using System;

namespace Neon.Collections {
    /// <summary>
    /// An unordered collection of items.
    /// </summary>
    public class Bag<T> {
        private T[] _items;

        public Bag(int capacity = 8) {
            _items = new T[capacity];
            Length = 0;
        }

        /// <summary>
        /// Copies from the given bag into this one.
        /// </summary>
        /// <param name="bag">The bag to copy from</param>
        public void CopyFrom(Bag<T> bag) {
            // Increase capacity if necessary
            if (bag.Length > _items.Length) {
                Array.Resize(ref _items, bag.Length);
            }

            // Copy items over
            Array.Copy(bag._items, _items, bag.Length);

            // Clear out old items
            if (Length > bag.Length) {
                Array.Clear(_items, bag.Length, Length - bag.Length);
            }

            // Update length
            Length = bag.Length;
        }

        /// <summary>
        /// Creates a duplicate of this bag that has a different backing array.
        /// </summary>
        public Bag<T> Copy() {
            Bag<T> bag = new Bag<T>(_items.Length);
            Array.Copy(_items, bag._items, Length);
            return bag;
        }

        /// <summary>
        /// Returns the index of the given item in the bag, or -1 if it is not found.
        /// </summary>
        public int IndexOf(T item) {
            for (int i = 0; i < Length; ++i) {
                if (ReferenceEquals(_items[i], item)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the item at given index from the bag.
        /// </summary>
        public void Remove(int index) {
            Contract.Requires(index >= 0);
            Contract.Requires(index < Length);

            // swap
            _items[index] = _items[Length - 1];

            // reduce length
            _items[Length - 1] = default(T);
            --Length;
        }

        /// <summary>
        /// Clears all stored items from this instance.
        /// </summary>
        public void Clear() {
            Array.Clear(_items, 0, Length);
            Length = 0;
        }

        /// <summary>
        /// Remove the item from the bag. This is O(n) and has to scan the bag to
        /// find the item.
        /// </summary>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        public bool Remove(T item) {
            int index = IndexOf(item);
            if (index == -1) {
                return false;
            }

            Remove(index);
            return true;
        }

        public void Append(T value) {
            if (Length == _items.Length) {
                Array.Resize(ref _items, (int)((_items.Length + 1) * 1.5));
            }

            _items[Length++] = value;
        }

        public T this[int index] {
            get {
                return _items[index];
            }
            set {
                _items[index] = value;
            }
        }

        public int Length {
            get;
            private set;
        }
    }
}
