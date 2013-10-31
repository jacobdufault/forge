using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Entities {
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
        /// Registers the given system with the EntityManager.
        /// </summary>
        void AddSystem(ISystem system);

        /// <summary>
        /// Updates the world. State changes (entity add, entity remove, ...) are propogated to the
        /// different registered listeners. Update listeners will be called and the given commands
        /// will be executed.
        /// </summary>
        void UpdateWorld(IEnumerable<IStructuredInput> commands);

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// Destroys the given entity.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
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
    }

    internal class MultithreadedSystem {
        /// <summary>
        /// The list of entities that are a) in this system and b) have been modified
        /// </summary>
        private List<IEntity>[] _modifiedEntities = new[] { new List<IEntity>(), new List<IEntity>() };
        private MultithreadedSystemSharedContext _context;
        private System _system;
        private ITriggerModified _modifiedTrigger;

        private ITriggerGlobalPreUpdate _globalPreUpdateTrigger;
        private ITriggerUpdate _updateTrigger;
        private ITriggerGlobalPostUpdate _globalPostUpdateTrigger;

        /// <summary>
        /// Reset event used to notify the primary thread when this system is done processing
        /// </summary>
        private ManualResetEvent _doneEvent;

        internal MultithreadedSystem(MultithreadedSystemSharedContext context, System system, ManualResetEvent doneEvent) {
            _context = context;

            _system = system;
            _modifiedTrigger = system.Trigger as ITriggerModified;
            _globalPreUpdateTrigger = system.Trigger as ITriggerGlobalPreUpdate;
            _updateTrigger = system.Trigger as ITriggerUpdate;
            _globalPostUpdateTrigger = system.Trigger as ITriggerGlobalPostUpdate;

            _doneEvent = doneEvent;
        }

        private void DoAdd(IEntity added) {
            Log<MultithreadedSystem>.Info("Adding {0} to the cache in {1}", added, _system.Trigger.GetType());
            if (_modifiedTrigger != null) {
                ((Entity)added).ModificationNotifier.Listener += ModificationNotifier_Listener;
            }
        }

        private void DoRemove(IEntity removed) {
            // if we removed an entity from the cache, then we don't want to hear of any more
            // modification events
            if (_modifiedTrigger != null) {
                ((Entity)removed).ModificationNotifier.Listener -= ModificationNotifier_Listener;

                ImmutableModifiedList().Remove(removed);

                List<IEntity> mutableModified = MutableModifiedList();
                lock (mutableModified) {
                    mutableModified.Remove(removed);
                }
            }
        }

        public void RunSystem(Object threadContext) {
            try {
                Log<MultithreadedSystem>.Info("Running on thread {0} for {1}", Thread.CurrentThread.ManagedThreadId, _system.Trigger.GetType());

                // process entities that were added to the system
                for (int i = 0; i < _context.AddedEntities.Count; ++i) {
                    IEntity added = _context.AddedEntities[i];
                    if (_system.UpdateCache(added) == System.CacheChangeResult.Added) {
                        DoAdd(added);
                    }
                }

                // process entities that were removed from the system
                for (int i = 0; i < _context.RemovedEntities.Count; ++i) {
                    IEntity removed = _context.RemovedEntities[i];
                    if (_system.Remove(removed)) {
                        DoRemove(removed);
                    }
                }

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

                // process modifications
                if (_modifiedTrigger != null) {
                    List<IEntity> immutableModified = ImmutableModifiedList();
                    for (int i = 0; i < immutableModified.Count; ++i) {
                        IEntity entity = immutableModified[i];
                        if (_system.Filter.ModificationCheck(entity)) {
                            Log<MultithreadedSystem>.Info("Dispatched modification notification for {0} in {1}", entity, _system.Trigger.GetType());
                            _modifiedTrigger.OnModified(immutableModified[i]);
                        }
                        else {
                            Log<MultithreadedSystem>.Info("Did not dispatch modification notification for {0} in {1}", entity, _system.Trigger.GetType());
                        }
                    }
                    immutableModified.Clear();
                }

                // run update methods
                // Call the BeforeUpdate methods - *user code*
                if (_globalPreUpdateTrigger != null) {
                    _globalPreUpdateTrigger.OnGlobalPreUpdate(_context.SingletonEntity);
                }

                if (_updateTrigger != null) {
                    for (int i = 0; i < _system.CachedEntities.Length; ++i) {
                        IEntity updated = _system.CachedEntities[i];
                        Log<MultithreadedSystem>.Info("Dispatched update notification for {0} in {1}", updated, _system.Trigger.GetType());
                        _updateTrigger.OnUpdate(updated);
                    }
                }
                else {
                    Log<MultithreadedSystem>.Info("Did not dispatch update notifications for {0}", _system.Trigger.GetType());

                }

                if (_globalPostUpdateTrigger != null) {
                    _globalPostUpdateTrigger.OnGlobalPostUpdate(_context.SingletonEntity);
                }

                ImmutableModifiedList().Clear();
            }
            finally {
                _doneEvent.Set();
            }
        }

        private void ConcurrentRemove(IEntity item, List<IEntity> list) {
            int length = list.Count;
            for (int i = 0; i < length; ++i) {
                if (item == list[i]) {
                    list[i] = null;
                }
            }
        }

        private List<IEntity> ImmutableModifiedList() {
            return _modifiedEntities[(_context.ModifiedIndex) % 2];
        }
        private List<IEntity> MutableModifiedList() {
            return _modifiedEntities[(_context.ModifiedIndex + 1) % 2];
        }

        private void ModificationNotifier_Listener(Entity entity) {
            List<IEntity> list = MutableModifiedList();
            lock (list) {
                list.Add(entity);
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

        private List<System> _allSystems = new List<System>();
        private List<System> _systemsWithUpdateTriggers = new List<System>();
        private List<System> _systemsWithInputTriggers = new List<System>();

        private List<ManualResetEvent> _resetEvents = new List<ManualResetEvent>();
        private List<MultithreadedSystem> _multithreadedSystems = new List<MultithreadedSystem>();

        /*
        private class ModifiedTrigger {
            public ITriggerModified Trigger;
            public Filter Filter;

            public ModifiedTrigger(ITriggerModified trigger) {
                Trigger = trigger;
                Filter = new Filter(DataAccessorFactory.MapTypesToDataAccessors(trigger.ComputeEntityFilter()));
            }
        }
        private List<ModifiedTrigger> _modifiedTriggers = new List<ModifiedTrigger>();
        */

        private List<ITriggerGlobalPreUpdate> _globalPreUpdateTriggers = new List<ITriggerGlobalPreUpdate>();
        private List<ITriggerGlobalPostUpdate> _globalPostUpdateTriggers = new List<ITriggerGlobalPostUpdate>();
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

        public EntityManager(IEntity singletonEntity) {
            _singletonEntity = (Entity)singletonEntity;
        }

        /// <summary>
        /// Registers the given system with the EntityManager.
        /// </summary>
        public void AddSystem(ISystem baseSystem) {
            Contract.Requires(_entities.Length == 0, "Cannot add a trigger after entities have been added");
            Log<EntityManager>.Info("({0}) Adding system {1}", UpdateNumber, baseSystem);

            if (baseSystem is ITriggerBaseFilter) {
                System system = new System((ITriggerBaseFilter)baseSystem);
                _allSystems.Add(system);

                if (baseSystem is ITriggerUpdate) {
                    _systemsWithUpdateTriggers.Add(system);
                }
                if (baseSystem is ITriggerInput) {
                    _systemsWithInputTriggers.Add(system);
                }

                ManualResetEvent doneEvent = new ManualResetEvent(false);
                _resetEvents.Add(doneEvent);
                _multithreadedSystems.Add(new MultithreadedSystem(this, system, doneEvent));
            }

            if (baseSystem is ITriggerGlobalPreUpdate) {
                _globalPreUpdateTriggers.Add((ITriggerGlobalPreUpdate)baseSystem);
            }
            if (baseSystem is ITriggerGlobalPostUpdate) {
                _globalPostUpdateTriggers.Add((ITriggerGlobalPostUpdate)baseSystem);
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
                toAdd.Show();

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
                toDestroy.RemovedFromEntityManager();
            }
            // can't clear b/c it is shared

            // Note that this loop is carefully constructed It has to handle a couple of (difficult)
            // things; first, it needs to support the item that is being iterated being removed, and
            // secondly, it needs to support more items being added to it as it iterates
            for (int i = 0; i < _cacheUpdateCurrent.Count; ++i) {
                Entity entity = _cacheUpdateCurrent[i];
                Log<EntityManager>.Info("{0}: Apply data state change update to {1}", UpdateNumber, entity);
                entity.DataStateChangeUpdate();
            }

            // apply the modifications to the modified entities
            foreach (var modified in _entitiesWithModifications) {
                Log<EntityManager>.Info("{0}: Applying modifications to {1}", UpdateNumber, modified);
                modified.ApplyModifications();
            }
            _entitiesWithModifications.Clear(); // this is not shared so we can clear it
        }

        private void MultithreadRunSystems() {
            // run all systems
            for (int i = 0; i < _multithreadedSystems.Count; ++i) {
                _resetEvents[i].Reset();

                _multithreadedSystems[i].RunSystem(null);
                //bool success = ThreadPool.QueueUserWorkItem(_multithreadedSystems[i].RunSystem);
                //Contract.Requires(success, "Unable to submit threading task to ThreadPool");
            }

            // block until the systems are done
            for (int i = 0; i < _resetEvents.Count; ++i) {
                _resetEvents[i].WaitOne();
            }
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
            SinglethreadFrameBegin();
            MultithreadRunSystems();
            SinglethreadFrameEnd();

            InvokeOnCommandMethods(commands);

            // update the singleton data
            _singletonEntity.ApplyModifications();

            // update dirty event processors
            InvokeEventProcessors();
        }

        private void InvokeEventProcessors() {
            Log<EntityManager>.Info("({0}) Invoking event processors; numInvoking={1}", UpdateNumber, _dirtyEventProcessors.Count);
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
                    Log<EntityManager>.Info("({0}) Checking {1} for input {2}", UpdateNumber, trigger, input);
                    if (trigger.IStructuredInputType.IsInstanceOfType(input)) {
                        Log<EntityManager>.Info("({0}) Invoking {1} on input {2}", UpdateNumber, trigger, input);
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
            Log<EntityManager>.Info("({0}) AddEntity({1}) called", UpdateNumber, instance);
            Entity entity = (Entity)instance;
            AddMutable().Add(entity);
            _cacheUpdatePending.Add(entity);
            entity.Hide();
        }

        /// <summary>
        /// Removes the given entity from the world.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        // TODO: make this internal
        public void RemoveEntity(IEntity instance) {
            Log<EntityManager>.Info("({0}) RemoveEntity({1}) called", UpdateNumber, instance);

            Entity entity = (Entity)instance;
            RemoveMutable().Add(entity);
            entity.Hide();
        }

        /// <summary>
        /// Called when an Entity has been modified.
        /// </summary>
        private void OnEntityModified(Entity sender) {
            _entitiesWithModifications.Add(sender);
        }

        /// <summary>
        /// Called when an entity has data state changes
        /// </summary>
        private void OnEntityDataStateChanged(Entity sender) {
            _cacheUpdatePending.Add(sender);
        }

        /// <summary>
        /// Called when an event processor has had an event added to it.
        /// </summary>
        private void EventProcessor_OnEventAdded(EventProcessor eventProcessor) {
            _dirtyEventProcessors.Add(eventProcessor);
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