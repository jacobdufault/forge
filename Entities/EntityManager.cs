using Neon.Collections;
using Neon.Entities.Serialization;
using Neon.Serialization;
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
        /// All entities that are currently in the EntityManager.
        /// </summary>
        IEnumerable<IEntity> Entities {
            get;
        }

        /// <summary>
        /// Singleton entity that contains global data.
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
        /// Should the EntityManager execute systems in separate threads?
        /// </summary>
        public static bool EnableMultithreading = true;

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
        /// A list of Entities that were removed from the EntityManager in the last update loop.
        /// This means that they are now ready to actually be removed from the EntityManager in this
        /// update.
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
        /// <remarks>
        /// If you look at other local variables, this one does not follow a common pattern of also
        /// storing the previous update's results. This is because this data is not shared with
        /// systems.
        /// </remarks>
        private ConcurrentWriterBag<Entity> _notifiedModifiedEntities = new ConcurrentWriterBag<Entity>();

        /// <summary>
        /// Entities that have state changes. Entities can have state changes for multiple frames,
        /// so the entities inside of this list can be from any update before the current one.
        /// </summary>
        // TODO: convert this to a bag if performance is a problem
        private List<Entity> _stateChangeEntities = new List<Entity>();

        /// <summary>
        /// Entities which have state changes in this frame. This collection will be added to
        /// _stateChangeEntities during the next update.
        /// </summary>
        private ConcurrentWriterBag<Entity> _notifiedStateChangeEntities = new ConcurrentWriterBag<Entity>();

        /// <summary>
        /// All of the multithreaded systems.
        /// </summary>
        private List<MultithreadedSystem> _multithreadedSystems = new List<MultithreadedSystem>();

        /// <summary>
        /// Lock used when modifying _updateTask.
        /// </summary>
        private object _updateTaskLock = new object();

        /// <summary>
        /// The task that represents our current Update method. This is not used internally except
        /// to notify the user that they cannot have concurrent Update calls running.
        /// </summary>
        private Task _updateTask;

        /// <summary>
        /// The key we use to access unordered list metadata from the entity.
        /// </summary>
        private static MetadataKey _entityUnorderedListMetadataKey = Entity.MetadataRegistry.GetKey();

        /// <summary>
        /// Events that the EntityManager dispatches.
        /// </summary>
        public EventProcessor EventProcessor = new EventProcessor();

        /// <summary>
        /// Singleton entity that contains global data.
        /// </summary>
        public IEntity SingletonEntity {
            get;
            set;
        }

        /// <summary>
        /// All entities that are currently in the EntityManager.
        /// </summary>
        public IEnumerable<IEntity> Entities {
            get {
                return _entities;
            }
        }

        /// <summary>
        /// Gets the update number.
        /// </summary>
        /// <value>The update number.</value>
        public int UpdateNumber {
            get;
            private set;
        }

        public EntityManager(IEntity singletonEntity) {
            SingletonEntity = (Entity)singletonEntity;
            SystemDoneEvent = new CountdownEvent(0);
            _eventProcessors.BeginMonitoring(EventProcessor);
        }

        internal EntityManager(int updateNumber, SerializedEntity singletonEntity,
            List<SerializedEntity> restoredEntities, List<ISystem> systems,
            SerializationConverter converter) {

            SystemDoneEvent = new CountdownEvent(0);
            _eventProcessors.BeginMonitoring(EventProcessor);
            
            {
                bool hasModification, hasStateChange;
                SingletonEntity = new Entity(singletonEntity, converter, out hasModification, out hasStateChange);
            }
            
            UpdateNumber = updateNumber;

            foreach (var serializedEntity in restoredEntities) {
                bool hasModification, hasStateChange;
                Entity restored = new Entity(serializedEntity, converter, out hasModification, out hasStateChange);

                if (serializedEntity.IsAdding) {
                    AddEntity(restored);
                }

                else {
                    // add the entity
                    InternalAddEntity(restored);

                    if (hasModification) {
                        restored.ModificationNotifier.Notify();
                    }

                    // done via InternalAddEntity
                    //if (hasStateChange) {
                    //    restored.DataStateChangeNotifier.Notify();
                    //}
                }

                if (serializedEntity.IsRemoving) {
                    RemoveEntity(restored);
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
                MultithreadedSystem multithreadingSystem = new MultithreadedSystem(this, (ITriggerBaseFilter)baseSystem);
                foreach (var entity in _entities) {
                    multithreadingSystem.Restore(entity);
                }

                _multithreadedSystems.Add(multithreadingSystem);
            }
            else {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Internal method to add an entity to the entity manager and register it with all
        /// associated systems. This executes the add immediately.
        /// </summary>
        /// <param name="toAdd">The entity to add.</param>
        private void InternalAddEntity(Entity toAdd) {
            toAdd.EntityManager = this;
            ((IEntity)toAdd).EventProcessor.Submit(ShowEntityEvent.Instance);

            // register listeners
            toAdd.ModificationNotifier.Listener += OnEntityModified;
            toAdd.DataStateChangeNotifier.Listener += OnEntityDataStateChanged;
            _eventProcessors.BeginMonitoring(((IEntity)toAdd).EventProcessor);

            // notify ourselves of data state changes so that it the entity is pushed to systems
            toAdd.DataStateChangeNotifier.Notify();

            // ensure it contains metadata for our keys
            ((IEntity)toAdd).Metadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();

            // add it our list of entities
            _entities.Add(toAdd, GetEntitiesListFromMetadata(toAdd));

            // notify listeners that we added an entity
            EventProcessor.Submit(new EntityAddedEvent(toAdd));
        }

        private void SinglethreadFrameEnd() {
            _addedEntities.Clear();
            _removedEntities.Clear();
        }

        private void SinglethreadFrameBegin() {
            // _addedEntities and _removedEntities were cleared in SinglethreadFrameEnd()
            _notifiedAddingEntities.CopyIntoAndClear(_addedEntities);
            _notifiedRemovedEntities.CopyIntoAndClear(_removedEntities);

            ++UpdateNumber;

            // Add entities
            for (int i = 0; i < _addedEntities.Count; ++i) {
                Entity toAdd = _addedEntities[i];

                InternalAddEntity(toAdd);

                // apply initialization changes
                toAdd.ApplyModifications();
            }
            // can't clear b/c it is shared

            // copy our state change entities notice that we do this after adding entities, because
            // adding entities triggers the data state change notifier
            _notifiedStateChangeEntities.CopyIntoAndClear(_stateChangeEntities);

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

                // notify listeners we removed an event
                EventProcessor.Submit(new EntityRemovedEvent(toRemove));
            }
            // can't clear b/c it is shared

            // Do a data state change on the given items.
            {
                int i = 0;
                while (i < _stateChangeEntities.Count) {
                    if (_stateChangeEntities[i].NeedsMoreDataStateChangeUpdates() == false) {
                        // reset the notifier so it can be added to the _stateChangeEntities again
                        _stateChangeEntities[i].DataStateChangeNotifier.Reset();
                        _stateChangeEntities.RemoveAt(i);
                    }
                    else {
                        _stateChangeEntities[i].DataStateChangeUpdate();
                        ++i;
                    }
                }
            }

            // apply the modifications to the modified entities this data is not shared, so we can
            // clear it
            _notifiedModifiedEntities.IterateAndClear(modified => {
                modified.ApplyModifications();
            });
        }

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

            // run all systems
            {
                SystemDoneEvent.Reset(_multithreadedSystems.Count);

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

        public void RunUpdateWorld(object commandsObject) {
            List<IStructuredInput> commands = (List<IStructuredInput>)commandsObject;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SinglethreadFrameBegin();
            long frameBegin = stopwatch.ElapsedTicks;

            MultithreadRunSystems(commands);
            long multithreadEnd = stopwatch.ElapsedTicks;

            SinglethreadFrameEnd();

            stopwatch.Stop();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();

            builder.AppendFormat("Frame updating took {0} ticks (before {1}, concurrent {2})", stopwatch.ElapsedTicks, frameBegin, multithreadEnd - frameBegin);

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
            Entity singletonEntity = (Entity)SingletonEntity;
            singletonEntity.ApplyModifications();
            singletonEntity.DataStateChangeUpdate();
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

        /// <summary>
        /// Runs all of the dirty event processors. The events for the event processors will be
        /// dispatched on the same thread that calls this method.
        /// </summary>
        public void RunEventProcessors() {
            _eventProcessors.DispatchEvents();
        }

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        public void AddEntity(IEntity instance) {
            Entity entity = (Entity)instance;

            _notifiedAddingEntities.Add(entity);

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
        /// Helper method that returns the _entities unordered list metadata.
        /// </summary>
        private UnorderedListMetadata GetEntitiesListFromMetadata(IEntity entity) {
            return (UnorderedListMetadata)entity.Metadata[_entityUnorderedListMetadataKey];
        }

        /// <summary>
        /// Called when an Entity has been modified.
        /// </summary>
        private void OnEntityModified(Entity sender) {
            _notifiedModifiedEntities.Add(sender);
        }

        /// <summary>
        /// Called when an entity has data state changes
        /// </summary>
        private void OnEntityDataStateChanged(Entity sender) {
            _notifiedStateChangeEntities.Add(sender);
        }

        /// <summary>
        /// Returns all entities that will be added in the next update.
        /// </summary>
        public List<Entity> GetEntitiesToAdd() {
            return _notifiedAddingEntities.ToList();
        }

        /// <summary>
        /// Returns all entities that will be removed in the next update.
        /// </summary>
        public List<Entity> GetEntitiesToRemove() {
            return _notifiedRemovedEntities.ToList();
        }

        #region MultithreadedSystemSharedContext Implementation
        List<Entity> MultithreadedSystemSharedContext.AddedEntities {
            get { return _addedEntities; }
        }

        List<Entity> MultithreadedSystemSharedContext.RemovedEntities {
            get { return _removedEntities; }
        }

        List<Entity> MultithreadedSystemSharedContext.StateChangedEntities {
            get { return _stateChangeEntities; }
        }

        /// <summary>
        /// Event the system uses to notify the primary thread that it is done processing.
        /// </summary>
        public CountdownEvent SystemDoneEvent { get; private set; }
        #endregion
    }
}