using Neon.Collections;
using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Entities.Implementation.Content {
    [JsonObject(MemberSerialization.OptIn)]
    internal class ContentTemplate : ITemplate {
        [JsonProperty("DefaultDataInstances")]
        private SparseArray<IData> _defaultDataInstances;

        private EventNotifier _eventNotifier;

        [JsonProperty("TemplateId")]
        public int TemplateId {
            get;
            private set;
        }

        [JsonProperty("PrettyName")]
        public string PrettyName {
            get;
            set;
        }

        public ContentTemplate(int id) {
            _defaultDataInstances = new SparseArray<IData>();
            _eventNotifier = new EventNotifier();

            TemplateId = id;
            PrettyName = "";
        }

        public ContentTemplate(ITemplate template)
            : this(template.TemplateId) {
            PrettyName = template.PrettyName;

            foreach (IData data in template.SelectCurrentData()) {
                AddDefaultData(data);
            }
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
            return _defaultDataInstances.Remove(accessor.Id);
        }

        public IEntity InstantiateEntity() {
            throw new InvalidOperationException("Unable to instantiate an entity from a ContentTemplate");
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