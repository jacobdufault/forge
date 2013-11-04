using System.Threading;

namespace Neon.Utilities {
    /// <summary>
    /// Generates unique integers that are sequential. This class is thread-safe.
    /// </summary>
    public class UniqueIntGenerator {
        /// <summary>
        /// The next integer to generate.
        /// </summary>
        private int _next;

        /// <summary>
        /// Returns the next unique int.
        /// </summary>
        public int Next() {
            return Interlocked.Increment(ref _next);
        }

        /// <summary>
        /// Notifies that UniqueIdGenerator that the given ID has already
        /// been consumed. Please note that this API is not thread-safe.
        /// </summary>
        public void Consume(int value) {
            if (value >= _next) {
                _next = value;
            }
        }
    }
}