using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An entity template contains default data values for another entity. While an EntityTemplate
    /// is not an entity itself, entities can be instantiated directly from it which contain the
    /// same initial data values as those stored in the template.
    /// </summary>
    public class EntityTemplate {
        /// <summary>
        /// Default data instances, mapped by DataAccessor.
        /// </summary>
        protected List<Data> _defaultDataInstances = new List<Data>();

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <param name="instance">The data instance to copy from.</param>
        public void AddDefaultData(Data instance) {
            _defaultDataInstances.Add(instance);
        }

        /// <summary>
        /// Adds all of the data inside of this template into the given entity.
        /// </summary>
        /// <param name="entity">The entity to inject our data into</param>
        public void InjectDataInto(IEntity entity) {
            for (int i = 0; i < _defaultDataInstances.Count; ++i) {
                Data defaultData = _defaultDataInstances[i];
                Data addedData = entity.AddOrModify(new DataAccessor(defaultData.GetType()));
                addedData.CopyFrom(defaultData);
            }
        }

        /// <summary>
        /// Instantiates an entity from this template.
        /// </summary>
        /// <returns>A new entity instance that contains data instances based off of this
        /// template.</returns>
        public virtual IEntity Instantiate() {
            IEntity entity = new Entity();
            InjectDataInto(entity);
            return entity;
        }

        /*
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
        */
    }
}