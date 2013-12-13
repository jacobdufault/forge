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

using Neon.Utilities;

namespace Neon.Collections {
    /// <summary>
    /// A set of items where only one is active and used.
    /// </summary>
    /// <typeparam name="T">The type of item stored.</typeparam>
    public class BufferedItem<T> where T : new() {
        /// <summary>
        /// The stored items
        /// </summary>
        private T[] Items;

        /// <summary>
        /// The _currentKeyboardState item that we are accessing
        /// </summary>
        private int CurrentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedItem{T}"/> class.
        /// </summary>
        /// <param name="count">The number of instances to allocate</param>
        public BufferedItem(int count = 2) {
            Items = new T[count];
            for (int i = 0; i < count; ++i) {
                Items[i] = new T();
            }

            CurrentIndex = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedItem{T}"/> class.
        /// </summary>
        /// <param name="instances">The instances to swap between.</param>
        public BufferedItem(params T[] instances) {
            Contract.Requires(instances.Length > 0);

            Items = instances;
            CurrentIndex = 0;
        }

        /// <summary>
        /// Swaps out the _currentKeyboardState item for the next one.
        /// </summary>
        /// <returns>The item that was deactivated</returns>
        public T Swap() {
            T ret = Get();

            ++CurrentIndex;
            if (CurrentIndex >= Items.Length) {
                CurrentIndex = 0;
            }

            return ret;
        }

        /// <summary>
        /// Gets the currently active item.
        /// </summary>
        public T Get() {
            return Items[CurrentIndex];
        }

        /// <summary>
        /// Returns an item in the rotation queue that is relative to the current item by the given
        /// offset.
        /// </summary>
        /// <param name="relativeOffset">How far away from the current item</param>
        /// <returns></returns>
        public T Get(int relativeOffset) {
            return Items[(CurrentIndex + relativeOffset) % Items.Length];
        }
    }
}