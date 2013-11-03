using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Collections {
    /// <summary>
    /// Provides a queue where pushing is assumed to be done concurrently, but reading is done in a
    /// single-thread.
    /// </summary>
    /// <typeparam name="T">The type of object stored.</typeparam>
    public class ConcurrentWriterBag<T> {
        /// <summary>
        /// All thread-local bags; this is used when iterating over the entire contents of the bag.
        /// </summary>
        private List<Bag<T>> _allCollections;

        /// <summary>
        /// The thread-local bag that is used for appending items.
        /// </summary>
        private ThreadLocal<Bag<T>> _collection;

        public ConcurrentWriterBag() {
            _allCollections = new List<Bag<T>>();

            _collection = new ThreadLocal<Bag<T>>(() => {
                Bag<T> bag = new Bag<T>();
                lock (_allCollections) {
                    _allCollections.Add(bag);
                }
                return bag;
            });
        }

        /// <summary>
        /// Adds the item in the collection.
        /// </summary>
        /// <remarks>
        /// This is a thread-safe function.
        /// </remarks>
        /// <param name="item">The item to enqueue</param>
        public void Add(T item) {
            _collection.Value.Append(item);
        }

        /// <summary>
        /// Calls the iterator over every item in the bag and then clears the bags that were
        /// iterated.
        /// </summary>
        /// <remarks>
        /// This method is **NOT** thread-safe; do NOT call Add while iterating the items.
        /// </remarks>
        /// <param name="iterator">The function to invoke on the items.</param>
        public void IterateAndClear(Action<T> iterator) {

            for (int i = 0; i < _allCollections.Count; ++i) {
                Bag<T> collection = _allCollections[i];
                for (int j = 0; j < collection.Length; ++j) {
                    iterator(collection[j]);
                }
                collection.Clear();
            }
        }
    }
}