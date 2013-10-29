using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Entities {
    public class Entity : IEntity {
        public bool Enabled {
            get;
            set;
        }

        public Data AddData(DataAccessor accessor) {
            // ensure that we have not already added a data of this type
            if (GetAddedData(accessor) != null) {
                throw new AlreadyAddedDataException(this, DataFactory.GetTypeFromAccessor(accessor));
            }

            // add our data
            Data data = DataAllocator.Allocate(accessor);
            data.Entity = this;
            _toAdd.Add(data);

            // notify the entity manager
            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();

            // populate the data instance from the prefab / etc
            DataAllocator.NotifyAllocated(accessor, this, data);

            // return the new instance
            return data;
        }

        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        public int UniqueId {
            get;
            private set;
        }

        public EventProcessor EventProcessor {
            get;
            private set;
        }

        protected internal Entity() {
            UniqueId = _idGenerator.Next();

            Enabled = true; // default to being enabled

            EventProcessor = new EventProcessor();

            DataStateChangeNotifier = new Notifier<Entity>(this);
            ModificationNotifier = new Notifier<Entity>(this);
        }

        public override string ToString() {
            return string.Format("Entity [uid={0}]", UniqueId);
        }

        public event Action OnHide;

        public event Action OnShow;

        public event Action OnRemoved;

        public void RemovedFromEntityManager() {
            if (OnRemoved != null) {
                OnRemoved();
            }
        }

        public void Hide() {
            if (OnHide != null) {
                OnHide();
            }
        }

        public void Show() {
            if (OnShow != null) {
                OnShow();
            }
        }

        private EntityManager _entityManager;

        public void AddedToEntityManager(EntityManager entityManager) {
            _entityManager = entityManager;
        }

        public void RemoveData(DataAccessor accessor) {
            _toRemove.Current.Add(accessor);

            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();
        }

        public void Destroy() {
            _entityManager.RemoveEntity(this);
        }

        public Notifier<Entity> DataStateChangeNotifier;
        public Notifier<Entity> ModificationNotifier;

        /// <summary>
        /// The data contained within the Entity. One item in the tuple is the current state and one
        /// item is the next state.
        /// </summary>
        private IterableSparseArray<ImmutableContainer<Data>> _data = new IterableSparseArray<ImmutableContainer<Data>>();

        /// <summary>
        /// Data that has been modified this frame and needs to be pushed out
        /// </summary>
        private SwappableItem<IterableSparseArray<ImmutableContainer<Data>>> _modifications = new SwappableItem<IterableSparseArray<ImmutableContainer<Data>>>(
            new IterableSparseArray<ImmutableContainer<Data>>(), new IterableSparseArray<ImmutableContainer<Data>>());

        /// <summary>
        /// Items that are pending removal in the next update call
        /// </summary>

        private List<Data> _toAdd = new List<Data>();

        private SwappableItem<List<DataAccessor>> _toRemove = new SwappableItem<List<DataAccessor>>(new List<DataAccessor>(), new List<DataAccessor>());
        private SparseArray<DataAccessor> _removed = new SparseArray<DataAccessor>();

        /// <summary>
        /// Object that is used to retrieve unordered list metadata from the entity.
        /// </summary>
        private MetadataContainer<object> _metadata = new MetadataContainer<object>();

        public MetadataContainer<object> Metadata {
            get {
                return _metadata;
            }
        }

        public static MetadataRegistry MetadataRegistry = new MetadataRegistry();

        /// <summary>
        /// Removes all data from the entity.
        /// </summary>
        public void RemoveAllData() {
            // TODO: potentially optimize this method
            foreach (var tuple in _data) {
                DataAccessor accessor = new DataAccessor(tuple.Item2.Current.GetType());
                RemoveData(accessor);
            }
        }

        private void DoModifications() {
            // apply modifications
            foreach (Tuple<int, ImmutableContainer<Data>> toApply in _modifications.Current) {
                // if we removed the data, then don't bother apply/dispatching modifications on it
                if (_data.Contains(toApply.Item1)) {
                    _data[toApply.Item1].Increment();

                    // TODO: make sure that visualization events are correctly copied when
                    //       reproducing data
                    _data[toApply.Item1].Current.DoUpdateVisualization();
                }
            }
            _modifications.Swap();
            _modifications.Current.Clear();
        }

        public void ApplyModifications() {
            DoModifications();

            ModificationNotifier.Reset();
            DataStateChangeNotifier.Reset();

            //Debug.Log(_frame++ + " WasAdded: " + WasAdded<TemporaryData>() + ", WasRemoved: " + WasRemoved<TemporaryData>());
        }

        public bool DataStateChangeUpdate() {
            // do removals
            {
                List<DataAccessor> removedStage2 = _toRemove.Previous;
                List<DataAccessor> removedStage1 = _toRemove.Current;
                _toRemove.Swap();

                for (int i = 0; i < removedStage1.Count; ++i) {
                    int id = removedStage1[i].Id;
                    _removed[id] = removedStage1[i];
                    // _removed[id] is removed from _removed in stage2
                }

                for (int i = 0; i < removedStage2.Count; ++i) {
                    int id = removedStage2[i].Id;
                    _removed.Remove(id);
                    _data.Remove(id);
                }
                removedStage2.Clear();
            }

            // do additions
            for (int i = 0; i < _toAdd.Count; ++i) {
                Data added = _toAdd[i];
                int id = DataFactory.GetId(added.GetType());

                _data[id] = new ImmutableContainer<Data>(added);

                // visualize the initial data
                added.DoUpdateVisualization();
            }
            _toAdd.Clear();

            // do we still have things to remove?
            return _toRemove.Previous.Count > 0;
        }

        /// <summary>
        /// Attempts to retrieve a data instance with the given DataAccessor from the list of added
        /// data.
        /// </summary>
        /// <param name="accessor">The DataAccessor to lookup</param>
        /// <returns>A data instance, or null if it cannot be found</returns>
        private Data GetAddedData(DataAccessor accessor) {
            int id = accessor.Id;
            // TODO: optimize this so we don't have to search through all added data... though
            // this should actually be pretty quick
            for (int i = 0; i < _toAdd.Count; ++i) {
                int addedId = DataFactory.GetId(_toAdd[i].GetType());
                if (addedId == id) {
                    return _toAdd[i];
                }
            }

            return null;
        }

        public Data Modify(DataAccessor accessor, bool force = false) {
            var id = accessor.Id;

            if (ContainsData(accessor) == false) {
                Data added = GetAddedData(accessor);
                if (added != null) {
                    return added;
                }

                throw new NoSuchDataException(this, DataFactory.GetTypeFromAccessor(accessor));
            }

            if (_modifications.Current.Contains(id) && !force && _data[id].Current.SupportsConcurrentModifications == false) {
                throw new RemodifiedDataException(this, DataFactory.GetTypeFromAccessor(accessor));
            }

            _modifications.Current[id] = _data[id];

            ModificationNotifier.Notify();

            return _data[id].Modifying;
        }

        public Data Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, DataFactory.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Current;
        }

        public Data Previous(DataAccessor accessor) {
            var id = accessor.Id;

            if (_data.Contains(id) == false) {
                throw new NoSuchDataException(this, DataFactory.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Previous;
        }

        public bool ContainsData(DataAccessor accessor) {
            int id = accessor.Id;
            return _data.Contains(id) && _removed.Contains(id) == false;
        }

        public bool WasModified(DataAccessor accessor) {
            return _modifications.Previous.Contains(accessor.Id);
        }
    }
}