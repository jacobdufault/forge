using Neon.Utility;

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
    }
}