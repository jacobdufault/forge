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
        protected List<Data> _defaultDataInstances = new List<Data>();

        /// <summary>
        /// The entity manager that spawned entities are injected into.
        /// </summary>
        // TODO: figure out some good DI system
        public static EntityManager EntityManager;

        public int TemplateId {
            get;
            private set;
        }

        public string PrettyName {
            get;
            private set;
        }

        public EntityTemplate(int id, string prettyName) {
            TemplateId = id;
            PrettyName = prettyName;
        }

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
            if (EntityManager != null) {
                EntityManager.AddEntity(entity);
            }
            return entity;
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Template [tid={0}, name={1}]", TemplateId, PrettyName);
            }
            else {
                return string.Format("Template [tid={0}]", TemplateId);
            }
        }

        public IEnumerator<Data> GetEnumerator() {
            return _defaultDataInstances.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _defaultDataInstances.GetEnumerator();
        }
    }
}