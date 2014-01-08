// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Forge.Collections;
using Forge.Entities.Implementation.Content;
using Forge.Entities.Implementation.Shared;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Forge.Entities.Implementation.Runtime {
    [JsonConverter(typeof(QueryableEntityConverter))]
    internal sealed class RuntimeEntity : IEntity, IDisposable {
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
        // thread safe because we never write
        private int _uniqueId;

        public int UniqueId {
            get { return _uniqueId; }
        }
        #endregion

        private IEventDispatcher _eventDispatcher;

        #region Metadata
        public EntityManagerMetadata Metadata = new EntityManagerMetadata();
        #endregion

        public RuntimeEntity() {
            DataStateChangeNotifier = new Notifier<RuntimeEntity>(this);
            ModificationNotifier = new Notifier<RuntimeEntity>(this);
        }

        private RuntimeEntity(int uniqueId, IEventDispatcher eventDispatcher, string prettyName)
            : this() {
            _eventDispatcher = eventDispatcher;
            _uniqueId = uniqueId;
            PrettyName = prettyName ?? "";
        }

        public RuntimeEntity(int uniqueId, ITemplate template, IEventDispatcher eventDispatcher)
            : this(uniqueId, eventDispatcher, "") {
            foreach (DataAccessor accessor in template.SelectData()) {
                Data.IData data = template.Current(accessor);
                AddData_unlocked(accessor, data.Duplicate());
            }
        }

        public void Initialize(ContentEntitySerializationFormat format, IEventDispatcher eventDispatcher) {
            _eventDispatcher = eventDispatcher;
            _uniqueId = format.UniqueId;
            PrettyName = format.PrettyName ?? "";

            foreach (ContentEntity.DataInstance data in format.Data) {
                DataAccessor accessor = new DataAccessor(data.CurrentData);

                if (data.WasAdded) {
                    _toAddStage1.Add(data.CurrentData);
                }

                else {
                    if (data.CurrentData is Data.NonVersioned) {
                        _data[accessor.Id] = new NonVersionedDataContainer(
                            (Data.NonVersioned)data.CurrentData);
                    }

                    else {
                        _data[accessor.Id] = new VersionedDataContainer(
                            (Data.IVersioned)data.PreviousData,
                            (Data.IVersioned)data.CurrentData,
                            (Data.IVersioned)data.CurrentData.Duplicate());
                    }

                    // There is going to be an ApplyModification call before systems actually view
                    // this Entity instance. With that in mind, we can treat our data initialization
                    // as if if were operating on the previous frame.

                    if (data.WasModified) {
                        // This is kind of an ugly hack, because to get the correct Previous/Current
                        // data we need to move Previous into Current so that Previous will reflect
                        // the true Previous value, not the Current one.

                        // Internal signal that a modification is going to take place
                        Modify(accessor);

                        if (data.CurrentData is Data.IVersioned) {
                            // Move Previous into Current, so that after the ApplyModification we
                            // have the correct data values
                            var container = (VersionedDataContainer)_data[accessor.Id];
                            container.Current.CopyFrom(container.Previous);
                        }
                    }

                    if (data.WasRemoved) {
                        RemoveData(accessor);
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
                if (_data.ContainsKey(id)) {
                    if (_data[id] is VersionedDataContainer) {
                        VersionedDataContainer container = (VersionedDataContainer)_data[id];
                        container.Increment();
                    }

                    _data[id].ModificationActivation.Reset();
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

                    _eventDispatcher.Submit(RemovedDataEvent.Create(this, DataAccessorFactory.GetTypeFromAccessor(new DataAccessor(id))));
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
                Data.IData added = _toAddStage1[i];
                int id = DataAccessorFactory.GetId(added.GetType());

                // make sure we do not readd the same data instance twice
                if (_data.ContainsKey(id)) {
                    exceptionToThrow = new AlreadyAddedDataException(this, new DataAccessor(added));
                    continue;
                }

                _addedLastFrame[id] = added;

                if (added is Data.IVersioned) {
                    Data.IVersioned versioned = (Data.IVersioned)added;

                    _data[id] = new VersionedDataContainer(versioned.Duplicate(), versioned.Duplicate(), versioned.Duplicate());
                }
                else {
                    Data.NonVersioned nonVersioned = (Data.NonVersioned)added;
                    _data[id] = new NonVersionedDataContainer(nonVersioned);
                }

                // visualize the initial data
                _eventDispatcher.Submit(AddedDataEvent.Create(this, added.GetType()));
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

            foreach (KeyValuePair<int, DataContainer> tuple in _data) {
                DataAccessor accessor = new DataAccessor(tuple.Key);

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
                    if (_data[accessor.Id] is VersionedDataContainer) {
                        VersionedDataContainer container = (VersionedDataContainer)_data[accessor.Id];
                        dataInstance.CurrentData = container.Modifying;
                        dataInstance.PreviousData = container.Current;
                    }
                    else {
                        NonVersionedDataContainer container = (NonVersionedDataContainer)_data[accessor.Id];
                        dataInstance.CurrentData = container.Data;
                    }

                    dataInstance.WasModified = true;
                }

                // we were not modified this frame, so don't perform a data swap
                else {
                    // do a data swap so our modified data is correct
                    if (_data[accessor.Id] is VersionedDataContainer) {
                        VersionedDataContainer container = (VersionedDataContainer)_data[accessor.Id];
                        dataInstance.CurrentData = container.Current;
                        dataInstance.PreviousData = container.Previous;
                    }
                    else {
                        NonVersionedDataContainer container = (NonVersionedDataContainer)_data[accessor.Id];
                        dataInstance.CurrentData = container.Data;
                    }

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
        private List<Data.IData> _toAddStage1 = new List<Data.IData>();
        private List<Data.IData> _toAddStage2 = new List<Data.IData>();
        private SparseArray<Data.IData> _addedLastFrame = new SparseArray<Data.IData>();

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
        private Data.IData GetAddedData_unlocked(DataAccessor accessor) {
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

        public Data.IData AddOrModify(DataAccessor accessor) {
            if (ContainsData(accessor) == false || WasRemoved(accessor)) {
                lock (_toAddStage1) {
                    Data.IData added = GetAddedData_unlocked(accessor);
                    if (added == null) {
                        added = AddData_unlocked(accessor);
                    }

                    return added;
                }
            }

            return Modify(accessor);
        }

        public ICollection<DataAccessor> SelectData(bool includeRemoved = false,
            Predicate<DataAccessor> filter = null, ICollection<DataAccessor> storage = null) {
            if (storage == null) {
                storage = new List<DataAccessor>();
            }

            foreach (var tuple in _data) {
                DataAccessor accessor = new DataAccessor(tuple.Key);
                if (includeRemoved == false && WasRemoved(accessor)) {
                    continue;
                }

                if (filter == null || filter(accessor)) {
                    storage.Add(accessor);
                }
            }

            return storage;
        }

        #region AddData
        private Data.IData AddData_unlocked(DataAccessor accessor, Data.IData instance = null) {
            // ensure that we have not already added a data of this type
            if (GetAddedData_unlocked(accessor) != null || ContainsData(accessor)) {
                throw new AlreadyAddedDataException(this, accessor);
            }

            // add our data
            Type dataType = DataAccessorFactory.GetTypeFromAccessor(accessor);
            if (instance == null) {
                instance = (Data.IData)Activator.CreateInstance(dataType);
            }
            _toAddStage1.Add(instance);

            // notify the entity manager
            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();

            // return the new instance
            return instance;
        }

        public Data.IData AddData(DataAccessor accessor) {
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
        public Data.IData Modify(DataAccessor accessor) {
            var id = accessor.Id;

            // if we don't contain the data, then we might have added in this frame, which means we
            // want to just return the added data -- otherwise, there is no data to modify
            if (ContainsData(accessor) == false) {
                Data.IData added = GetAddedData_unlocked(accessor);
                if (added != null) {
                    return added;
                }

                throw new NoSuchDataException(this, accessor);
            }

            // notify everyone that the data has been modified and see if we're the first ones to
            // modify the data
            if (_data[id].ModificationActivation.TryActivate()) {
                _concurrentModifications.Add(accessor);
                ModificationNotifier.Notify();
            }

            // someone else already modified the data
            else {
                // nonversioned data cannot be modified multiple times unless its concurrent
                if (_data[id] is NonVersionedDataContainer) {
                    if (((NonVersionedDataContainer)_data[id]).Data is Data.ConcurrentNonVersioned == false) {
                        throw new RemodifiedDataException(this, accessor);
                    }
                }

                // versioned data cannot be modified multiple times unless its concurrent
                if (_data[id] is VersionedDataContainer) {
                    if (((VersionedDataContainer)_data[id]).Current is Data.ConcurrentVersioned == false) {
                        throw new RemodifiedDataException(this, accessor);
                    }
                }
            }

            if (_data[id] is VersionedDataContainer) {
                return ((VersionedDataContainer)_data[id]).Modifying;
            }
            else {
                return ((NonVersionedDataContainer)_data[id]).Data;
            }
        }
        #endregion

        public Data.IData Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            if (_data[accessor.Id] is VersionedDataContainer) {
                var container = (VersionedDataContainer)_data[accessor.Id];
                return container.Current;
            }

            else {
                // nonversioned data cannot get current values after modification has occurred
                var container = (NonVersionedDataContainer)_data[accessor.Id];
                if (container.ModificationActivation.IsActivated) {
                    throw new InvalidOperationException("Cannot retrieve current value for modified non-versioned data");
                }
                return container.Data;
            }
        }

        public Data.IVersioned Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            if (_data[accessor.Id] is VersionedDataContainer == false) {
                throw new PreviousRequiresVersionedDataException(this, accessor);
            }

            return ((VersionedDataContainer)_data[accessor.Id]).Previous;
        }

        #region ContainsData
        public bool ContainsData(DataAccessor accessor) {
            return _data.ContainsKey(accessor.Id);
        }
        #endregion

        public bool WasModified(DataAccessor accessor) {
            return _modifiedLastFrame.ContainsKey(accessor.Id);
        }

        public bool WasAdded(DataAccessor accessor) {
            return _addedLastFrame.ContainsKey(accessor.Id);
        }

        public bool WasRemoved(DataAccessor accessor) {
            return _removedLastFrame.ContainsKey(accessor.Id);
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Entity [uid={0}, name={1}]", _uniqueId, PrettyName);
            }
            else {
                return string.Format("Entity [uid={0}]", _uniqueId);
            }
        }

        public void Dispose() {
            _concurrentModifications.Dispose();
        }
    }
}