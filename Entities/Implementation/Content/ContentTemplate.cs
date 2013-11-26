using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Content {
    internal class ContentTemplate : ITemplate {
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        private SparseArray<IData> _data;
        private EventNotifier _eventNotifier;

        public ContentTemplate()
            : this(_idGenerator.Next(), "") {
        }

        public ContentTemplate(int id, string prettyName) {
            _data = new SparseArray<IData>();
            _eventNotifier = new EventNotifier();

            TemplateId = id;
            PrettyName = prettyName;
        }

        public int TemplateId {
            get;
            private set;
        }

        public IEntity InstantiateEntity() {
            ContentEntity entity = new ContentEntity();

            foreach (var pair in _data) {
                IData data = pair.Value;

                IData added = entity.AddData(new DataAccessor(data));
                added.CopyFrom(data);
            }

            return entity;
        }

        public ICollection<IData> SelectCurrentData(Predicate<IData> filter = null,
            ICollection<IData> storage = null) {
            if (storage == null) {
                storage = new List<IData>();
            }

            foreach (var pair in _data) {
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

            return _data[accessor.Id];
        }

        public IData Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id];
        }

        public bool ContainsData(DataAccessor accessor) {
            return _data.Contains(accessor.Id);
        }

        public bool WasModified(DataAccessor accessor) {
            return false;
        }

        public string PrettyName {
            get;
            set;
        }
    }
}