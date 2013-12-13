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

namespace Neon.Collections {
    /// <summary>
    /// Contains two items which can be swapped between a Previous and a Current state.
    /// </summary>
    /// <typeparam name="T">The type of item stored</typeparam>
    public class SwappableItem<T> {
        /// <summary>
        /// The first item
        /// </summary>
        private T _a;

        /// <summary>
        /// The second item
        /// </summary>
        private T _b;

        /// <summary>
        /// The current item
        /// </summary>
        private bool _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwappableItem{T}"/> class.
        /// </summary>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        public SwappableItem(T a, T b) {
            _a = a;
            _b = b;
        }

        /// <summary>
        /// Swap the Current and Previous items.
        /// </summary>
        public void Swap() {
            _current = !_current;
        }

        /// <summary>
        /// The current item.
        /// </summary>
        public T Current {
            get {
                if (_current) {
                    return _a;
                }
                return _b;
            }
        }

        /// <summary>
        /// The previous item.
        /// </summary>
        public T Previous {
            get {
                if (_current) {
                    return _b;
                }
                return _a;
            }
        }
    }

}