using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neon.Entities {
    public class SimpleThreadPool {
        private Thread[] _threads;
        private ConcurrentBag<Action> _tasks;

        public SimpleThreadPool(int threads) {
            _tasks = new ConcurrentBag<Action>();

            _threads = new Thread[threads];
            for (int i = 0; i < _threads.Length; ++i) {
                _threads[i] = new Thread(ThreadStart);
                _threads[i].Start();
            }
        }

        ~SimpleThreadPool() {
            foreach (var thread in _threads) {
                thread.Abort();
            }
        }

        public void Push(Action task) {
            _tasks.Add(task);
        }

        private void ThreadStart() {
            try {
                while (true) {
                    Action task = null;
                    if (_tasks.TryTake(out task)) {
                        task();
                    }
                }
            }
            catch (ThreadAbortException) { }
        }


    }

    /// <summary>
    /// A set of operations that are used for managing entities.
    /// </summary>
    public interface IEntityManager {
        /// <summary>
        /// Code to call when we do our next update.
        /// </summary>
        //event Action OnNextUpdate;

        /// <summary>
        /// Our current update number. Useful for debugging purposes.
        /// </summary>
        int UpdateNumber {
            get;
        }

        /// <summary>
        /// Updates the world. State changes (entity add, entity remove, ...) are propagated to the
        /// different registered listeners. Update listeners will be called and the given commands
        /// will be executed.
        /// </summary>
        void UpdateWorld(IEnumerable<IStructuredInput> commands);

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="entity">The instance to add</param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// Destroys the given entity.
        /// </summary>
        /// <param name="entity">The entity instance to remove</param>
        void RemoveEntity(IEntity entity);

        /// <summary>
        /// Singleton entity that contains global data
        /// </summary>
        IEntity SingletonEntity {
            get;
            set;
        }
    }

    internal interface MultithreadedSystemSharedContext {
        IEntity SingletonEntity { get; }
        int ModifiedIndex { get; }
        List<Entity> AddedEntities { get; }
        List<Entity> RemovedEntities { get; }
        List<Entity> StateChangedEntities { get; }

        ValueToReferenceWrapper<long> MultithreadingIndex { get; }
    }

    public class ValueToReferenceWrapper<T> {
        public T Value;

        public ValueToReferenceWrapper(T value) {
            Value = value;
        }
    }

    internal class MultithreadedSystem {
        /// <summary>
        /// Entities which were modified last update
        /// </summary>
        private Bag<IEntity> _modifiedEntities = new Bag<IEntity>();

        /// <summary>
        /// Entities which need to be removed from _nextModifiedEntities
        /// </summary>
        private List<IEntity> _removedMutableEntities = new List<IEntity>();

        /// <summary>
        /// Entities that have been modified as this system is updating
        /// </summary>
        private Bag<IEntity> _nextModifiedEntities = new Bag<IEntity>();


        private MultithreadedSystemSharedContext _context;
        public System _system;
        private ITriggerModified _modifiedTrigger;

        private ITriggerGlobalPreUpdate _globalPreUpdateTrigger;
        private ITriggerUpdate _updateTrigger;
        private ITriggerGlobalPostUpdate _globalPostUpdateTrigger;

        internal MultithreadedSystem(MultithreadedSystemSharedContext context, System system, List<Entity> entitiesWithModifications) {
            _context = context;

            _system = system;
            _modifiedTrigger = system.Trigger as ITriggerModified;
            _globalPreUpdateTrigger = system.Trigger as ITriggerGlobalPreUpdate;
            _updateTrigger = system.Trigger as ITriggerUpdate;
            _globalPostUpdateTrigger = system.Trigger as ITriggerGlobalPostUpdate;

            foreach (var entity in entitiesWithModifications) {
                if (_system.Filter.Check(entity)) {
                    _nextModifiedEntities.Append(entity);
                }
            }
        }

        public void Restore(IEntity entity) {
            if (_system.Restore(entity)) {
                DoAdd(entity);
            }
        }

        private void DoAdd(IEntity added) {
            if (_modifiedTrigger != null) {
                ((Entity)added).ModificationNotifier.Listener += ModificationNotifier_Listener;
            }
        }

        private void DoRemove(IEntity removed) {
            // if we removed an entity from the cache, then we don't want to hear of any more
            // modification events
            if (_modifiedTrigger != null) {
                ((Entity)removed).ModificationNotifier.Listener -= ModificationNotifier_Listener;

                _modifiedEntities.Remove(removed);
                _removedMutableEntities.Add(removed);
            }
        }

        public void RunSystem(Object threadContext) {
            RunSystem();
        }
        public void BookkeepingAfterAllSystemsHaveRun() {
            BookkeepingAfterAllSystemsHaveRun(null);
        }

        public void BookkeepingAfterAllSystemsHaveRun(Object threadContext) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                _modifiedEntities.Clear();

                // copy everything from _nextModifiedEntities into _modifiedEntities, except those
                // items which have been removed
                for (int i = 0; i < _nextModifiedEntities.Length; ++i) {
                    IEntity entity = _nextModifiedEntities[i];
                    if (_removedMutableEntities.Contains(entity) == false) {
                        _modifiedEntities.Append(entity);
                    }
                }

                _nextModifiedEntities.Clear();
                _removedMutableEntities.Clear();

                //Log<EntityManager>.Info("[BEF] Running bookkeeping on {0} took {1} ticks", _system.Trigger.GetType(), stopwatch.ElapsedTicks);
            }
            finally {
                Interlocked.Increment(ref _context.MultithreadingIndex.Value);

                BookkeepingTicks = stopwatch.ElapsedTicks;
                //stopwatch.Stop();
                //Log<EntityManager>.Info("[AFT] Running bookkeeping on {0} took {1} ticks", _system.Trigger.GetType(), stopwatch.ElapsedTicks);
            }
        }

        public long RunSystemTicks;
        public long BookkeepingTicks;

        public long AddedTicks;
        public long RemovedTicks;
        public long StateChangeTicks;
        public long ModificationTicks;
        public long UpdateTicks;

        public void RunSystem() {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                // process entities that were added to the system
                int addedCount = _context.AddedEntities.Count;
                for (int i = 0; i < addedCount; ++i) {
                    IEntity added = _context.AddedEntities[i];
                    if (_system.UpdateCache(added) == System.CacheChangeResult.Added) {
                        DoAdd(added);
                    }
                }
                AddedTicks = stopwatch.ElapsedTicks;

                // process entities that were removed from the system
                int removedCount = _context.RemovedEntities.Count;
                for (int i = 0; i < removedCount; ++i) {
                    IEntity removed = _context.RemovedEntities[i];
                    if (_system.Remove(removed)) {
                        DoRemove(removed);
                    }
                }
                RemovedTicks = stopwatch.ElapsedTicks - AddedTicks;

                // process state changes
                for (int i = 0; i < _context.StateChangedEntities.Count; ++i) {
                    IEntity stateChanged = _context.StateChangedEntities[i];
                    System.CacheChangeResult change = _system.UpdateCache(stateChanged);
                    if (change == System.CacheChangeResult.Added) {
                        DoAdd(stateChanged);
                    }
                    else if (change == System.CacheChangeResult.Removed) {
                        DoRemove(stateChanged);
                    }
                }
                StateChangeTicks = stopwatch.ElapsedTicks - RemovedTicks - AddedTicks;

                // process modifications
                if (_modifiedTrigger != null) {
                    for (int i = 0; i < _modifiedEntities.Length; ++i) {
                        IEntity entity = _modifiedEntities[i];
                        if (_system.Filter.ModificationCheck(entity)) {
                            _modifiedTrigger.OnModified(entity);
                        }
                    }
                }
                ModificationTicks = stopwatch.ElapsedTicks - StateChangeTicks - RemovedTicks - AddedTicks;

                // run update methods Call the BeforeUpdate methods - *user code*
                if (_globalPreUpdateTrigger != null) {
                    _globalPreUpdateTrigger.OnGlobalPreUpdate(_context.SingletonEntity);
                }

                if (_updateTrigger != null) {
                    for (int i = 0; i < _system.CachedEntities.Length; ++i) {
                        IEntity updated = _system.CachedEntities[i];
                        _updateTrigger.OnUpdate(updated);
                    }
                }
                UpdateTicks = stopwatch.ElapsedTicks - ModificationTicks - StateChangeTicks - RemovedTicks - AddedTicks;

                if (_globalPostUpdateTrigger != null) {
                    _globalPostUpdateTrigger.OnGlobalPostUpdate(_context.SingletonEntity);
                }

            }
            finally {
                Interlocked.Increment(ref _context.MultithreadingIndex.Value);
                RunSystemTicks = stopwatch.ElapsedTicks;
            }
        }

        private void ModificationNotifier_Listener(Entity entity) {
            lock (_nextModifiedEntities) {
                _nextModifiedEntities.Append(entity);
            }
        }

    }

    /// <summary>
    /// The EntityManager requires an associated Entity which is not injected into the
    /// EntityManager.
    /// </summary>
    public class EntityManager : IEntityManager, MultithreadedSystemSharedContext {
        /// <summary>
        /// Event processors which need their events dispatched.
        /// </summary>
        private List<EventProcessor> _dirtyEventProcessors = new List<EventProcessor>();

        /// <summary>
        /// The list of active Entities in the world.
        /// </summary>
        private UnorderedList<IEntity> _entities = new UnorderedList<IEntity>();

        /// <summary>
        /// A list of Entities that need to be added to the world.
        /// </summary>
        private BufferedItem<List<Entity>> _entitiesToAdd = new BufferedItem<List<Entity>>();
        private List<Entity> AddImmutable() {
            return _entitiesToAdd.Get(0);
        }
        private List<Entity> AddMutable() {
            return _entitiesToAdd.Get(1);
        }

        /// <summary>
        /// A list of Entities that need to be removed from the world.
        /// </summary>
        private BufferedItem<List<Entity>> _entitiesToRemove = new BufferedItem<List<Entity>>();
        private List<Entity> RemoveImmutable() {
            return _entitiesToRemove.Get(0);
        }
        private List<Entity> RemoveMutable() {
            return _entitiesToRemove.Get(1);
        }

        /// <summary>
        /// A double buffered list of Entities that have been modified.
        /// </summary>
        private List<Entity> _entitiesWithModifications = new List<Entity>();

        /// <summary>
        /// The entities that are dirty relative to system caches.
        /// </summary>
        private List<Entity> _cacheUpdateCurrent = new List<Entity>();
        private List<Entity> _cacheUpdatePending = new List<Entity>();

        private List<System> _systemsWithInputTriggers = new List<System>();

        private List<MultithreadedSystem> _multithreadedSystems = new List<MultithreadedSystem>();

        private List<ITriggerGlobalInput> _globalInputTriggers = new List<ITriggerGlobalInput>();

        /// <summary>
        /// The key we use to access unordered list metadata from the entity.
        /// </summary>
        private static MetadataKey _entityUnorderedListMetadataKey = Entity.MetadataRegistry.GetKey();

        /// <summary>
        /// The key we use to access our modified listeners for the entity
        /// </summary>
        private static MetadataKey _entityModifiedListenersKey = Entity.MetadataRegistry.GetKey();

        private Entity _singletonEntity;
        public IEntity SingletonEntity {
            get {
                return _singletonEntity;
            }
            set {
                _singletonEntity = (Entity)value;
            }
        }

        public ValueToReferenceWrapper<long> MultithreadingIndex { get; private set; }

        public EntityManager(IEntity singletonEntity) {
            _singletonEntity = (Entity)singletonEntity;
            MultithreadingIndex = new ValueToReferenceWrapper<long>(0);
        }

        public class RestoredEntity {
            public bool HasModification;
            public bool HasStateChange;

            public bool IsAdding;
            public bool IsRemoving;

            public Entity Entity;
        }

        internal EntityManager(int updateNumber, IEntity singletonEntity, List<RestoredEntity> restoredEntities, List<ISystem> systems) {
            MultithreadingIndex = new ValueToReferenceWrapper<long>(0);
            
            UpdateNumber = updateNumber;
            SingletonEntity = singletonEntity;

            foreach (var restoredEntity in restoredEntities) {
                if (restoredEntity.IsAdding) {
                    AddEntity(restoredEntity.Entity);
                }

                else {
                    // add the entity
                    {
                        Entity toAdd = restoredEntity.Entity;

                        toAdd.EntityManager = this;
                        ((IEntity)toAdd).EventProcessor.Submit(ShowEntityEvent.Instance);

                        // register listeners
                        toAdd.ModificationNotifier.Listener += OnEntityModified;
                        toAdd.DataStateChangeNotifier.Listener += OnEntityDataStateChanged;
                        ((IEntity)toAdd).EventProcessor.EventAddedNotifier.Listener += EventProcessor_OnEventAdded;

                        // apply initialization changes
                        toAdd.ApplyModifications();

                        // ensure it contains metadata for our keys
                        ((IEntity)toAdd).Metadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();

                        // add it our list of entities
                        _entities.Add(toAdd, GetEntitiesListFromMetadata(toAdd));
                    }

                    Console.WriteLine("Entity " + restoredEntity.Entity + " has modification? " + restoredEntity.HasModification);
                    if (restoredEntity.HasModification) {
                        restoredEntity.Entity.ModificationNotifier.Notify();
                    }

                    if (restoredEntity.HasStateChange) {
                        restoredEntity.Entity.DataStateChangeNotifier.Notify();
                    }
                }

                if (restoredEntity.IsRemoving) {
                    RemoveEntity(restoredEntity.Entity);
                }
            }

            foreach (var system in systems) {
                AddSystem(system);
            }
        }

        /// <summary>
        /// Registers the given system with the EntityManager.
        /// </summary>
        public void AddSystem(ISystem baseSystem) {
            if (baseSystem is ITriggerBaseFilter) {
                System system = new System((ITriggerBaseFilter)baseSystem);
                MultithreadedSystem multithreadingSystem = new MultithreadedSystem(this, system, _entitiesWithModifications);
                foreach (var entity in _entities) {
                    multithreadingSystem.Restore(entity);
                }

                _multithreadedSystems.Add(multithreadingSystem);

                if (baseSystem is ITriggerInput) {
                    _systemsWithInputTriggers.Add(system);
                }
            }
            else if (baseSystem is ITriggerGlobalPostUpdate || baseSystem is ITriggerGlobalPreUpdate) {
                throw new NotImplementedException();
            }

            if (baseSystem is ITriggerGlobalInput) {
                _globalInputTriggers.Add((ITriggerGlobalInput)baseSystem);
            }

        }

        public int UpdateNumber {
            get;
            private set;
        }

        private void SinglethreadFrameBegin() {
            _entitiesToAdd.Swap();
            _entitiesToRemove.Swap();

            _cacheUpdateCurrent.AddRange(_cacheUpdatePending);
            _cacheUpdatePending.Clear();

            ++UpdateNumber;

            // Add entities
            List<Entity> addImmutable = AddImmutable();
            for (int i = 0; i < addImmutable.Count; ++i) {
                Entity toAdd = addImmutable[i];

                toAdd.EntityManager = this;
                ((IEntity)toAdd).EventProcessor.Submit(ShowEntityEvent.Instance);

                // register listeners
                toAdd.ModificationNotifier.Listener += OnEntityModified;
                toAdd.DataStateChangeNotifier.Listener += OnEntityDataStateChanged;
                ((IEntity)toAdd).EventProcessor.EventAddedNotifier.Listener += EventProcessor_OnEventAdded;

                // apply initialization changes
                toAdd.ApplyModifications();

                // ensure it contains metadata for our keys
                ((IEntity)toAdd).Metadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();

                // add it our list of entities
                _entities.Add(toAdd, GetEntitiesListFromMetadata(toAdd));
            }
            // can't clear b/c it is shared

            // Remove entities
            List<Entity> removeImmutable = RemoveImmutable();
            for (int i = 0; i < removeImmutable.Count; ++i) {
                Entity toDestroy = removeImmutable[i];

                // remove listeners
                toDestroy.ModificationNotifier.Listener -= OnEntityModified;
                toDestroy.DataStateChangeNotifier.Listener -= OnEntityDataStateChanged;
                ((IEntity)toDestroy).EventProcessor.EventAddedNotifier.Listener -= EventProcessor_OnEventAdded;

                // remove all data from the entity and then push said changes out
                toDestroy.RemoveAllData();
                toDestroy.DataStateChangeUpdate();

                // remove the entity from the list of entities
                _entities.Remove(toDestroy, GetEntitiesListFromMetadata(toDestroy));
                ((IEntity)toDestroy).EventProcessor.Submit(DestroyedEntityEvent.Instance);
            }
            // can't clear b/c it is shared

            // Note that this loop is carefully constructed It has to handle a couple of (difficult)
            // things; first, it needs to support the item that is being iterated being removed, and
            // secondly, it needs to support more items being added to it as it iterates
            for (int i = 0; i < _cacheUpdateCurrent.Count; ++i) {
                Entity entity = _cacheUpdateCurrent[i];
                entity.DataStateChangeUpdate();
            }

            // apply the modifications to the modified entities
            foreach (var modified in _entitiesWithModifications) {
                modified.ApplyModifications();
            }
            _entitiesWithModifications.Clear(); // this is not shared so we can clear it
        }

        public static bool EnableMultithreading = false;

        //private SimpleThreadPool pool = new SimpleThreadPool(1);

        private void MultithreadRunSystems() {
            // run all bookkeeping
            {
                // TODO: use a countdown event
                //CountdownEvent countdown = new CountdownEvent();
                //countdown.Reset(32);

                MultithreadingIndex.Value = 0;
                int multithreadingIndexTarget = _multithreadedSystems.Count;

                // run all systems
                for (int i = 0; i < _multithreadedSystems.Count; ++i) {
                    if (EnableMultithreading) {
                        Task.Factory.StartNew(_multithreadedSystems[i].BookkeepingAfterAllSystemsHaveRun, null);
                        //bool success = ThreadPool.UnsafeQueueUserWorkItem(_multithreadedSystems[i].BookkeepingAfterAllSystemsHaveRun, null);
                        //Contract.Requires(success, "Unable to submit threading task to ThreadPool");
                        //pool.Push(_multithreadedSystems[i].BookkeepingAfterAllSystemsHaveRun);
                    }
                    else {
                        _multithreadedSystems[i].BookkeepingAfterAllSystemsHaveRun();
                    }
                }

                // block until the systems are done
                while (Interlocked.Read(ref MultithreadingIndex.Value) != multithreadingIndexTarget) {
                }

                //countdown.Wait();
            }



            {
                MultithreadingIndex.Value = 0;
                int multithreadingIndexTarget = _multithreadedSystems.Count;

                // run all systems
                for (int i = 0; i < _multithreadedSystems.Count; ++i) {
                    //_resetEvents[i].Reset();

                    if (EnableMultithreading) {
                        Task.Factory.StartNew(_multithreadedSystems[i].RunSystem, null);
                        //bool success = ThreadPool.UnsafeQueueUserWorkItem(_multithreadedSystems[i].RunSystem, null);
                        //Contract.Requires(success, "Unable to submit threading task to ThreadPool");
                        //pool.Push(_multithreadedSystems[i].RunSystem);
                    }
                    else {
                        _multithreadedSystems[i].RunSystem();
                    }
                }

                // block until the systems are done
                while (Interlocked.Read(ref MultithreadingIndex.Value) != multithreadingIndexTarget) {
                }
            }


            //for (int i = 0; i < _resetEvents.Count; ++i) {
            //    _resetEvents[i].WaitOne();
            //}
        }

        private void SinglethreadFrameEnd() {
            // clear out immutable states
            AddImmutable().Clear();
            RemoveImmutable().Clear();

            // update immutable/mutable states for cache updates
            {
                int i = 0;
                while (i < _cacheUpdateCurrent.Count) {
                    if (_cacheUpdateCurrent[i].NeedsMoreDataStateChangeUpdates() == false) {
                        _cacheUpdateCurrent.RemoveAt(i);
                    }
                    else {
                        ++i;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the world. State changes (entity add, entity remove, ...) are propagated to the
        /// different registered listeners. Update listeners will be called and the given commands
        /// will be executed.
        /// </summary>
        public void UpdateWorld(IEnumerable<IStructuredInput> commands) {
            string stats = string.Format("cacheUpdateCurrent.Count={0} cacheUpdatePending={1} dirtyEventProcessors={2} entitiesToAdd(0)={3} entitiesToAdd(1)={4} entitiesToRemove(0)={4} entitiesToRemove(1)={5} entitiesWithModifications={6}",
                this._cacheUpdateCurrent.Count,
                this._cacheUpdatePending.Count,
                this._dirtyEventProcessors.Count,
                this._entitiesToAdd.Get(0).Count,
                this._entitiesToAdd.Get(1).Count,
                this._entitiesToRemove.Get(0).Count,
                this._entitiesToRemove.Get(1).Count,
                this._entitiesWithModifications.Count);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SinglethreadFrameBegin();
            long frameBegin = stopwatch.ElapsedTicks;

            MultithreadRunSystems();
            long multithreadEnd = stopwatch.ElapsedTicks;

            SinglethreadFrameEnd();
            long frameEnd = stopwatch.ElapsedTicks;

            stopwatch.Stop();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();


            builder.AppendFormat("Frame updating took {0} (before {1}, concurrent {2}, after {3}) ticks with info {4}", stopwatch.ElapsedTicks, frameBegin, multithreadEnd - frameBegin, frameEnd - multithreadEnd, stats);

            for (int i = 0; i < _multithreadedSystems.Count; ++i) {
                builder.AppendLine();
                builder.AppendFormat(@"  {1}/{2} ({3}|{4}|{5}|{6}|{7}) ticks for system {0}",
                    _multithreadedSystems[i]._system.Trigger.GetType(),

                    _multithreadedSystems[i].BookkeepingTicks,
                    _multithreadedSystems[i].RunSystemTicks,

                    _multithreadedSystems[i].AddedTicks,
                    _multithreadedSystems[i].RemovedTicks,
                    _multithreadedSystems[i].StateChangeTicks,
                    _multithreadedSystems[i].ModificationTicks,
                    _multithreadedSystems[i].UpdateTicks);

            }
            Log<EntityManager>.Info(builder.ToString());
            Log<EntityManager>.Info("------------------");
            Log<EntityManager>.Info("------------------");
            Log<EntityManager>.Info("------------------");
            Log<EntityManager>.Info("------------------");
            Log<EntityManager>.Info("------------------");
            Log<EntityManager>.Info("------------------");

            InvokeOnCommandMethods(commands);

            // update the singleton data
            _singletonEntity.ApplyModifications();

            // update dirty event processors (this has to be done on the main thread)
            InvokeEventProcessors();
        }

        private void InvokeEventProcessors() {
            for (int i = 0; i < _dirtyEventProcessors.Count; ++i) {
                _dirtyEventProcessors[i].DispatchEvents();
            }
            _dirtyEventProcessors.Clear();
        }

        private UnorderedListMetadata GetEntitiesListFromMetadata(IEntity entity) {
            return (UnorderedListMetadata)entity.Metadata[_entityUnorderedListMetadataKey];
        }

        /// <summary>
        /// Dispatches the set of commands to all [InvokeOnCommand] methods.
        /// </summary>
        private void InvokeOnCommandMethods(IEnumerable<IStructuredInput> inputSequence) {
            // Call the OnCommand methods - *user code*
            foreach (var input in inputSequence) {
                for (int i = 0; i < _globalInputTriggers.Count; ++i) {
                    var trigger = _globalInputTriggers[i];
                    if (trigger.IStructuredInputType.IsInstanceOfType(input)) {
                        trigger.OnGlobalInput(input, SingletonEntity);
                    }
                }

                for (int i = 0; i < _systemsWithInputTriggers.Count; ++i) {
                    System system = _systemsWithInputTriggers[i];
                    ITriggerInput trigger = (ITriggerInput)system.Trigger;

                    for (int j = 0; j < system.CachedEntities.Length; ++j) {
                        IEntity entity = system.CachedEntities[j];
                        if (entity.Enabled) {
                            trigger.OnInput(input, entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        public void AddEntity(IEntity instance) {
            lock (this) {
                Entity entity = (Entity)instance;
                AddMutable().Add(entity);
                _cacheUpdatePending.Add(entity);
                ((IEntity)instance).EventProcessor.Submit(HideEntityEvent.Instance);
            }
        }

        /// <summary>
        /// Removes the given entity from the world.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        // TODO: make this internal
        public void RemoveEntity(IEntity instance) {
            lock (this) {
                Entity entity = (Entity)instance;
                RemoveMutable().Add(entity);
                ((IEntity)instance).EventProcessor.Submit(HideEntityEvent.Instance);
            }
        }

        /// <summary>
        /// Called when an Entity has been modified.
        /// </summary>
        private void OnEntityModified(Entity sender) {
            lock (this) {
                _entitiesWithModifications.Add(sender);
            }
        }

        /// <summary>
        /// Called when an entity has data state changes
        /// </summary>
        private void OnEntityDataStateChanged(Entity sender) {
            lock (this) {
                _cacheUpdatePending.Add(sender);
            }
        }

        /// <summary>
        /// Called when an event processor has had an event added to it.
        /// </summary>
        private void EventProcessor_OnEventAdded(EventProcessor eventProcessor) {
            lock (this) {
                _dirtyEventProcessors.Add(eventProcessor);
            }
        }

        int MultithreadedSystemSharedContext.ModifiedIndex {
            get { return UpdateNumber; }
        }

        List<Entity> MultithreadedSystemSharedContext.AddedEntities {
            get { return AddImmutable(); }
        }

        List<Entity> MultithreadedSystemSharedContext.RemovedEntities {
            get { return RemoveImmutable(); }
        }

        List<Entity> MultithreadedSystemSharedContext.StateChangedEntities {
            get { return _cacheUpdateCurrent; }
        }
    }
}