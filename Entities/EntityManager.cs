using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neon.Entities {
    /// <summary>
    /// A set of operations that are used for managing entities.
    /// </summary>
    public interface IEntityManager {
        /// <summary>
        /// Updates the world on another thread. Systems have their respective triggers activated.
        /// The structured input commands are dispatched to systems which are interested. Make sure
        /// that this method is not invoked before the returned Task is completed.
        /// </summary>
        Task UpdateWorld(List<IStructuredInput> commands);

        /// <summary>
        /// Adds the given entity to the EntityManager.
        /// </summary>
        /// <param name="entity">The instance to add</param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// Runs all of the dirty event processors. The events for the event processors will be
        /// dispatched on the same thread that calls this method.
        /// </summary>
        void RunEventProcessors();

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
        /// Manages all of the event processors.
        /// </summary>
        private EventProcessorManager _eventProcessors = new EventProcessorManager();

        /// <summary>
        /// The list of active Entities in the world.
        /// </summary>
        private UnorderedList<IEntity> _entities = new UnorderedList<IEntity>();

        /// <summary>
        /// A list of Entities that were added to the EntityManager in the last update loop. This
        /// means that they are now ready to actually be added to the EntityManager in this update.
        /// </summary>
        private List<Entity> _addedEntities = new List<Entity>();

        /// <summary>
        /// The entities which are added to the EntityManager in this frame. This is concurrently
        /// written to as systems create new entities.
        /// </summary>
        private ConcurrentWriterBag<Entity> _notifiedAddingEntities = new ConcurrentWriterBag<Entity>();


        /// <summary>
        /// A list of Entities that were removed from the EntityManager in the last update loop. This
        /// means that they are now ready to actually be removed from the EntityManager in this update.
        /// </summary>
        private List<Entity> _removedEntities = new List<Entity>();

        /// <summary>
        /// The entities which are removed to the EntityManager in this frame. This is concurrently
        /// written to as systems remove entities.
        /// </summary>
        private ConcurrentWriterBag<Entity> _notifiedRemovedEntities = new ConcurrentWriterBag<Entity>();

        /// <summary>
        /// A list of Entities that have been modified.
        /// </summary>
        // TODO: concurrent writer
        private List<Entity> _entitiesWithModifications = new List<Entity>();

        /// <summary>
        /// The entities that are dirty relative to system caches.
        /// </summary>
        // TODO: replace this
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
                        _eventProcessors.BeginMonitoring(((IEntity)toAdd).EventProcessor);

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
            _addedEntities.Clear();
            _notifiedAddingEntities.CopyIntoAndClear(_addedEntities);

            _removedEntities.Clear();
            _notifiedRemovedEntities.CopyIntoAndClear(_removedEntities);

            _cacheUpdateCurrent.AddRange(_cacheUpdatePending);
            _cacheUpdatePending.Clear();

            ++UpdateNumber;

            // Add entities
            for (int i = 0; i < _addedEntities.Count; ++i) {
                Entity toAdd = _addedEntities[i];

                toAdd.EntityManager = this;
                ((IEntity)toAdd).EventProcessor.Submit(ShowEntityEvent.Instance);

                // register listeners
                toAdd.ModificationNotifier.Listener += OnEntityModified;
                toAdd.DataStateChangeNotifier.Listener += OnEntityDataStateChanged;
                _eventProcessors.BeginMonitoring(((IEntity)toAdd).EventProcessor);

                // apply initialization changes
                toAdd.ApplyModifications();

                // ensure it contains metadata for our keys
                ((IEntity)toAdd).Metadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();

                // add it our list of entities
                _entities.Add(toAdd, GetEntitiesListFromMetadata(toAdd));
            }
            // can't clear b/c it is shared

            // Remove entities
            for (int i = 0; i < _removedEntities.Count; ++i) {
                Entity toRemove = _removedEntities[i];

                // remove listeners
                toRemove.ModificationNotifier.Listener -= OnEntityModified;
                toRemove.DataStateChangeNotifier.Listener -= OnEntityDataStateChanged;
                _eventProcessors.StopMonitoring(((IEntity)toRemove).EventProcessor);

                // remove all data from the entity and then push said changes out
                toRemove.RemoveAllData();
                toRemove.DataStateChangeUpdate();

                // remove the entity from the list of entities
                _entities.Remove(toRemove, GetEntitiesListFromMetadata(toRemove));
                ((IEntity)toRemove).EventProcessor.Submit(DestroyedEntityEvent.Instance);
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

        private object _updateTaskLock = new object();
        private Task _updateTask;

        public void RunUpdateWorld(object commandsObject) {
            List<IStructuredInput> commands = (List<IStructuredInput>)commandsObject;

            string stats = string.Format("cacheUpdateCurrent.Count={0} cacheUpdatePending={1} entitiesWithModifications={2}",
                this._cacheUpdateCurrent.Count,
                this._cacheUpdatePending.Count,
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
        }

        public Task UpdateWorld(List<IStructuredInput> commands) {
            lock (_updateTaskLock) {
                if (_updateTask != null) {
                    throw new InvalidOperationException("Cannot call UpdateWorld before the returned task has completed.");
                }

                Task updateTask = Task.Factory.StartNew(RunUpdateWorld, commands);
                _updateTask = updateTask.ContinueWith((t) => {
                    lock (_updateTaskLock) {
                        _updateTask = null;
                    }
                });

                return _updateTask;
            }
        }

        public void RunEventProcessors() {
            _eventProcessors.DispatchEvents();
        }

        private UnorderedListMetadata GetEntitiesListFromMetadata(IEntity entity) {
            return (UnorderedListMetadata)entity.Metadata[_entityUnorderedListMetadataKey];
        }

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        public void AddEntity(IEntity instance) {
            Entity entity = (Entity)instance;
            _notifiedAddingEntities.Add(entity);

            lock (this) {
                _cacheUpdatePending.Add(entity);
            }

            instance.EventProcessor.Submit(HideEntityEvent.Instance);
        }

        /// <summary>
        /// Removes the given entity from the world.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        // TODO: make this internal
        public void RemoveEntity(IEntity instance) {
            _notifiedRemovedEntities.Add((Entity)instance);
            instance.EventProcessor.Submit(HideEntityEvent.Instance);
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

        List<Entity> MultithreadedSystemSharedContext.AddedEntities {
            get { return _addedEntities; }
        }

        List<Entity> MultithreadedSystemSharedContext.RemovedEntities {
            get { return _removedEntities; }
        }

        List<Entity> MultithreadedSystemSharedContext.StateChangedEntities {
            get { return _cacheUpdateCurrent; }
        }
    }
}