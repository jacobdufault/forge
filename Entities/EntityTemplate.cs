using Neon.Collections;
using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An EntityTemplate contains default data values for another entity. While an EntityTemplate
    /// is not an entity itself, it supports the IQueryableEntity interface which means that it can
    /// be treated as a readable entity. Further, entities can be instantiated directly from the
    /// EntityTemplate which contain the same initial data values as those stored in the template.
    /// </summary>
    /// <remarks>
    /// Some methods in the IQueryableEntity interface, such as WasModified, do not really apply to
    /// EntityTemplate. In these scenarios, EntityTemplate returns intelligent defaults. For
    /// example, in the case of WasModified, EntityTemplate always returns false.
    /// </remarks>
    public class EntityTemplate : IQueryableEntity {
        /// <summary>
        /// Default data instances, mapped by DataAccessor.
        /// </summary>
        private IterableSparseArray<Data> _defaultDataInstances = new IterableSparseArray<Data>();

        /// <summary>
        /// Event processor used for dispatching interesting events (ie, data adds and removes)
        /// </summary>
        private EventProcessor _eventProcessor = new EventProcessor();

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
            set;
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
            _defaultDataInstances[DataAccessorFactory.GetId(instance)] = (instance);
        }

        /// <summary>
        /// Adds all of the data inside of this template into the given entity.
        /// </summary>
        /// <param name="entity">The entity to inject our data into</param>
        public void InjectDataInto(IEntity entity) {
            foreach (var tuple in _defaultDataInstances) {
                Data defaultData = tuple.Item2;

                Data addedData = entity.AddOrModify(new DataAccessor(defaultData));
                addedData.CopyFrom(defaultData);
            }
        }

        /// <summary>
        /// Instantiates an entity from this template.
        /// </summary>
        /// <returns>A new entity instance that contains data instances based off of this
        /// template.</returns>
        public virtual IEntity Instantiate(bool addToEntityManager = true) {
            Entity entity = new Entity();
            InjectDataInto(entity);

            if (addToEntityManager == false) {
                entity.ApplyModifications();
                entity.DataStateChangeUpdate();
            }
            else if (EntityManager != null) {
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

        /// <summary>
        /// Select data that is contained within the entity. This returns all data that passes the
        /// given filter.
        /// </summary>
        /// <param name="filter">An optional filter to check data with</param>
        /// <param name="storage">An optional storage location to store the result in; if null, then
        /// a new collection is created and returned</param>
        /// <returns></returns>
        public ICollection<Data> SelectData(Predicate<Data> filter = null,
            ICollection<Data> storage = null) {
            if (storage == null) {
                storage = new List<Data>();
            }

            foreach (var tuple in _defaultDataInstances) {
                Data data = tuple.Item2;

                if (filter == null || filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        ICollection<Data> IQueryableEntity.SelectCurrentData(Predicate<Data> filter,
            ICollection<Data> storage) {
            return SelectData(filter, storage);
        }

        EventProcessor IQueryableEntity.EventProcessor {
            get { return _eventProcessor; }
        }

        Data IQueryableEntity.Current(DataAccessor accessor) {
            int id = accessor.Id;
            if (_defaultDataInstances.Contains(id) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _defaultDataInstances[accessor.Id];
        }

        Data IQueryableEntity.Previous(DataAccessor accessor) {
            if (((IQueryableEntity)this).ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return ((IQueryableEntity)this).Current(accessor);
        }

        bool IQueryableEntity.ContainsData(DataAccessor accessor) {
            return _defaultDataInstances.Contains(accessor.Id);
        }

        bool IQueryableEntity.WasModified(DataAccessor accessor) {
            if (((IQueryableEntity)this).ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return false;
        }

        int IQueryableEntity.UniqueId {
            get { return TemplateId; }
        }
    }
}