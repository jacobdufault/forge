using Neon.Collections;
using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Shared {
    internal class Template : ITemplate {
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        private SparseArray<IData> _defaultDataInstances;
        private EventNotifier _eventNotifier;

        /// <summary>
        /// The game engine that entities are added to when they are instantiated.
        /// </summary>
        public GameEngine GameEngine {
            get;
            set;
        }

        public Template()
            : this(_idGenerator.Next(), "") {
        }

        public Template(int id, string prettyName) {
            _defaultDataInstances = new SparseArray<IData>();
            _eventNotifier = new EventNotifier();

            TemplateId = id;
            PrettyName = prettyName;
        }

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="data">The data instance to copy from.</param>
        public void AddDefaultData(IData data) {
            if (GameEngine != null) {
                throw new InvalidOperationException("Template cannot be modified while game is being played");
            }

            _defaultDataInstances[DataAccessorFactory.GetId(data)] = data;
        }

        /// <summary>
        /// Remove the given type of data from the template instance. New instances will not longer
        /// have this added to the template.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="accessor">The type of data to remove.</param>
        /// <returns>True if the data was removed.</returns>
        public bool RemoveDefaultData(DataAccessor accessor) {
            if (GameEngine != null) {
                throw new InvalidOperationException("Template cannot be modified while game is being played");
            }

            return _defaultDataInstances.Remove(accessor.Id);
        }

        public int TemplateId {
            get;
            private set;
        }

        public IEntity InstantiateEntity() {
            if (GameEngine == null) {
                throw new InvalidOperationException("Unable to instantiate entity with no game engine");
            }

            RuntimeEntity entity = new RuntimeEntity(this);

            GameEngine.AddEntity(entity);

            return entity;
        }

        public ICollection<IData> SelectCurrentData(Predicate<IData> filter = null,
            ICollection<IData> storage = null) {
            if (storage == null) {
                storage = new List<IData>();
            }

            foreach (var pair in _defaultDataInstances) {
                IData data = pair.Value;
                if (filter == null || filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        public IEventNotifier EventNotifier {
            get {
                return _eventNotifier;
            }
        }

        public IData Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _defaultDataInstances[accessor.Id];
        }

        public IData Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _defaultDataInstances[accessor.Id];
        }

        public bool ContainsData(DataAccessor accessor) {
            return _defaultDataInstances.Contains(accessor.Id);
        }

        public bool WasModified(DataAccessor accessor) {
            return false;
        }

        public string PrettyName {
            get;
            set;
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Template [tid={0}, name={1}]", TemplateId, PrettyName);
            }
            else {
                return string.Format("Template [tid={0}]", TemplateId);
            }
        }
    }
}