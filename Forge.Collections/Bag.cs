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
using System.Collections;
using System.Collections.Generic;

namespace Forge.Collections {
    /// <summary>
    /// An unordered collection of items.
    /// </summary>
    public sealed class Bag<T> : IList<T>, ICollection<T>, ICollection, IEnumerable<T>, IEnumerable {
        private T[] _items;

        public Bag()
            : this(8) {
        }

        public Bag(int capacity) {
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
            return Array.IndexOf(_items, item, 0, Length);
        }

        /// <summary>
        /// Removes the item at given index from the bag in O(1) time. This operation does not
        /// maintain the order of elements inside of the bag!
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
        /// Remove the item from the bag. This is O(n) and has to scan the bag to find the item.
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

        public void Add(T value) {
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

        #region IList<T> Interface

        // not a general method because it's contract isn't very useful for a bag...
        void IList<T>.Insert(int index, T item) {
            if (Length == _items.Length) {
                Array.Resize(ref _items, (int)((_items.Length + 1) * 1.5));
            }

            // shift elements forwards
            for (int i = Length - 1; i > index; --i) {
                _items[i] = _items[i - 1];
            }

            // update the reference at index
            _items[index] = item;

            ++Length;
        }

        // not a general method because it's contract isn't very useful for a bag...
        void IList<T>.RemoveAt(int index) {
            for (int i = index; i < (Length - 1); ++i) {
                _items[i] = _items[i + 1];
            }
            _items[Length] = default(T);
            --Length;
        }

        /// <summary>
        /// Returns true if the Bag contains an instance of the given item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>True if it is in the bag, false otherwise.</returns>
        public bool Contains(T item) {
            return IndexOf(item) >= 0;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < Length; ++i) {
                int targetIndex = i + arrayIndex;

                if (targetIndex > array.Length) {
                    break;
                }

                array[targetIndex] = _items[i];
            }
        }

        int ICollection<T>.Count {
            get { return Length; }
        }

        bool ICollection<T>.IsReadOnly {
            get { return false; }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            for (int i = 0; i < Length; ++i) {
                yield return _items[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            for (int i = 0; i < Length; ++i) {
                yield return _items[i];
            }
        }
        #endregion

        #region ICollection Interface
        void ICollection.CopyTo(Array array, int index) {
            ((ICollection<T>)this).CopyTo((T[])array, index);
        }

        int ICollection.Count {
            get {
                return ((ICollection<T>)this).Count;
            }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }
        #endregion
    }
}