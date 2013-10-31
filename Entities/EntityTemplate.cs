using Neon.Collections;
using Neon.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An entity template contains default data values for another entity. While an EntityTemplate
    /// is not an entity itself, entities can be instantiated directly from it which contain the
    /// same initial data values as those stored in the template.
    /// </summary>
    public class EntityTemplate : IEnumerable<Data> {
        /// <summary>
        /// Default data instances, mapped by DataAccessor.
        /// </summary>
        private IterableSparseArray<Data> _defaultDataInstances = new IterableSparseArray<Data>();

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <param name="instance">The data instance to copy from.</param>
        public void AddDefaultData(Data instance) {
            int id = DataAccessorFactory.GetId(instance.GetType());
            _defaultDataInstances[id] = instance;
        }

        /// <summary>
        /// Attempts to return the default data instance for the given Data type in the given
        /// Entity.
        /// </summary>
        /// <remarks>
        /// The returned data instance should *NEVER* be modified.
        /// </remarks>
        /// <param name="dataType">The type of data to lookup</param>
        /// <returns>A potential data instance that contains default values.</returns>
        public Maybe<Data> GetDefaultInstance(DataAccessor dataType) {
            if (_defaultDataInstances.Contains(dataType.Id)) {
                return Maybe<Data>.Just(_defaultDataInstances[dataType.Id]);
            }

            return Maybe<Data>.Empty;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to
        /// iterate through the collection.</returns>
        public IEnumerator<Data> GetEnumerator() {
            foreach (var tuple in _defaultDataInstances) {
                yield return tuple.Item2;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to
        /// iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return _defaultDataInstances.GetEnumerator();
        }
    }
}