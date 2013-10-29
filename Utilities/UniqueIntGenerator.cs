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
    }
}