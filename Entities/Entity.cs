using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neon.Entities {
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

        protected internal Entity() {
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
        /// Invokes OnHide(). This must be called on the Unity thread.
        /// </summary>
        public void Hide() {
            if (_onHide != null) {
                _onHide();
            }
        }
        /// <summary>
        /// Invokes OnShow(). This must be called on the Unity thread.
        /// </summary>
        public void Show() {
            if (_onShow != null) {
                _onShow();
            }
        }
        /// <summary>
        /// Invokes OnRemoved(). This must be called on the Unity thread.
        /// </summary>
        public void RemovedFromEntityManager() {
            if (_onRemoved != null) {
                _onRemoved();
            }
        }

        /// <summary>
        /// Removes all data instances from the Entity.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)] // TODO: shouldn't need a lock
        public void RemoveAllData() {
            // TODO: potentially optimize this method
            foreach (var tuple in _data) {
                DataAccessor accessor = new DataAccessor(tuple.Item2.Current.GetType());
                RemoveData_unlocked(accessor);
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

        [MethodImpl(MethodImplOptions.Synchronized)] // TODO: shouldn't need a lock
        public void ApplyModifications() {
            DoModifications();

            ModificationNotifier.Reset();
            DataStateChangeNotifier.Reset();
        }


        /// <summary>
        /// Must be invoked on the Unity thread.
        /// </summary>
        /// <returns>If more data state change updates are needed</returns>
        [MethodImpl(MethodImplOptions.Synchronized)] // TODO: shouldn't need a lock
        public void DataStateChangeUpdate() {
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
                int id = DataAccessorFactory.GetId(added.GetType());

                _data[id] = new ImmutableContainer<Data>(added);

                // visualize the initial data
                added.DoUpdateVisualization();
            }
            _toAdd.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)] // TODO: shouldn't need a lock
        public bool NeedsMoreDataStateChangeUpdates() {
            // do we still have things to remove?
            return _toRemove.Previous.Count > 0;
        }
        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        void IEntity.Destroy() {
            EntityManager.RemoveEntity(this);
        }

        #region Events
        private Action _onShow;
        event Action IEntity.OnShow {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add { Delegate.Combine(_onShow, value); }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove { Delegate.Remove(_onShow, value); }
        }

        private Action _onHide;
        event Action IEntity.OnHide {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add { Delegate.Combine(_onHide, value); }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove { Delegate.Remove(_onHide, value); }
        }

        private Action _onRemoved;
        event Action IEntity.OnRemoved {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add { Delegate.Combine(_onRemoved, value); }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove { Delegate.Remove(_onRemoved, value); }
        }
        #endregion

        #region Instance data
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        Data IEntity.AddOrModify(DataAccessor accessor) {
            if (ContainsData_unlocked(accessor) == false) {
                Data added = GetAddedData_unlocked(accessor);
                if (added == null) {
                    return AddData_unlocked(accessor);
                }
            }

            return Modify_unlocked(accessor);
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
        [MethodImpl(MethodImplOptions.Synchronized)]
        Data IEntity.AddData(DataAccessor accessor) {
            return AddData_unlocked(accessor);
        }
        #endregion

        #region RemoveData
        private void RemoveData_unlocked(DataAccessor accessor) {
            _toRemove.Current.Add(accessor);

            ModificationNotifier.Notify();
            DataStateChangeNotifier.Notify();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        void IEntity.RemoveData(DataAccessor accessor) {
            RemoveData_unlocked(accessor);
        }
        #endregion

        #region Modify
        private Data Modify_unlocked(DataAccessor accessor, bool force = false) {
            var id = accessor.Id;

            if (ContainsData_unlocked(accessor) == false) {
                Data added = GetAddedData_unlocked(accessor);
                if (added != null) {
                    return added;
                }

                throw new NoSuchDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            if (_modifications.Current.Contains(id) && !force && _data[id].Current.SupportsConcurrentModifications == false) {
                throw new RemodifiedDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            _modifications.Current[id] = _data[id];

            ModificationNotifier.Notify();

            return _data[id].Modifying;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        Data IEntity.Modify(DataAccessor accessor, bool force) {
            return Modify_unlocked(accessor, force);
        }
        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        Data IEntity.Current(DataAccessor accessor) {
            if (_data.Contains(accessor.Id) == false) { 
                throw new NoSuchDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Current;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        Data IEntity.Previous(DataAccessor accessor) {
            if (ContainsData_unlocked(accessor) == false) {
                throw new NoSuchDataException(this, DataAccessorFactory.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Previous;
        }

        #region ContainsData
        private bool ContainsData_unlocked(DataAccessor accessor) {
            int id = accessor.Id;
            return _data.Contains(id) && _removed.Contains(id) == false;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        bool IEntity.ContainsData(DataAccessor accessor) {
            return ContainsData_unlocked(accessor);
        }
        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        bool IEntity.WasModified(DataAccessor accessor) {
            return _modifications.Previous.Contains(accessor.Id);
        }

        public override string ToString() {
            return string.Format("Entity [uid={0}]", _uniqueId);
        }
    }
}