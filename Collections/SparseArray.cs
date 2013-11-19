using Neon.Utilities;
using System;

namespace Neon.Collections {
    /// <summary>
    /// Stores a list of items where the are gaps between items; not every index in the array contains an element.
    /// </summary>
    public class SparseArray<T> {
        /// <summary>
        /// The internal array of elements inside of the array.
        /// </summary>
        private Maybe<T>[] Elements;

        /// <summary>
        /// Creates a new SparseArray with the given capacity.
        /// </summary>
        /// <param name="capacity">The capacity to initialize with</param>
        public SparseArray(int capacity = 8) {
            Elements = new Maybe<T>[capacity];
        }

        /// <summary>
        /// Gets or sets the element at the given index.
        /// </summary>
        /// <param name="index">The index to set</param>
        /// <returns>If getting, then the value at the given index.</returns>
        public T this[int index] {
            get {
                if (index >= Elements.Length || Elements[index].IsEmpty) {
                    return default(T);
                }

                return Elements[index].Value;
            }

            set {
                EnsureIndex(index);
                Elements[index] = Maybe.Just(value);
            }
        }

        /// <summary>
        /// Clears out all elements inside of the SparseArray.
        /// </summary>
        public void Clear() {
            Array.Clear(Elements, 0, Elements.Length);
        }

        /// <summary>
        /// Removes the element at the given index.
        /// </summary>
        /// <param name="index">The index of the element to remove</param>
        /// <returns>If an element was removed</returns>
        public bool Remove(int index) {
            bool removed = false;

            if (index < Elements.Length) {
                removed = Elements[index].Exists;
                Elements[index] = Maybe<T>.Empty;
            }

            return removed;
        }

        /// <summary>
        /// Checks to see if there is an element at the given index.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns>True if there is a contained element, false otherwise.</returns>
        public bool Contains(int index) {
            if (index >= Elements.Length || Elements[index].IsEmpty) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures that index is a valid index into the Elements array.
        /// </summary>
        /// <param name="index">The index to verify.</param>
        private void EnsureIndex(int index) {
            if (index >= Elements.Length) {
                int newSize = Elements.Length;
                while (index >= newSize) {
                    newSize *= 2;
                }

                Array.Resize(ref Elements, newSize);
            }
        }
    }
}