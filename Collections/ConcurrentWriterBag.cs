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

using System;
using System.Collections.Generic;
using System.Threading;

namespace Forge.Collections {
    /// <summary>
    /// Provides a queue where pushing is assumed to be done concurrently, but reading is done in a
    /// single-thread.
    /// </summary>
    /// <typeparam name="T">The type of object stored.</typeparam>
    // TODO: consider making this class disposable
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
            _collection.Value.Add(item);
        }

        /// <summary>
        /// Returns all items inside of the bag as a list. This method is not thread safe. This
        /// method does *not* clear the collection.
        /// </summary>
        public List<T> ToList() {
            List<T> result = new List<T>();
            for (int i = 0; i < _allCollections.Count; ++i) {
                Bag<T> collection = _allCollections[i];
                for (int j = 0; j < collection.Length; ++j) {
                    result.Add(collection[j]);
                }
            }
            return result;
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

        /// <summary>
        /// Adds all of the elements inside of this bag into the given destination collection.
        /// </summary>
        /// <remarks>
        /// This method is **NOT** thread-safe; do NOT call Add before this method has returned.
        /// </remarks>
        /// <param name="destination">The collection to copy items into.</param>
        public void CopyIntoAndClear(ICollection<T> destination) {
            for (int i = 0; i < _allCollections.Count; ++i) {
                Bag<T> collection = _allCollections[i];
                for (int j = 0; j < collection.Length; ++j) {
                    destination.Add(collection[j]);
                }
                collection.Clear();
            }
        }
    }
}