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