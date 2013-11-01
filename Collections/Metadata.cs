using Neon.Utilities;
using System.Runtime.CompilerServices;

namespace Neon.Collections {
    /// <summary>
    /// Used to create metadata keys that can access data in MetadataContainers.
    /// </summary>
    public class MetadataRegistry {
        private UniqueIntGenerator _keyGenerator = new UniqueIntGenerator();

        public MetadataKey GetKey() {
            return new MetadataKey() {
                Index = _keyGenerator.Next()
            };
        }
    }

    /// <summary>
    /// Stores metadata.
    /// </summary>
    public class MetadataContainer<T> {
        /// <summary>
        /// The actual data storage
        /// </summary>
        private SparseArray<T> _container = new SparseArray<T>();

        /// <summary>
        /// Returns the stored metadata value for the given key.
        /// </summary>
        public T Get(MetadataKey key) {
            //lock (this) {
                return _container[key.Index];
            //}
        }

        /// <summary>
        /// Updates the stored metadata value for the given key.
        /// </summary>
        public void Set(MetadataKey key, T value) {
            //lock (this) {
                _container[key.Index] = value;
            //}
        }

        /// <summary>
        /// Attempts to remove the object store at the key.
        /// </summary>
        /// <remarks>
        /// This just calls Set(key, default(T)).
        /// </remarks>
        public void Remove(MetadataKey key) {
            Set(key, default(T));
        }

        /// <summary>
        /// Store or retrieve a metadata value.
        /// </summary>
        /// <param name="key">The key used. Used CreateKey to create a new one.</param>
        /// <returns>The stored metadata.</returns>
        public T this[MetadataKey key] {
            get {
                return Get(key);
            }

            set {
                Set(key, value);
            }
        }
    }

    /// <summary>
    /// Stores a slot inside of a metadata container.
    /// </summary>
    public struct MetadataKey {
        /// <summary>
        /// The index into the internal array that the key maps to
        /// </summary>
        internal int Index;
    }
}