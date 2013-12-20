using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Runtime {
    /// <summary>
    /// A runtime version of an ITemplate designed for efficiency.
    /// </summary>
    /// <remarks>
    /// This class does *NOT* implement JsonObject; it should never go through the save pipeline!
    /// Instead, convert it to a ContentTemplate and save that.
    /// </remarks>
    internal class RuntimeTemplate : ITemplate {
        /// <summary>
        /// The data instances inside of the template.
        /// </summary>
        private SparseArray<IData> _defaultDataInstances;

        /// <summary>
        /// The event notifier used to notify listeners of interesting events.
        /// </summary>
        private EventNotifier _eventNotifier;

        /// <summary>
        /// The game engine that entities are added to when they are instantiated.
        /// </summary>
        private GameEngine _gameEngine;

        public RuntimeTemplate(ContentTemplate content, GameEngine engine) {
            _defaultDataInstances = new SparseArray<IData>();
            _eventNotifier = new EventNotifier();
            _gameEngine = engine;

            TemplateId = content.TemplateId;
            PrettyName = content.PrettyName;
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
            throw new InvalidOperationException("Template cannot be modified while game is being played");
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
            throw new InvalidOperationException("Template cannot be modified while game is being played");
        }

        public int TemplateId {
            get;
            private set;
        }

        public IEntity InstantiateEntity() {
            RuntimeEntity entity = new RuntimeEntity(this);

            _gameEngine.AddEntity(entity);

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