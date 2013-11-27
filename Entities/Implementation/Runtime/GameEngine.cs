using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Serialization;
using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neon.Entities {
    /// <summary>
    /// The EntityManager requires an associated Entity which is not injected into the
    /// EntityManager.
    /// </summary>
    internal class GameEngine : MultithreadedSystemSharedContext {
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
        private UnorderedList<RuntimeEntity> _entities = new UnorderedList<RuntimeEntity>();

        /// <summary>
        /// A list of Entities that were added to the EntityManager in the last update loop. This
        /// means that they are now ready to actually be added to the EntityManager in this update.
        /// </summary>
        private List<RuntimeEntity> _addedEntities = new List<RuntimeEntity>();

        /// <summary>
        /// The entities which are added to the EntityManager in this frame. This is concurrently
        /// written to as systems create new entities.
        /// </summary>
        private ConcurrentWriterBag<RuntimeEntity> _notifiedAddingEntities = new ConcurrentWriterBag<RuntimeEntity>();

        /// <summary>
        /// A list of Entities that were removed from the EntityManager in the last update loop.
        /// This means that they are now ready to actually be removed from the EntityManager in this
        /// update.
        /// </summary>
        private List<RuntimeEntity> _removedEntities = new List<RuntimeEntity>();

        /// <summary>
        /// The entities which are removed to the EntityManager in this frame. This is concurrently
        /// written to as systems remove entities.
        /// </summary>
        private ConcurrentWriterBag<RuntimeEntity> _notifiedRemovedEntities = new ConcurrentWriterBag<RuntimeEntity>();

        /// <summary>
        /// A list of Entities that have been modified.
        /// </summary>
        /// <remarks>
        /// If you look at other local variables, this one does not follow a common pattern of also
        /// storing the previous update's results. This is because this data is not shared with
        /// systems.
        /// </remarks>
        private ConcurrentWriterBag<RuntimeEntity> _notifiedModifiedEntities = new ConcurrentWriterBag<RuntimeEntity>();

        /// <summary>
        /// Entities that have state changes. Entities can have state changes for multiple frames,
        /// so the entities inside of this list can be from any update before the current one.
        /// </summary>
        // TODO: convert this to a bag if performance is a problem
        private List<RuntimeEntity> _stateChangeEntities = new List<RuntimeEntity>();

        /// <summary>
        /// Entities which have state changes in this frame. This collection will be added to
        /// _stateChangeEntities during the next update.
        /// </summary>
        private ConcurrentWriterBag<RuntimeEntity> _notifiedStateChangeEntities = new ConcurrentWriterBag<RuntimeEntity>();

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
        private static int _entityUnorderedListMetadataKey = EntityManagerMetadata.GetUnorderedListMetadataIndex();

        /// <summary>
        /// Events that the EntityManager dispatches.
        /// </summary>
        public IEventNotifier EventNotifier = new EventNotifier();

        /// <summary>
        /// Singleton entity that contains global data.
        /// </summary>
        public RuntimeEntity SingletonEntity {
            get;
            set;
        }

        /// <summary>
        /// Gets the update number.
        /// </summary>
        /// <value>The update number.</value>
        public int UpdateNumber {
            get;
            private set;
        }

        public GameEngine(IContentDatabase contentDatabase, int updateNumber) {
            foreach (var template in contentDatabase.Templates) {
                Template tem = (Template)template;

                if (tem != null) {
                    throw new InvalidOperationException("Attempt to create multiple GameEngines " +
                        "from the same content database; this is not currently supported");
                }

                tem.GameEngine = this;
            }

            // TODO: ensure that we send out systems send out state change removal notifications

            SystemDoneEvent = new CountdownEvent(0);
            _eventProcessors.BeginMonitoring((EventNotifier)EventNotifier);

            UpdateNumber = updateNumber;

            SingletonEntity = new RuntimeEntity((ContentEntity)contentDatabase.SingletonEntity);

            foreach (var entity in contentDatabase.ActiveEntities) {
                AddEntity(new RuntimeEntity((ContentEntity)entity));
            }

            foreach (var entity in contentDatabase.AddedEntities) {
                RuntimeEntity runtimeEntity = new RuntimeEntity((ContentEntity)entity);

                // add the entity
                InternalAddEntity(runtimeEntity);

                if (((ContentEntity)entity).HasModification) {
                    runtimeEntity.ModificationNotifier.Notify();
                }

                // done via InternalAddEntity
                //if (deserializedEntity.HasStateChange) {
                //    deserializedEntity.Entity.DataStateChangeNotifier.Notify();
                //}
            }

            foreach (var entity in contentDatabase.RemovedEntities) {
                RuntimeEntity runtimeEntity = new RuntimeEntity((ContentEntity)entity);

                // add the entity
                InternalAddEntity(runtimeEntity);

                if (((ContentEntity)entity).HasModification) {
                    runtimeEntity.ModificationNotifier.Notify();
                }

                // done via InternalAddEntity
                //if (deserializedEntity.HasStateChange) {
                //    deserializedEntity.Entity.DataStateChangeNotifier.Notify();
                //}

                RemoveEntity(runtimeEntity);
            }

            foreach (var system in contentDatabase.Systems) {
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
        private void InternalAddEntity(RuntimeEntity toAdd) {
            toAdd.GameEngine = this;
            ((EventNotifier)((IEntity)toAdd).EventNotifier).Submit(ShowEntityEvent.Instance);

            // register listeners
            toAdd.ModificationNotifier.Listener += OnEntityModified;
            toAdd.DataStateChangeNotifier.Listener += OnEntityDataStateChanged;
            _eventProcessors.BeginMonitoring((EventNotifier)((IEntity)toAdd).EventNotifier);

            // notify ourselves of data state changes so that it the entity is pushed to systems
            toAdd.DataStateChangeNotifier.Notify();

            // ensure it contains metadata for our keys
            toAdd.Metadata.UnorderedListMetadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();

            // add it our list of entities
            _entities.Add(toAdd, GetEntitiesListFromMetadata(toAdd));

            // notify listeners that we added an entity
            ((EventNotifier)EventNotifier).Submit(new EntityAddedEvent(toAdd));
        }

        private void SinglethreadFrameEnd() {
            _addedEntities.Clear();
            _removedEntities.Clear();
        }

        public void UpdateEntitiesWithStateChanges() {
            // _addedEntities and _removedEntities were cleared in SinglethreadFrameEnd()
            _notifiedAddingEntities.CopyIntoAndClear(_addedEntities);
            _notifiedRemovedEntities.CopyIntoAndClear(_removedEntities);

            ++UpdateNumber;

            // Add entities
            for (int i = 0; i < _addedEntities.Count; ++i) {
                RuntimeEntity toAdd = _addedEntities[i];

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
                RuntimeEntity toRemove = _removedEntities[i];

                // remove listeners
                toRemove.ModificationNotifier.Listener -= OnEntityModified;
                toRemove.DataStateChangeNotifier.Listener -= OnEntityDataStateChanged;
                _eventProcessors.StopMonitoring((EventNotifier)((IEntity)toRemove).EventNotifier);

                // remove all data from the entity and then push said changes out
                toRemove.RemoveAllData();
                toRemove.DataStateChangeUpdate();

                // remove the entity from the list of entities
                _entities.Remove(toRemove, GetEntitiesListFromMetadata(toRemove));
                ((EventNotifier)((IEntity)toRemove).EventNotifier).Submit(DestroyedEntityEvent.Instance);

                // notify listeners we removed an event
                ((EventNotifier)EventNotifier).Submit(new EntityRemovedEvent(toRemove));
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

            // update the singleton data
            SingletonEntity.ApplyModifications();
            SingletonEntity.DataStateChangeUpdate();
        }

        private void MultithreadRunSystems(List<IGameInput> input) {
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
            List<IGameInput> commands = (List<IGameInput>)commandsObject;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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
            Log<GameEngine>.Info(builder.ToString());
        }

        public Task UpdateWorld(List<IGameInput> commands) {
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
        public void AddEntity(RuntimeEntity instance) {
            _notifiedAddingEntities.Add(instance);

            ((EventNotifier)((IEntity)instance).EventNotifier).Submit(HideEntityEvent.Instance);
        }

        /// <summary>
        /// Removes the given entity from the world.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        // TODO: make this internal
        public void RemoveEntity(RuntimeEntity instance) {
            _notifiedRemovedEntities.Add(instance);
            ((EventNotifier)((IEntity)instance).EventNotifier).Submit(HideEntityEvent.Instance);
        }

        /// <summary>
        /// Helper method that returns the _entities unordered list metadata.
        /// </summary>
        private UnorderedListMetadata GetEntitiesListFromMetadata(RuntimeEntity entity) {
            return entity.Metadata.UnorderedListMetadata[_entityUnorderedListMetadataKey];
        }

        /// <summary>
        /// Called when an Entity has been modified.
        /// </summary>
        private void OnEntityModified(RuntimeEntity sender) {
            _notifiedModifiedEntities.Add(sender);
        }

        /// <summary>
        /// Called when an entity has data state changes
        /// </summary>
        private void OnEntityDataStateChanged(RuntimeEntity sender) {
            _notifiedStateChangeEntities.Add(sender);
        }

        /// <summary>
        /// Returns all entities that will be added in the next update.
        /// </summary>
        public List<IEntity> GetEntitiesToAdd() {
            return _notifiedAddingEntities.ToList().Select(e => (IEntity)e).ToList();
        }

        /// <summary>
        /// Returns all entities that will be removed in the next update.
        /// </summary>
        public List<IEntity> GetEntitiesToRemove() {
            return _notifiedRemovedEntities.ToList().Select(e => (IEntity)e).ToList();
        }

        #region MultithreadedSystemSharedContext Implementation
        List<RuntimeEntity> MultithreadedSystemSharedContext.AddedEntities {
            get { return _addedEntities; }
        }

        List<RuntimeEntity> MultithreadedSystemSharedContext.RemovedEntities {
            get { return _removedEntities; }
        }

        List<RuntimeEntity> MultithreadedSystemSharedContext.StateChangedEntities {
            get { return _stateChangeEntities; }
        }

        /// <summary>
        /// Event the system uses to notify the primary thread that it is done processing.
        /// </summary>
        public CountdownEvent SystemDoneEvent { get; private set; }
        #endregion
    }
}