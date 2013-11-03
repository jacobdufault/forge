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
    /// <summary>
    /// A set of operations that are used for managing entities.
    /// </summary>
    public interface IEntityManager {
        /// <summary>
        /// The current update number that the entity manager is on.
        /// </summary>
        int UpdateNumber {
            get;
        }

        /// <summary>
        /// Updates the world. Systems have their respective triggers activated. The structured
        /// input commands are dispatched to systems which are interested.
        /// </summary>
        void UpdateWorld(List<IStructuredInput> commands);

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="entity">The instance to add</param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// Singleton entity that contains global data
        /// </summary>
        IEntity SingletonEntity {
            get;
            set;
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

        private List<MultithreadedSystem> _multithreadedSystems = new List<MultithreadedSystem>();

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

        public CountdownEvent SystemDoneEvent { get; private set; }

        public EntityManager(IEntity singletonEntity) {
            _singletonEntity = (Entity)singletonEntity;
            SystemDoneEvent = new CountdownEvent(0);
        }

        public class RestoredEntity {
            public bool HasModification;
            public bool HasStateChange;

            public bool IsAdding;
            public bool IsRemoving;

            public Entity Entity;
        }

        internal EntityManager(int updateNumber, IEntity singletonEntity, List<RestoredEntity> restoredEntities, List<ISystem> systems) {
            SystemDoneEvent = new CountdownEvent(0);
            
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
                MultithreadedSystem multithreadingSystem = new MultithreadedSystem(this, (ITriggerBaseFilter)baseSystem, _entitiesWithModifications);
                foreach (var entity in _entities) {
                    multithreadingSystem.Restore(entity);
                }

                _multithreadedSystems.Add(multithreadingSystem);
            }
            else {
                throw new NotImplementedException();
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

        private void MultithreadRunSystems(List<IStructuredInput> input) {
            // run all bookkeeping
            {
                SystemDoneEvent.Reset(_multithreadedSystems.Count);

                // run all systems
                for (int i = 0; i < _multithreadedSystems.Count; ++i) {
                    if (EnableMultithreading) {
                        Task.Factory.StartNew(_multithreadedSystems[i].BookkeepingBeforeRunningSystems);
                        //bool success = ThreadPool.UnsafeQueueUserWorkItem(_multithreadedSystems[i].BookkeepingAfterAllSystemsHaveRun, null);
                        //Contract.Requires(success, "Unable to submit threading task to ThreadPool");
                    }
                    else {
                        _multithreadedSystems[i].BookkeepingBeforeRunningSystems();
                    }
                }

                // block until the systems are done
                SystemDoneEvent.Wait();
            }



            {
                SystemDoneEvent.Reset(_multithreadedSystems.Count);

                // run all systems
                for (int i = 0; i < _multithreadedSystems.Count; ++i) {
                    if (EnableMultithreading) {
                        Task.Factory.StartNew(_multithreadedSystems[i].RunSystem, input);
                        //bool success = ThreadPool.UnsafeQueueUserWorkItem(_multithreadedSystems[i].RunSystem, input);
                        //Contract.Requires(success, "Unable to submit threading task to ThreadPool");
                    }
                    else {
                        _multithreadedSystems[i].RunSystem(input);
                    }
                }

                // block until the systems are done
                SystemDoneEvent.Wait();
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
        public void UpdateWorld(List<IStructuredInput> commands) {
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

            MultithreadRunSystems(commands);
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
                    _multithreadedSystems[i].Trigger.GetType(),

                    _multithreadedSystems[i].BookkeepingTicks,
                    _multithreadedSystems[i].RunSystemTicks,

                    _multithreadedSystems[i].AddedTicks,
                    _multithreadedSystems[i].RemovedTicks,
                    _multithreadedSystems[i].StateChangeTicks,
                    _multithreadedSystems[i].ModificationTicks,
                    _multithreadedSystems[i].UpdateTicks);

            }
            Log<EntityManager>.Info(builder.ToString());

            // update the singleton data
            _singletonEntity.ApplyModifications();
            _singletonEntity.DataStateChangeUpdate();

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