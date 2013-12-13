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
using System.Linq;
using System.Text;
using System.Threading;

namespace Neon.Utilities {
    /// <summary>
    /// Thread-safe activation trigger that only activates once.
    /// </summary>
    /// <remarks>
    /// In a sense, this is equivalent to extending Interlocked to operate on booleans.
    /// </remarks>
    public class AtomicActivation {
        /// <summary>
        /// An activated activation state
        /// </summary>
        private const long ACTIVATED = 1;

        /// <summary>
        /// A deactivated activation state.
        /// </summary>
        private const long UNACTIVATED = 0;

        /// <summary>
        /// Have we been activated?
        /// </summary>
        private long _activated;

        /// <summary>
        /// Initializes a new instance of the AtomicActivation class in an unactivated state.
        /// </summary>
        public AtomicActivation() {
            Reset();
        }

        /// <summary>
        /// Resets the activation state, so that Activate() will return true on then next call.
        /// </summary>
        public void Reset() {
            Interlocked.Exchange(ref _activated, UNACTIVATED);
        }

        /// <summary>
        /// Returns true if the current activation state is activated.
        /// </summary>
        public bool IsActivated {
            get {
                return Interlocked.Read(ref _activated) == ACTIVATED;
            }
        }

        /// <summary>
        /// Returns true if the activation state was previously unactivated.
        /// </summary>
        /// <returns>True if the activation activated for this call</returns>
        public bool TryActivate() {
            long previousValue = Interlocked.Exchange(ref _activated, ACTIVATED);
            if (previousValue == UNACTIVATED) {
                return true;
            }

            return false;
        }
    }
}