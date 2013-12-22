using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Runtime {
    [JsonConverter(typeof(EntityConverter))]
    internal class RuntimeEntity : IEntity {
        #region Pretty Name
        /// <summary>
        /// The Entity's pretty name, used for debugging / printing purposes.
        /// </summary>
        /// <remarks>
        /// If the entity does not have a pretty name, then this value is set to an empty string.
        /// </remarks>
        public string PrettyName {
            get;
            set;
        }
        #endregion

        #region Unique ID
        /// <summary>
        /// Generates unique identifiers.
        /// </summary>
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        // thread safe because we never write
        private int _uniqueId;

        public int UniqueId {
            get { return _uniqueId; }
        }
        #endregion

        #region Metadata
        public EntityManagerMetadata Metadata = new EntityManagerMetadata();
        #endregion

        #region Event Processor
        IEventNotifier IQueryableEntity.EventNotifier {
            get { return EventNotifier; }
        }
        public EventNotifier EventNotifier {
            get;
            private set;
        }
        #endregion

        public RuntimeEntity() {
            EventNotifier = new EventNotifier();

            DataStateChangeNotifier = new Notifier<RuntimeEntity>(this);
            ModificationNotifier = new Notifier<RuntimeEntity>(this);
        }

        private RuntimeEntity(int uniqueId, string prettyName)
            : this() {
            _uniqueId = uniqueId;
            _idGenerator.Consume(_uniqueId);

            PrettyName = prettyName ?? "";
        }

        public RuntimeEntity(ITemplate template)
            : this(_idGenerator.Next(), "") {
            foreach (var data in template.SelectCurrentData()) {
                AddData_unlocked(new DataAccessor(data)).CopyFrom(data);
            }
        }

        public void Initialize(ContentEntitySerializationFormat format) {
            _uniqueId = format.UniqueId;
            PrettyName = format.PrettyName ?? "";

            foreach (ContentEntity.DataInstance data in format.Data) {
                DataAccessor accessor = new DataAccessor(data.CurrentData);

                if (data.WasAdded) {
                    _toAddStage1.Add(data.CurrentData);
                }

                else {
                    IData current = data.CurrentData;
                    IData previous = data.PreviousData;

                    _data[accessor.Id] = new DataContainer(previous, current, current.Duplicate());

                    // There is going to be an ApplyModification call before systems actually view
                    // this Entity instance. With that in mind, we can treat our data initialization
                    // as if if were operating on the previous frame.

                    if (data.WasModified) {
                        // This is kind of an ugly hack, because to get the correct Previous/Current
                        // data we need to move Previous into Current so that Previous will reflect
                        // the true Previous value, not the Current one.

                        // Internal signal that a modification is going to take place
                        ((IEntity)this).Modify(accessor);

                        // Move Previous into Current, so that after the ApplyModification we have
                        // the correct data values
                        _data[accessor.Id].Current.CopyFrom(_data[accessor.Id].Previous);
                    }

                    if (data.WasRemoved) {
                        ((IEntity)this).RemoveData(accessor);
                    }
                }
            }
        }

        #region Private EntityManager only API
        public Notifier<RuntimeEntity> DataStateChangeNotifier;
        public Notifier<RuntimeEntity> ModificationNotifier;

        public GameEngine GameEngine;

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
                }
            });
        }

        public void ApplyModifications() {
            DoModifications();

            ModificationNotifier.Reset();

            // We do *not* reset the DataStateChangeNotifier here. This may seem unusual, but the
            // reason is for efficiency. This implementation is tightly coupled with the
            // EntityManager. The EntityManager contains a list of Entities with data state changes
            // (that systems use to check to see if an entity needs to be contained within it).
            // Because data state changes run for multiple updates, that list can contain an Entity
            // even if a modification has been applied. Activating the notifier inserts the Entity
            // into the list. Reseting it effectively says that the Entity is no longer in the data
            // state change list. Therefore, the EntityManager knows best when the entity is not in
            // the list and is therefore responsible for reseting the notifier.

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
        public void DataStateChangeUpdate() {
            // do removals
            {
                for (int i = 0; i < _toRemoveStage1.Count; ++i) {
                    int id = _toRemoveStage1[i].Id;
                    _removedLastFrame[id] = _toRemoveStage1[i];
                    // _removedLastFrame[id] is removed in stage2

                    EventNotifier.Submit(new RemovedDataEvent(_data[id].Current.GetType()));
                }

                for (int i = 0; i < _toRemoveStage2.Count; ++i) {
                    int id = _toRemoveStage2[i].Id;
                    _removedLastFrame.Remove(id);
                    _data.Remove(id);
                }
                _toRemoveStage2.Clear();

                Utils.Swap(ref _toRemoveStage1, ref _toRemoveStage2);
            }

            // We don't throw an exception immediately. If we are throwing an exception, that means
            // that the Entity is in an invalid state (adding a bad data instance). However, the
            // only way to recover from that invalid state is by having this method terminate.
            // Hence, we only throw an exception at the end of the method.
            Exception exceptionToThrow = null;

            // do additions
            for (int i = 0; i < _toAddStage1.Count; ++i) {
                IData added = _toAddStage1[i];
                int id = DataAccessorFactory.GetId(added.GetType());

                // make sure we do not readd the same data instance twice
                if (_data.Contains(id)) {
                    exceptionToThrow = new AlreadyAddedDataException(this, new DataAccessor(added));
                    continue;
                }

                _addedLastFrame[id] = added;

                _data[id] = new DataContainer(added.Duplicate(), added.Duplicate(), added.Duplicate());

                // visualize the initial data
                EventNotifier.Submit(new AddedDataEvent(added.GetType()));
            }

            for (int i = 0; i < _toAddStage2.Count; ++i) {
                _addedLastFrame.Remove(new DataAccessor(_toAddStage2[i]).Id);
            }
            _toAddStage2.Clear();

            Utils.Swap(ref _toAddStage1, ref _toAddStage2);

            if (exceptionToThrow != null) {
                throw exceptionToThrow;
            }
        }

        internal bool NeedsMoreDataStateChangeUpdates() {
            // do we still have things to remove or to add?
            return _toRemoveStage1.Count > 0 || _toRemoveStage2.Count > 0 ||
                _toAddStage1.Count > 0 || _toAddStage2.Count > 0;
        }
        #endregion

        void IEntity.Destroy() {
            // EntityManager.RemoveEntity is synchronized
            GameEngine.RemoveEntity(this);
        }

        public ContentEntitySerializationFormat GetSerializedFormat() {
            List<ContentEntity.DataInstance> data = new List<ContentEntity.DataInstance>();

            // the data instances that have been modified in the current update
            HashSet<int> modifiedThisFrame = new HashSet<int>();
            {
                List<DataAccessor> modifications = _concurrentModifications.ToList();
                foreach (DataAccessor modification in modifications) {
                    modifiedThisFrame.Add(modification.Id);
                }
            }

            // the data instances that have been removed in the current update
            HashSet<int> removedThisFrame = new HashSet<int>();
            {
                foreach (DataAccessor item in _toRemoveStage1) {
                    removedThisFrame.Add(item.Id);
                }
            }

            foreach (var tuple in _data) {
                DataContainer container = tuple.Value;
                DataAccessor accessor = new DataAccessor(container.Current);

                // If the data was removed this frame, then next frame it won't exist anymore, so we
                // don't serialize it
                if (WasRemoved(accessor)) {
                    continue;
                }

                var dataInstance = new ContentEntity.DataInstance() {
                    // these items are never added this frame; if WasAdded is true now, it will be
                    // false next frame
                    WasAdded = false,

                    // the data *may* have been removed this frame, though
                    WasRemoved = removedThisFrame.Contains(accessor.Id)
                };

                // if we were modified this frame, then we have to do a data swap (and set
                // WasModified to true)
                if (modifiedThisFrame.Contains(accessor.Id)) {
                    // do a data swap so our modified data is correct
                    dataInstance.CurrentData = container.Modifying;
                    dataInstance.PreviousData = container.Current;

                    dataInstance.WasModified = true;
                }

                // we were not modified this frame, so don't perform a data swap
                else {
                    dataInstance.CurrentData = container.Current;
                    dataInstance.PreviousData = container.Previous;
                    dataInstance.WasModified = false;
                }

                data.Add(dataInstance);
            }

            foreach (var toAdd in _toAddStage1) {
                data.Add(new ContentEntity.DataInstance() {
                    CurrentData = toAdd,
                    PreviousData = toAdd,
                    WasAdded = true,

                    // added data is never modified
                    WasModified = false,

                    // added data also cannot be removed in the same frame it was added in
                    WasRemoved = false
                });
            }

            return new ContentEntitySerializationFormat() {
                PrettyName = PrettyName,
                UniqueId = UniqueId,
                Data = data
            };
        }

        #region Instance data
        /// <summary>
        /// The data contained within the Entity. One item in the tuple is the current state and one
        /// item is the next state.
        /// </summary>
        /// <remarks>
        /// Only the entity manager calls entity APIs that write to this; it is single-threaded
        /// only.
        /// </remarks>
        private SparseArray<DataContainer> _data = new SparseArray<DataContainer>();

        /// <summary>
        /// Data that has been modified this frame and needs to be pushed out
        /// </summary>
        private SparseArray<DataContainer> _modifiedLastFrame = new SparseArray<DataContainer>();
        private ConcurrentWriterBag<DataAccessor> _concurrentModifications = new ConcurrentWriterBag<DataAccessor>();

        /// <summary>
        /// Items that are pending addition in the next update call
        /// </summary>
        private List<IData> _toAddStage1 = new List<IData>();
        private List<IData> _toAddStage2 = new List<IData>();
        private SparseArray<IData> _addedLastFrame = new SparseArray<IData>();

        /// <summary>
        /// Items that are going to be removed. Removal is a two stage process, because after a data
        /// item has been removed the final state of the data can be queried in the next update.
        /// </summary>
        private List<DataAccessor> _toRemoveStage1 = new List<DataAccessor>();

        /// <summary>
        /// Data that was removed not in the update that is currently executing, but in the update
        /// that previously executed.
        /// </summary>
        private List<DataAccessor> _toRemoveStage2 = new List<DataAccessor>();

        /// <summary>
        /// Data that was removed during the last frame (update). This is identical to
        /// _toRemoveStage1, except that it provides a fast way to check to see if a data item has
        /// been removed.
        /// </summary>
        private SparseArray<DataAccessor> _removedLastFrame = new SparseArray<DataAccessor>();
        #endregion

        #region Helper Methods
        /// <summary>
        /// Attempts to retrieve a data instance with the given DataAccessor from the list of added
        /// data.
        /// </summary>
        /// <param name="accessor">The DataAccessor to lookup</param>
        /// <returns>A data instance, or null if it cannot be found</returns>
        private IData GetAddedData_unlocked(DataAccessor accessor) {
            int id = accessor.Id;
            // TODO: optimize this so we don't have to search through all added data... though
            // this should actually be pretty quick
            for (int i = 0; i < _toAddStage1.Count; ++i) {
                int addedId = DataAccessorFactory.GetId(_toAddStage1[i].GetType());
                if (addedId == id) {
                    return _toAddStage1[i];
                }
            }

            return null;
        }
        #endregion

        public IData AddOrModify(DataAccessor accessor) {
            if (((IEntity)this).ContainsData(accessor) == false) {
                lock (_toAddStage1) {
                    IData added = GetAddedData_unlocked(accessor);
                    if (added == null) {
                        added = AddData_unlocked(accessor);
                    }

                    return added;
                }
            }

            return ((IEntity)this).Modify(accessor);
        }

        public ICollection<IData> SelectCurrentData(Predicate<IData> filter = null,
            ICollection<IData> storage = null) {
            if (storage == null) {
                storage = new List<IData>();
            }

            foreach (var tuple in _data) {
                IData data = tuple.Value.Current;
                if (filter == null || filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        #region AddData
        private IData AddData_unlocked(DataAccessor accessor) {
            // ensure that we have not already added a data of this type
            if (GetAddedData_unlocked(accessor) != null) {
                throw new AlreadyAddedDataException(this, accessor);
            }

            // add our data
            Type dataType = DataAccessorFactory.GetTypeFromAccessor(accessor);
            IData data = (IData)Activator.CreateInstance(dataType);
            _toAddStage1.Add(data);

            // notify the entity manager
            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();

            // return the new instance
            return data;
        }

        public IData AddData(DataAccessor accessor) {
            lock (_toAddStage1) {
                return AddData_unlocked(accessor);
            }
        }
        #endregion

        #region RemoveData
        public void RemoveData(DataAccessor accessor) {
            lock (_toRemoveStage1) {
                _toRemoveStage1.Add(accessor);
            }

            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();
        }
        #endregion

        #region Modify
        public IData Modify(DataAccessor accessor) {
            var id = accessor.Id;

            if (((IEntity)this).ContainsData(accessor) == false) {
                IData added = GetAddedData_unlocked(accessor);
                if (added != null) {
                    return added;
                }

                throw new NoSuchDataException(this, accessor);
            }

            if (_data[id].MotificationActivation.TryActivate()) {
                _concurrentModifications.Add(accessor);
                ModificationNotifier.Notify();
            }
            else if (_data[id].Current.SupportsConcurrentModifications == false) {
                throw new RemodifiedDataException(this, accessor);
            }

            return _data[id].Modifying;
        }
        #endregion

        public IData Current(DataAccessor accessor) {
            if (_data.Contains(accessor.Id) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id].Current;
        }

        public IData Previous(DataAccessor accessor) {
            if (((IEntity)this).ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id].Previous;
        }

        #region ContainsData
        public bool ContainsData(DataAccessor accessor) {
            // We contain data if a) data contains it and b) it was not removed in the last frame
            int id = accessor.Id;
            return _data.Contains(id) && _removedLastFrame.Contains(id) == false;
        }
        #endregion

        public bool WasModified(DataAccessor accessor) {
            return _modifiedLastFrame.Contains(accessor.Id);
        }

        public bool WasAdded(DataAccessor accessor) {
            return _addedLastFrame.Contains(accessor.Id);
        }

        public bool WasRemoved(DataAccessor accessor) {
            return _removedLastFrame.Contains(accessor.Id);
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Entity [uid={0}, name={1}]", _uniqueId, PrettyName);
            }
            else {
                return string.Format("Entity [uid={0}]", _uniqueId);
            }
        }
    }
}