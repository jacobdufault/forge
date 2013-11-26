using Neon.Collections;
using Neon.Utilities;
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
    internal class EntityTemplate : ITemplate {
        /// <summary>
        /// Default data instances, mapped by DataAccessor.
        /// </summary>
        private SparseArray<IData> _defaultDataInstances = new SparseArray<IData>();

        /// <summary>
        /// Event processor used for dispatching interesting events (ie, data adds and removes)
        /// </summary>
        private EventNotifier _eventProcessor = new EventNotifier();

        /// <summary>
        /// Generator for auto-generated template ids.
        /// </summary>
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        /// <summary>
        /// The entity manager that spawned entities are injected into.
        /// </summary>
        // TODO: figure out some good DI system
        public static GameEngine GameEngine;

        /// <summary>
        /// Returns the unique template id for the template.
        /// </summary>
        public int TemplateId {
            get;
            private set;
        }

        /// <summary>
        /// Returns the user-defined pretty name for the template. This is useful for debugging
        /// purposes.
        /// </summary>
        public string PrettyName {
            get;
            set;
        }

        /// <summary>
        /// Creates a new EntityTemplate with an automatically generated id and an empty pretty
        /// name.
        /// </summary>
        public EntityTemplate()
            : this(_idGenerator.Next(), "") {
        }

        /// <summary>
        /// Creates a new EntityTemplate with the given id and the given pretty name.
        /// </summary>
        public EntityTemplate(int id, string prettyName) {
            _idGenerator.Consume(id);
            TemplateId = id;
            PrettyName = prettyName;
        }

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <param name="instance">The data instance to copy from.</param>
        public void AddDefaultData(IData instance) {
            _defaultDataInstances[DataAccessorFactory.GetId(instance)] = instance;
        }

        /// <summary>
        /// Adds all of the data inside of this template into the given entity.
        /// </summary>
        /// <param name="entity">The entity to inject our data into</param>
        public void InjectDataInto(IEntity entity) {
            foreach (var tuple in _defaultDataInstances) {
                IData defaultData = tuple.Value;

                IData addedData = entity.AddOrModify(new DataAccessor(defaultData));
                addedData.CopyFrom(defaultData);
            }
        }

        /// <summary>
        /// Instantiates an entity from this template.
        /// </summary>
        /// <returns>A new entity instance that contains data instances based off of this
        /// template.</returns>
        public virtual IEntity InstantiateEntity() {
            Entity entity = new Entity();
            InjectDataInto(entity);

            GameEngine.AddEntity(entity);

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
        public ICollection<IData> SelectData(Predicate<IData> filter = null,
            ICollection<IData> storage = null) {
            if (storage == null) {
                storage = new List<IData>();
            }

            foreach (var tuple in _defaultDataInstances) {
                IData data = tuple.Value;

                if (filter == null || filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        ICollection<IData> IQueryableEntity.SelectCurrentData(Predicate<IData> filter,
            ICollection<IData> storage) {
            return SelectData(filter, storage);
        }

        IEventNotifier IQueryableEntity.EventNotifier {
            get { return _eventProcessor; }
        }

        IData IQueryableEntity.Current(DataAccessor accessor) {
            int id = accessor.Id;
            if (_defaultDataInstances.Contains(id) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _defaultDataInstances[accessor.Id];
        }

        IData IQueryableEntity.Previous(DataAccessor accessor) {
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
    }
}