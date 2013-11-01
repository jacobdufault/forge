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
