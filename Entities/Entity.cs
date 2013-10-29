using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Entities {
    /// <summary>
    /// Exception thrown when a data type is added to an entity, but the entity already contains an
    /// instance of said data type.
    /// </summary>
    [Serializable]
    public class AlreadyAddedDataException : Exception {
        /// <summary>
        /// Creates the exception with the given context and data type.
        /// </summary>
        /// <param name="context">The entity that triggered the exception.</param>
        /// <param name="type">The data type that was already added.</param>
        internal AlreadyAddedDataException(IEntity context, Type type)
            : base("The entity already has a data instance for type=" + type + " in " + context) {
        }
    }

    /// <summary>
    /// Exception thrown when data is attempted to be retrieved from an Entity, but the entity does
    /// not contain an instance of said data type.
    /// </summary>
    [Serializable]
    public class NoSuchDataException : Exception {
        /// <summary>
        /// Creates the exception with the given context and data type.
        /// </summary>
        /// <param name="context">The entity that triggered the exception.</param>
        /// <param name="type">The data type that the entity lacks.</param>
        internal NoSuchDataException(IEntity context, Type type)
            : base("No such data for type=" + type + " in " + context) {
        }
    }

    /// <summary>
    /// An exception that is thrown when a data instance has been modified more than once in an
    /// update loop, but that data is not allowed to be concurrently modified.
    /// </summary>
    [Serializable]
    public class RemodifiedDataException : Exception {
        /// <summary>
        /// Creates the exception with the given context and data type.
        /// </summary>
        /// <param name="context">The entity that triggered the exception.</param>
        /// <param name="type">The data type that was concurrently modified.</param>
        internal RemodifiedDataException(IEntity context, Type type)
            : base("Already modified data for type=" + type + " in " + context) {
        }
    }

    /// <summary>
    /// Helper methods built on top of the core IEntity API.
    /// </summary>
    public static class IEntityExtensions {
        /// <summary>
        /// Adds the given data type, or modifies an instance of it.
        /// </summary>
        /// <remarks>
        /// This is a helper method that captures a common pattern.
        /// </remarks>
        /// <typeparam name="T">The type of data modified</typeparam>
        /// <returns>A modifiable instance of data of type T</returns>
        public static T AddOrModify<T>(this IEntity entity) where T : Data {
            DataAccessor accessor = DataMap<T>.Accessor;

            if (entity.ContainsData(accessor) == false) {
                return (T)entity.AddData(accessor);
            }
            return (T)entity.Modify(accessor);
        }

        public static T AddData<T>(this IEntity entity) where T : Data {
            return (T)entity.AddData(DataMap<T>.Accessor);
        }

        public static void RemoveData<T>(this IEntity entity) where T : Data {
            entity.RemoveData(DataMap<T>.Accessor);
        }

        public static T Modify<T>(this IEntity entity, bool force = false) where T : Data {
            return (T)entity.Modify(DataMap<T>.Accessor, force);
        }

        public static T Current<T>(this IEntity entity) where T : Data {
            return (T)entity.Current(DataMap<T>.Accessor);
        }

        public static T Previous<T>(this IEntity entity) where T : Data {
            return (T)entity.Previous(DataMap<T>.Accessor);
        }

        public static bool ContainsData<T>(this IEntity entity) where T : Data {
            return entity.ContainsData(DataMap<T>.Accessor);
        }

        public static bool WasModified<T>(this IEntity entity) where T : Data {
            return entity.WasModified(DataMap<T>.Accessor);
        }

    }

    /// <summary>
    /// An Entity contains some data.
    /// </summary>
    public interface IEntity {
        /// <summary>
        /// Destroys the entity. The entity is not destroyed immediately, but instead at the end of
        /// the next update loop. Systems will get a chance to process the destruction of the
        /// entity.
        /// </summary>
        void Destroy();

        [Obsolete("Use EventProcessor")]
        event Action OnShow;

        [Obsolete("Use EventProcessor")]
        event Action OnHide;

        [Obsolete("Use EventProcessor")]
        event Action OnRemoved;

        /// <summary>
        /// Gets the event processor that Systems use to notify the external world of interesting
        /// events.
        /// </summary>
        EventProcessor EventProcessor {
            get;
        }

        /// <summary>
        /// Add a Data instance of with the given accessor to the Entity.
        /// </summary>
        /// <remarks>
        /// The aux allocators will be called in this method, giving them a chance to populate the
        /// data with any necessary information.
        /// </remarks>
        /// <param name="accessor"></param>
        /// <returns>The data instance that can be used to initialize the data</returns>
        Data AddData(DataAccessor accessor);

        /// <summary>
        /// Removes the given data type from the entity.
        /// </summary>
        /// <remarks>
        /// The data instance is not removed in this frame, but in the next one. In the next frame,
        /// Previous[T] and Modify[T] will both throw NoSuchData exceptions, but Current[T] will
        /// return the current data instance.
        /// </remarks>
        /// <typeparam name="T">The type of data to remove</typeparam>
        // TODO: add test for Remove and Modify in the same frame
        void RemoveData(DataAccessor accessor);

        /// <summary>
        /// If Enabled is set to false, then the Entity will not be processed in any Update or
        /// StructuredInput based systems. However, modification ripples are still applied to the
        /// Entity until it has no more modifications.
        /// </summary>
        bool Enabled {
            get;
            set;
        }

        /// <summary>
        /// Modify the given data instance. The current and previous values are still accessible.
        /// Please note that a data instance can only be modified once; an exception is thrown if
        /// one instance is modified multiple times.
        /// </summary>
        /// <param name="force">If the modification should be forced; ie, if there is already a
        /// modification then it will be overwritten. This should *NEVER* be used in systems or
        /// general client code; it is available for inspector GUI changes.</param>
        Data Modify(DataAccessor accessor, bool force = false);

        /// <summary>
        /// Gets the current data value for the given type.
        /// </summary>
        Data Current(DataAccessor accessor);

        /// <summary>
        /// Gets the previous data value for the data type.
        /// </summary>
        Data Previous(DataAccessor accessor);

        /// <summary>
        /// Checks to see if this Entity contains the given type of data and if that data can be
        /// modified.
        /// </summary>
        /// <remarks>
        /// Interestingly, if the data has been removed, ContainsData[T] will return false but
        /// Current[T] will return an instance (though Previous[T] and Modify[T] will both throw
        /// exceptions) .
        /// </remarks>
        bool ContainsData(DataAccessor accessor);

        /// <summary>
        /// Returns if the Entity was modified in the previous update.
        /// </summary>
        bool WasModified(DataAccessor accessor);

        /// <summary>
        /// Metadata container that allows arbitrary data to be stored within the Entity.
        /// </summary>
        MetadataContainer<object> Metadata {
            get;
        }

        /// <summary>
        /// The unique id for the entity
        /// </summary>
        int UniqueId {
            get;
        }
    }

    public class Entity : IEntity {
        public bool Enabled {
            get;
            set;
        }

        public T AddData<T>() where T : Data {
            return (T)AddData(DataMap<T>.Accessor);
        }

        public Data AddData(DataAccessor accessor) {
            // TODO: ensure that we handle when we have removed the data

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

        private static int _nextId;
        private int _uid;

        public int UniqueId {
            get { return _uid; }
        }

        public EventProcessor EventProcessor {
            get;
            private set;
        }

        protected internal Entity() {
            _uid = Interlocked.Increment(ref _nextId);
            Enabled = true; // default to being enabled

            EventProcessor = new EventProcessor();

            DataStateChangeNotifier = new Notifier<Entity>(this);
            ModificationNotifier = new Notifier<Entity>(this);
        }

        public override string ToString() {
            return string.Format("Entity [uid={0}]", _uid);
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

        /// <summary>
        /// Called *ONLY* be simulation components so that they remove themselves from the entity
        /// </summary>
        public void RemoveData<T>() where T : Data {
            RemoveData(DataMap<T>.Accessor);
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

        public Data[] GetAllData<T>() where T : Data {
            return GetAllData(DataMap<T>.Accessor);
        }

        public Data[] GetAllData(DataAccessor accessor) {
            return _data[accessor.Id].Items;
        }

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