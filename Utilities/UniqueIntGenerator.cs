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

using Newtonsoft.Json;
using System.Threading;

namespace Neon.Utilities {
    /// <summary>
    /// Generates unique integers that are sequential. This class is thread-safe.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UniqueIntGenerator {
        /// <summary>
        /// The next integer to generate.
        /// </summary>
        [JsonProperty("NextId")]
        private int _next;

        /// <summary>
        /// Returns the next unique int.
        /// </summary>
        public int Next() {
            return Interlocked.Increment(ref _next);
        }

        /// <summary>
        /// Notifies that UniqueIdGenerator that the given ID has already been consumed. Please note
        /// that this API is not thread-safe.
        /// </summary>
        public void Consume(int value) {
            if (value >= _next) {
                _next = value;
            }
        }
    }
}