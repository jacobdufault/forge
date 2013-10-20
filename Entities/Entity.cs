using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Neon.Utility;
using Neon.Collections;

namespace Neon.Entities {
    [Serializable]
    public class NoSuchDataException : Exception {
        public NoSuchDataException(Type type)
            : base("No such data for type=" + type) {
        }
    }

    [Serializable]
    public class RemodifiedDataException : Exception {
        public RemodifiedDataException(Type type)
            : base("Already modified data for type=" + type) {
        }
    }

    public interface IEntity {
        /// <summary>
        /// Destroys the entity.
        /// </summary>
        void Destroy();

        event Action OnShow;
        event Action OnHide;
        event Action OnRemoved;

        T AddData<T>() where T : Data;

        /// <summary>
        /// Add a Data instance of with the given accessor to the Entity.
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns></returns>
        Data AddData(DataAccessor accessor);


        //void RemoveData<T>() where T : Data;
        //void RemoveData(DataAccessor accessor);

        Data[] GetAllData<T>() where T : Data;
        Data[] GetAllData(DataAccessor accessor);

        /// <summary>
        /// Returns the data instances that pass the predicate. The data checked are all current instances
        /// inside of the Entity.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the data</param>
        /// <returns>All data instances which pass the predicate</returns>
        IEnumerable<Data> SelectData(Predicate<Data> predicate);

        /// <summary>
        /// If Enabled is set to false, then the Entity will not be processed in any Update
        /// or StructuredInput based systems. However, modification ripples are still
        /// applied to the Entity until it has no more modifications.
        /// </summary>
        bool Enabled {
            get;
            set;
        }

        /// <summary>
        /// Modify the given data instance. The current and previous values are still accessible.
        /// Please note that a data instance can only be modified once; an exception is thrown
        /// if one instance is modified multiple times.
        /// </summary>
        /// <param name="force">
        /// If the modification should be forced; ie, if there is already a modification then it
        /// will be overwritten. This should *NEVER* be used in systems or general client code; it
        /// is available for inspector GUI changes.
        /// </param>
        T Modify<T>() where T : Data;
        T Modify<T>(bool force) where T : Data;
        Data Modify(DataAccessor accessor);
        Data Modify(DataAccessor accessor, bool force);

        /// <summary>
        /// Gets the current data value for the given type.
        /// </summary>
        T Current<T>() where T : Data;
        Data Current(DataAccessor accessor);

        /// <summary>
        /// Gets the previous data value for the data type.
        /// </summary>
        T Previous<T>() where T : Data;
        Data Previous(DataAccessor accessor);

        /// <summary>
        /// Checks to see if this Entity contains the given type of data. If this method returns
        /// true, then the data can be modified. Please note, however, that Modify() may still
        /// throw a RemodifiedDataException.
        /// </summary>
        bool ContainsModifableData<T>() where T : Data;
        bool ContainsModifableData(DataAccessor accessor);

        /// <summary>
        /// Checks to see if this Entity contains the given type of data. Note that this
        /// method gives no promises about if the data can be modified.
        /// </summary>
        bool ContainsData<T>() where T : Data;
        bool ContainsData(DataAccessor accessor);

        /// <summary>
        /// Returns if the Entity was modified in the previous update.
        /// </summary>
        bool WasModified<T>() where T : Data;
        bool WasModified(DataAccessor accessor);

        /// <summary>
        /// Returns if the given data was added to the entity in the previous update.
        /// </summary>
        bool WasAdded<T>() where T : Data;
        bool WasAdded(DataAccessor accessor);

        /// <summary>
        /// Returns if the given data was removed from the entity in the previous update.
        /// </summary>
        /// <remarks>
        /// The removed data can be accessed via GetPrevious. Please note, however, that GetCurrent and
        /// Modify will throw NoSuchDataExceptions if they are also called with the same DataAccessor.
        /// </remarks>
        bool WasRemoved<T>() where T : Data;
        bool WasRemoved(DataAccessor accessor);

        /// <summary>
        /// Manage the entity in unordered lists
        /// </summary>
        MetadataContainer<object> Metadata {
            get;
        }

        /// <summary>
        /// Applies modifications to the entity, ie, swaps out old data with new data.
        /// </summary>
        void ApplyModifications();

        /// <summary>
        /// The unique id for the entity
        /// </summary>
        int UniqueId {
            get;
        }
    }

    public partial class Entity {
        public bool Enabled {
            get;
            set;
        }

        public T AddData<T>() where T : Data {
            return (T)AddData(DataMap<T>.Accessor);
        }

        public Data AddData(DataAccessor accessor) {
            Data data = DataAllocator.Allocate(accessor);
            data.Entity = this;

            int id = accessor.Id;
            _data[id] = new StoredData(data);

            _toAddStage1.Add(new DataAccessor(id));

            DispatchModificationNotification();
            DispatchDataStateChangedNotification();

            // Get our initial data from a prefab/etc
            DataAllocator.NotifyAllocated(accessor, this, _data[id].Current);

            // the user modifies the current state; the initialized data
            // is copied around to other instances when _added[id] is set to true
            // in the data state change update method
            return _data[id].Current;
        }
    }

    public partial class Entity {
        private static int _nextId;
        private int _uid;

        public int UniqueId {
            get { return _uid; }
        }

        internal protected Entity() {
            _uid = Interlocked.Increment(ref _nextId);
            Enabled = true; // default to being enabled
        }

        public override string ToString() {
            return string.Format("Entity [uid={0}]", _uid);
        }
    }

    /// <summary>
    /// A Entity contains a set of SimulationData.
    /// </summary>
    public partial class Entity : IEntity {
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
            /*
            if (EntityManager.HasInstance == false) {
                return;
            }
            */
            _toRemoveStage1.Add(accessor);

            DispatchModificationNotification();
            DispatchDataStateChangedNotification();
        }

        /*
        /// <summary>
        /// If this is true (which is normally should be), then the Entity will automatically
        /// be registered into the Entity Manager, which will allow it to interact with
        /// systems.
        /// </summary>
        /// <remarks>
        /// Having this set to false is only desirable when a singleton entity instance is
        /// desired, ie, an Entity that every system can access and that holds global level
        /// data.
        /// </remarks>
        public bool EntityManagerInjection = true;

        void OnEnable() {
            if (EntityManagerInjection) {
                EntityManager.Instance.AddEntity(this);
            }
        }

        void OnDisable() {
            if (EntityManagerInjection) {
                if (EntityManager.HasInstance) {
                    EntityManager.Instance.RemoveEntity(this);
                }
            }
        }

        public void Destroy() {
            if (EntityManagerInjection) {
                EntityManager.Instance.RemoveEntity(this);
            }
        }
        */

        public void Destroy() {
            _entityManager.RemoveEntity(this);
        }

        private static Swappable<T> CreateSwappable<T>(T a, T b) {
            return new Swappable<T>(a, b);
        }

        private class Swappable<T> {
            T _a;
            T _b;
            bool _current;

            public Swappable(T a, T b) {
                _a = a;
                _b = b;
            }

            public void Swap() {
                _current = !_current;
            }

            public T Current {
                get {
                    if (_current) {
                        return _a;
                    }
                    return _b;
                }
            }

            public T Previous {
                get {
                    if (_current) {
                        return _b;
                    }
                    return _a;
                }
            }
        }

        public event Action<Entity> OnDataStateChanged;
        private bool _onDataStateChangeNotificationDispatched;
        private void DispatchDataStateChangedNotification() {
            if (_onDataStateChangeNotificationDispatched == false) {
                _onDataStateChangeNotificationDispatched = true;
                if (OnDataStateChanged != null) {
                    OnDataStateChanged(this);
                }
            }
        }

        /// <summary>
        /// Notifies the world when a modification to the entity has been made.
        /// </summary>
        public event Action<Entity> OnModified;
        private bool _modificationNotificationDispatched;
        private void DispatchModificationNotification() {
            if (_modificationNotificationDispatched == false) {
                _modificationNotificationDispatched = true;
                if (OnModified != null) {
                    OnModified(this);
                }
            }
        }

        /// <summary>
        /// Determines which item in the tuple of simulation data is considered "active" and which is
        /// considered "inactive" / the previous value
        /// </summary>
        private int _swappableIndex;
        private class StoredData {
            private Data[] _items;
            private int _swappableIndex;

            public Data[] Items {
                get {
                    return _items;
                }
            }

            public StoredData(Data data) {
                _items = new Data[] { data.Duplicate(), data.Duplicate(), data.Duplicate() };
            }

            //public void Print(string prefix) {
            //    Log.Info("{0}update#{1} Modifying: {2} Current: {3} Previous: {4}", prefix, EntityManager.Instance.UpdateNumber, Modifying, Current, Previous);
            //}

            /// <summary>
            /// Updates Modifying/Current/Previous so that they point to the next element
            /// </summary>
            public void Increment() {
                // If the object we modified supports multiple modifications, then we need
                // to give it a chance to resolve those modifications so that it is in a
                // consistent state
                if (Modifying.SupportsMultipleModifications) {
                    Modifying.ResolveModifications();
                }

                // this code is tricky because we change what data.{Previous,Current,Modifying} refer to when we increment _swappableIndex below

                // when we increment _swappableIndex:
                //  modifying becomes current
                //  current becomes previous
                //  previous becomes modifying

                // current refers to the current _swappableIndex
                //  current_modifying : data.Modifying
                //  current_current : data.Current
                //  current_previous : data.Previous

                // next refers to when _swappableIndex += 1
                //  next_modifying : data.Previous
                //  next_current : data.Modifying
                //  next_previous : data.Current

                // we want next_modifying to become a copy of current_modifying
                Previous.CopyFrom(Modifying);
                // we want next_current to become a copy of current_modifying
                //Modifying.CopyFrom(data.Modifying);
                // we want next_previous to become a copy of current_current
                //Current.CopyFrom(data.Current);

                // Visualize based on Modifying
                Modifying.DoUpdateVisualization();

                // update Modifying/Current/Previous references
                ++_swappableIndex;

                //Print("After application");
            }

            public Data Modifying {
                get {
                    return _items[(_swappableIndex + 2) % _items.Length];
                }
                set {
                    _items[(_swappableIndex + 2) % _items.Length] = value;
                }
            }

            public Data Current {
                get {
                    return _items[(_swappableIndex + 1) % _items.Length];
                }
                set {
                    _items[(_swappableIndex + 1) % _items.Length] = value;
                }
            }

            public Data Previous {
                get {
                    return _items[(_swappableIndex + 0) % _items.Length];
                }
                set {
                    _items[(_swappableIndex + 0) % _items.Length] = value;
                }
            }
        }

        public Data[] GetAllData<T>() where T : Data {
            return GetAllData(DataMap<T>.Accessor);
        }

        public Data[] GetAllData(DataAccessor accessor) {
            return _data[accessor.Id].Items;
        }

        public IEnumerable<Data> SelectData(Predicate<Data> predicate) {
            foreach (Tuple<int, StoredData> tuple in _data) {
                StoredData data = tuple.Item2;
                DataAccessor accessor = new DataAccessor(DataFactory.Instance.GetId(data.Current.GetType()));
                if (WasAdded(accessor) == false && WasRemoved(accessor) == false && predicate(data.Current)) {
                    yield return data.Current;
                }
            }
        }

        /// <summary>
        /// The data contained within the Entity. One item in the tuple is the current state and one item
        /// is the next state.
        /// </summary>
        private IterableSparseArray<StoredData> _data = new IterableSparseArray<StoredData>();

        /// <summary>
        /// Data that has been modified this frame and needs to be pushed out
        /// </summary>
        private Swappable<IterableSparseArray<StoredData>> _modifications = new Swappable<IterableSparseArray<StoredData>>(
            new IterableSparseArray<StoredData>(), new IterableSparseArray<StoredData>());

        /// <summary>
        /// Items that are pending removal in the next update call
        /// </summary>

        List<DataAccessor> _toAddStage1 = new List<DataAccessor>();
        List<DataAccessor> _toAddStage2 = new List<DataAccessor>();
        SparseArray<bool> _added = new SparseArray<bool>();

        List<DataAccessor> _toRemoveStage1 = new List<DataAccessor>();
        List<DataAccessor> _toRemoveStage2 = new List<DataAccessor>();
        SparseArray<Data> _removed = new SparseArray<Data>();

        bool _removedAllData = false;

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
            _removedAllData = true;
        }

        private void DoModifications() {
            // apply modifications
            foreach (Tuple<int, StoredData> toApply in _modifications.Current) {
                // if we removed the data, then don't bother apply/dispatching modifications on it
                if (_data.Contains(toApply.Item1)) {
                    _data[toApply.Item1].Increment();
                }
            }
            _modifications.Swap();
            _modifications.Current.Clear();

            // move the swappable items so that previous/current/modifying works for the next generation
            ++_swappableIndex;
        }

        public void ApplyModifications() {
            DoModifications();

            _modificationNotificationDispatched = false;
            _onDataStateChangeNotificationDispatched = false;

            //Debug.Log(_frame++ + " WasAdded: " + WasAdded<TemporaryData>() + ", WasRemoved: " + WasRemoved<TemporaryData>());
        }

        public bool DataStateChangeUpdate() {
            // do additions
            for (int i = 0; i < _toAddStage1.Count; ++i) {
                int id = _toAddStage1[i].Id;
                _added[id] = true;

                // copy the initialized data into the previous data
                _data[id].Modifying.CopyFrom(_data[id].Current);

                // visualize the initial data
                _data[id].Current.DoUpdateVisualization();
            }
            for (int i = 0; i < _toAddStage2.Count; ++i) {
                _added[_toAddStage2[i].Id] = false;
            }
            _toAddStage2.Clear();
            Utils.Swap(ref _toAddStage1, ref _toAddStage2);

            // do removals
            for (int i = 0; i < _toRemoveStage1.Count; ++i) {
                int id = _toRemoveStage1[i].Id;
                _removed[id] = _data[id].Current;
            }
            for (int i = 0; i < _toRemoveStage2.Count; ++i) {
                int id = _toRemoveStage2[i].Id;
                _removed.Remove(id);
                _data.Remove(id);
            }
            _toRemoveStage2.Clear();
            Utils.Swap(ref _toRemoveStage1, ref _toRemoveStage2);

            return _toAddStage2.Count > 0 || _toRemoveStage2.Count > 0;
        }

        public T Modify<T>() where T : Data {
            return (T)Modify(DataMap<T>.Accessor, false);
        }
        public T Modify<T>(bool force) where T : Data {
            return (T)Modify(DataMap<T>.Accessor, force);
        }
        public Data Modify(DataAccessor accessor) {
            return Modify(accessor, false);
        }
        public Data Modify(DataAccessor accessor, bool force) {
            var id = accessor.Id;

            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(DataFactory.Instance.GetTypeFromAccessor(accessor));
            }
            if (_modifications.Current.Contains(id) && !force && _data[id].Current.SupportsMultipleModifications == false) {
                throw new RemodifiedDataException(DataFactory.Instance.GetTypeFromAccessor(accessor));
            }
            _modifications.Current[id] = _data[id];

            DispatchModificationNotification();

            return _data[id].Modifying;
        }

        public T Current<T>() where T : Data {
            return (T)Current(DataMap<T>.Accessor);
        }
        public Data Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(DataFactory.Instance.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Current;
        }

        public T Previous<T>() where T : Data {
            return (T)Previous(DataMap<T>.Accessor);
        }
        public Data Previous(DataAccessor accessor) {
            var id = accessor.Id;

            if (_data.Contains(id) == false) {
                throw new NoSuchDataException(DataFactory.Instance.GetTypeFromAccessor(accessor));
            }

            return _data[accessor.Id].Previous;
        }

        public bool ContainsModifableData<T>() where T : Data {
            return ContainsModifableData(DataMap<T>.Accessor);
        }
        public bool ContainsModifableData(DataAccessor accessor) {
            return _data.Contains(accessor.Id) && WasAdded(accessor) == false && WasRemoved(accessor) == false;
        }

        public bool ContainsData<T>() where T : Data {
            return ContainsData(DataMap<T>.Accessor);
        }
        public bool ContainsData(DataAccessor accessor) {
            int id = accessor.Id;
            return _data.Contains(id);
        }

        public bool WasAdded<T>() where T : Data {
            return WasAdded(DataMap<T>.Accessor);
        }
        public bool WasAdded(DataAccessor accessor) {
            return _added[accessor.Id];
        }

        public bool WasRemoved<T>() where T : Data {
            return WasRemoved(DataMap<T>.Accessor);
        }
        public bool WasRemoved(DataAccessor accessor) {
            int id = accessor.Id;
            return _removed.Contains(id) || (_removedAllData && _data.Contains(id));
        }

        public bool WasModified<T>() where T : Data {
            return WasModified(DataMap<T>.Accessor);
        }
        public bool WasModified(DataAccessor accessor) {
            return _modifications.Previous.Contains(accessor.Id);
        }
    }
}