using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Content {
    internal class ContentEntity : IEntity {
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        private SparseArray<IData> _currentData;
        private SparseArray<IData> _previousData;
        private EventNotifier _eventNotifier;

        public ContentEntity()
            : this(_idGenerator.Next(), "") {
        }

        public ContentEntity(int uniqueId, string prettyName) {
            _currentData = new SparseArray<IData>();
            _previousData = new SparseArray<IData>();
            _eventNotifier = new EventNotifier();

            UniqueId = uniqueId;
            PrettyName = prettyName;
        }

        public int UniqueId {
            get;
            private set;
        }

        public void Destroy() {
            // do nothing
        }

        public IData AddOrModify(DataAccessor accessor) {
            if (ContainsData(accessor)) {
                return Modify(accessor);
            }

            return AddData(accessor);
        }

        public IData AddData(DataAccessor accessor) {
            if (ContainsData(accessor) == true) {
                throw new AlreadyAddedDataException(this, accessor);
            }

            Type dataType = DataAccessorFactory.GetTypeFromAccessor(accessor);
            _currentData[accessor.Id] = (IData)Activator.CreateInstance(dataType);
            _previousData[accessor.Id] = (IData)Activator.CreateInstance(dataType);

            return _currentData[accessor.Id];
        }

        public void RemoveData(DataAccessor accessor) {
            int id = accessor.Id;

            if (_currentData.Contains(id) == false || _previousData.Contains(id) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            _currentData.Remove(id);
            _previousData.Remove(id);
        }

        public IData Modify(DataAccessor accessor) {
            throw new NotImplementedException();
        }

        public ICollection<IData> SelectCurrentData(Predicate<IData> filter = null,
            ICollection<IData> storage = null) {
            if (storage == null) {
                storage = new List<IData>();
            }

            foreach (var pair in _currentData) {
                IData data = pair.Value;
                if (filter == null || filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        public IEventNotifier EventNotifier {
            get { return _eventNotifier; }
        }

        public IData Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _currentData[accessor.Id];
        }

        public IData Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _previousData[accessor.Id];
        }

        public bool ContainsData(DataAccessor accessor) {
            return _currentData.Contains(accessor.Id);
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