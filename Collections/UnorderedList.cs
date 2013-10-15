using Neon.Utility;
using System;
using System.Collections.Generic;

namespace Neon.Collections {
    public class UnorderedListMetadata {
        internal int Index {
            get;
            set;
        }

        internal object StoredIn {
            get;
            set;
        }

        public UnorderedListMetadata() {
            Index = -1;
            StoredIn = null;
        }
    }

    /// <summary>
    /// A list of items that is unordered. This provides O(1) addition, O(1) removal, and O(1) iteration.
    /// However, having all of these nice properties requires some metadata to be stored on each item,
    /// which means that Add takes a parameter of metadata for the stored item. Same with remove.
    /// </summary>
    /// <typeparam name="T">The type of item to store.</typeparam>
    public class UnorderedList<T> : IEnumerable<T> {
        /// <summary>
        /// Data stored inside of the UnorderedList.
        /// </summary>
        private struct StoredItem {
            public T Item;
            public UnorderedListMetadata Metadata;
        }

        /// <summary>
        /// The stored list of items.
        /// </summary>
        private StoredItem[] Items;

        /// <summary>
        /// Creates a new UnorderedList with the given capacity.
        /// </summary>
        /// <param name="capacity">The capacity to give to the list</param>
        public UnorderedList(int capacity = 8) {
            Items = new StoredItem[capacity];
            Length = 0;
        }

        /// <summary>
        /// The number of stored objects in Items.
        /// </summary>
        public int Length {
            get;
            private set;
        }

        private StoredItem Pop() {
            StoredItem item = Items[Length - 1];
            Items[Length - 1] = default(StoredItem);
            --Length;
            return item;
        }

        /// <summary>
        /// Checks to see if the given item is contained in the UnorderedList. This is O(1).
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <param name="metadata">The item's metadata</param>
        /// <returns>True if the item is contained, false otherwise</returns>
        public bool Contains(T item, UnorderedListMetadata metadata) {
            if (metadata.Index < 0) {
                return false;
            }

            if (metadata.Index >= Length) {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(Items[metadata.Index].Item, item);
        }

        /// <summary>
        /// Removes the stored item.
        /// </summary>
        /// <returns>True if the item was removed, false otherwise</returns>
        public bool Remove(T item, UnorderedListMetadata metadata) {
            if (metadata.Index == -1) {
                return false;
            }

            int index = metadata.Index;
            metadata.Index = -1;
            metadata.StoredIn = null;

            StoredItem replacer = Pop();

            // If the item to remove was the item at the end of the list or there are no items left then the popped item
            // was equal to the item itself, so just return
            if (index == Length || Length == 0) {
                return true;
            }

            // Replace the removed item with the popped item.
            replacer.Metadata.Index = index;
            replacer.Metadata.StoredIn = this;
            Items[index] = replacer;
            return true;
        }

        /// <summary>
        /// Adds an item to the list. The location of the item in the list is unspecified.
        /// </summary>
        public void Add(T item, UnorderedListMetadata metadata) {
            // Make sure the item is not in another list
            if (metadata.Index != -1) {
                throw new Exception(String.Format("Item {0} is already stored in another UnorderedList, or has been added twice to the same one", item));
            }

            // Grow the array if necessary
            if (Length == Items.Length) {
                Array.Resize(ref Items, Items.Length * 2);
            }

            // Add the item
            metadata.Index = Length;
            metadata.StoredIn = this;
            Items[Length++] = new StoredItem() {
                Item = item,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Set or get the item at the specified index.
        /// </summary>
        public T this[int index] {
            get {
                Contract.Requires(index >= 0 && index < Length, "The index is out of bounds");

                return Items[index].Item;
            }

            set {
                Contract.Requires(index >= 0 && index < Length, "The index is out of bounds");

                Items[index].Item = value;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Length; ++i) {
                yield return Items[i].Item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}