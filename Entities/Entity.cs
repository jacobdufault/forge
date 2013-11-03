using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Neon.Entities {
    public class SerializedEntityData {
        public bool WasModified;
        public bool IsAdding;
        public bool IsRemoving;

        public Data Previous;
        public Data Current;

        public SerializedEntityData(bool wasModifying, bool isAdding, bool isRemoving, Data previous, Data current) {
            WasModified = wasModifying;
            IsAdding = isAdding;
            IsRemoving = isRemoving;
            Previous = previous;
            Current = current;
        }
    }

    public class Entity : IEntity {
        #region Enabled
        private volatile bool _enabled;
        bool IEntity.Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
            }
        }
        #endregion

        #region Unique ID
        /// <summary>
        /// Generates unique identifiers.
        /// </summary>
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        // thread safe because we never write
        private int _uniqueId;

        int IEntity.UniqueId {
            get { return _uniqueId; }
        }
        #endregion

        #region Metadata
        /// <summary>
        /// Used to retrieve keys for storing things in instance-specific metadata containers.
        /// </summary>
        public static MetadataRegistry MetadataRegistry = new MetadataRegistry();
        private MetadataContainer<object> _metadata = new MetadataContainer<object>();
        MetadataContainer<object> IEntity.Metadata {
            get {
                return _metadata;
            }
        }
        #endregion

        #region Event Processor
        private EventProcessor _eventProcessor;
        EventProcessor IEntity.EventProcessor {
            get { return _eventProcessor; }
        }
        #endregion

        /// <summary>
        /// Reconstructs an entity with the given unique id and the set of restored data instances.
        /// </summary>
        /// <remarks>
        /// Notice, however, that this function does *NOT* notify the EntityManager if a data instance has been restored which has a modification or a state change.
        /// </remarks>
        internal Entity(int uniqueId, List<SerializedEntityData> restoredData) {
            _uniqueId = uniqueId;
            _idGenerator.Consume(uniqueId);
            _enabled = true;
            _eventProcessor = new EventProcessor();

            DataStateChangeNotifier = new Notifier<Entity>(this);
            ModificationNotifier = new Notifier<Entity>(this);

            foreach (var data in restoredData) {
                if (data.IsAdding) {
                    _toAdd.Add(data.Current);
                }

                else {
                    int id = DataAccessorFactory.GetId(data.Current.GetType());
                    _data[id] = new ImmutableContainer<Data>(data.Previous, data.Current, data.Current.Duplicate());

                    if (data.IsRemoving) {
                        _toRemove.Previous.Add(new DataAccessor(id));
                    }
                }
            }
        }

        public Entity() {
            _uniqueId = _idGenerator.Next();
            _enabled = true; // default to being enabled
            _eventProcessor = new EventProcessor();

            DataStateChangeNotifier = new Notifier<Entity>(this);
            ModificationNotifier = new Notifier<Entity>(this);
        }

        #region Private EntityManager only API
        public Notifier<Entity> DataStateChangeNotifier;
        public Notifier<Entity> ModificationNotifier;

        public EntityManager EntityManager;

        /// <summary>
        /// Removes all data instances from the Entity.
        /// </summary>
        internal void RemoveAllData() {
            // TODO: potentially optimize this method
            foreach (var tuple in _data) {
                DataAccessor accessor = new DataAccessor(tuple.Item2.Current.GetType());
                ((IEntity)this).RemoveData(accessor);
            }
        }

        private void DoModifications() {
            _modifiedLastFrame.Clear();

            // apply modifications
            _concurrentModifications.IterateAndClear(dataAccessor => {
                int id = dataAccessor.Id;

                // if we removed the data, then don't bother apply/dispatching modifications on it
                if (_data.Contains(id)) {
                    _data[id].MotificationActivation.Reset();
                    _data[id].Increment();

                    _modifiedLastFrame[id] = _data[id];

                    // TODO: make sure that visualization events are correctly copied when
                    //       reproducing data
                    _data[id].Current.DoUpdateVisualization();
                }
            });
        }

        internal void ApplyModifications() {
            DoModifications();

            ModificationNotifier.Reset();

            // We do *not* reset the DataStateChangeNotifier here.
            // This may seem unusual, but the reason is for efficiency. This implementation is
            // tightly coupled with the EntityManager. The EntityManager contains a list of Entities
            // with data state changes (that systems use to check to see if an entity needs to be
            // contained within it). Because data state changes run for multiple updates, that list
            // can contain an Entity even if a modification has been applied. Activating the
            // notifier inserts the Entity into the list. Reseting it effectively says that the
            // Entity is no longer in the data state change list. Therefore, the EntityManager knows
            // best when the entity is not in the list and is therefore responsible for reseting
            // the notifier.

            // DataStateChangeNotifier.Reset(); do not uncomment me; see above
        }


        /// <summary>
        /// Applies data state changes to the entity.
        /// </summary>
        /// <remarks>
        /// This function is not thread-safe; no other API calls can be made to the Entity while
        /// this function is being executed.
        /// </remarks>
        /// <returns>If more data state change updates are needed</returns>
        internal void DataStateChangeUpdate() {
            // do removals
            {
                List<DataAccessor> removedStage2 = _toRemove.Previous;
                List<DataAccessor> removedStage1 = _toRemove.Current;
                _toRemove.Swap();

                for (int i = 0; i < removedStage1.Count; ++i) {
                    int id = removedStage1[i].Id;
                    _removed[id] = removedStage1[i];
                    // _removed[id] is removed from _removed in stage2

                    ((this as IEntity)).EventProcessor.Submit(new RemovedDataEvent(_data[id].Current.GetType()));
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
                ((this as IEntity)).EventProcessor.Submit(new AddedDataEvent(added.GetType()));

                int id = DataAccessorFactory.GetId(added.GetType());
                _data[id] = new ImmutableContainer<Data>(added.Duplicate(), added.Duplicate(), added.Duplicate());

                // visualize the initial data
                added.DoUpdateVisualization();
            }
            _toAdd.Clear();
        }

        internal bool NeedsMoreDataStateChangeUpdates() {
            // do we still have things to remove or to add?
            return _toRemove.Previous.Count > 0 || _toRemove.Current.Count > 0 || _toAdd.Count > 0;
        }
        #endregion

        void IEntity.Destroy() {
            // EntityManager.RemoveEntity is synchronized
            EntityManager.RemoveEntity(this);
        }

        #region Instance data
        /// <summary>
        /// The data contained within the Entity. One item in the tuple is the current state and one
        /// item is the next state.
        /// </summary>
        /// <remarks>
        /// Only the entity manager calls entity APIs that write to this; it is single-threaded only.
        /// </remarks>
        private IterableSparseArray<ImmutableContainer<Data>> _data = new IterableSparseArray<ImmutableContainer<Data>>();

        /// <summary>
        /// Data that has been modified this frame and needs to be pushed out
        /// </summary>
        private SparseArray<ImmutableContainer<Data>> _modifiedLastFrame = new SparseArray<ImmutableContainer<Data>>();
        private ConcurrentWriterBag<DataAccessor> _concurrentModifications = new ConcurrentWriterBag<DataAccessor>();

        /// <summary>
        /// Items that are pending removal in the next update call
        /// </summary>

        private List<Data> _toAdd = new List<Data>();

        private SwappableItem<List<DataAccessor>> _toRemove = new SwappableItem<List<DataAccessor>>(new List<DataAccessor>(), new List<DataAccessor>());
        private SparseArray<DataAccessor> _removed = new SparseArray<DataAccessor>();
        #endregion

        #region Helper Methods
        /// <summary>
        /// Attempts to retrieve a data instance with the given DataAccessor from the list of added
        /// data.
        /// </summary>
        /// <param name="accessor">The DataAccessor to lookup</param>
        /// <returns>A data instance, or null if it cannot be found</returns>
        private Data GetAddedData_unlocked(DataAccessor accessor) {
            int id = accessor.Id;
            // TODO: optimize this so we don't have to search through all added data... though
            // this should actually be pretty quick
            for (int i = 0; i < _toAdd.Count; ++i) {
                int addedId = DataAccessorFactory.GetId(_toAdd[i].GetType());
                if (addedId == id) {
                    return _toAdd[i];
                }
            }

            return null;
        }
        #endregion

        Data IEntity.AddOrModify(DataAccessor accessor) {
            if (((IEntity)this).ContainsData(accessor) == false) {
                lock (_toAdd) {
                    Data added = GetAddedData_unlocked(accessor);
                    if (added == null) {
                        added = AddData_unlocked(accessor);
                    }

                    return added;
                }
            }

            return ((IEntity)this).Modify(accessor);
        }

        ICollection<Data> IEntity.SelectCurrentData(Predicate<Data> filter, ICollection<Data> storage) {
            if (storage == null) {
                storage = new List<Data>();
            }

            foreach (var tuple in _data) {
                Data data = tuple.Item2.Current;
                if (filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        #region AddData
        private Data AddData_unlocked(DataAccessor accessor) {
            // ensure that we have not already added a data of this type
            if (GetAddedData_unlocked(accessor) != null) {
                throw new AlreadyAddedDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            // add our data
            Data data = DataAllocator.Allocate(accessor);
            _toAdd.Add(data);

            // initialize data outside of lock
            data.Entity = this;

            // notify the entity manager
            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();

            // populate the data instance from the prefab / etc
            DataAllocator.NotifyAllocated(accessor, this, data);

            // return the new instance
            return data;
        }

        Data IEntity.AddData(DataAccessor accessor) {
            lock (_toAdd) {
                return AddData_unlocked(accessor);
            }
        }
        #endregion

        #region RemoveData
        void IEntity.RemoveData(DataAccessor accessor) {
            lock (_toRemove.Current) {
                _toRemove.Current.Add(accessor);
            }

            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();
        }
        #endregion

        #region Modify
        Data IEntity.Modify(DataAccessor accessor, bool force) {
            var id = accessor.Id;

            if (((IEntity)this).ContainsData(accessor) == false) {
                Data added = GetAddedData_unlocked(accessor);
                if (added != null) {
                    return added;
                }

                throw new NoSuchDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            if (_data[id].MotificationActivation.TryActivate()) {
                _concurrentModifications.Add(accessor);
                ModificationNotifier.Notify();
            }
            else if (!force && _data[id].Current.SupportsConcurrentModifications == false) {
                throw new RemodifiedDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            return _data[id].Modifying;
        }
        #endregion

        Data IEntity.Current(DataAccessor accessor) {
            if (_data.Contains(accessor.Id) == false) {
                throw new NoSuchDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Current;
        }

        Data IEntity.Previous(DataAccessor accessor) {
            if (((IEntity)this).ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Previous;
        }

        #region ContainsData
        bool IEntity.ContainsData(DataAccessor accessor) {
            int id = accessor.Id;
            return _data.Contains(id) && _removed.Contains(id) == false;
        }
        #endregion

        bool IEntity.WasModified(DataAccessor accessor) {
            return _modifiedLastFrame.Contains(accessor.Id);
        }

        public override string ToString() {
            return string.Format("Entity [uid={0}]", _uniqueId);
        }
    }
}