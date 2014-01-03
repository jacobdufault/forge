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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Forge.Entities.Implementation.Runtime {
    /// <summary>
    /// The EntityManager requires an associated Entity which is not injected into the
    /// EntityManager.
    /// </summary>
    internal sealed class GameEngine : MultithreadedSystemSharedContext, IGameEngine {
        private enum GameEngineNextState {
            SynchronizeState,
            Update
        }
        private GameEngineNextState _nextState;

        /// <summary>
        /// Should the EntityManager execute systems in separate threads?
        /// </summary>
        public static bool EnableMultithreading = true;

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
        /// Lock used when modifying _updateTask.
        /// </summary>
        private object _updateTaskLock = new object();

        /// <summary>
        /// The key we use to access unordered list metadata from the entity.
        /// </summary>
        private static int _entityUnorderedListMetadataKey = EntityManagerMetadata.GetUnorderedListMetadataIndex();

        private List<ExecutionGroup> _executionGroups;

        private List<ISystem> _systems;

        /// <summary>
        /// Generator used for generating unique identifiers for entities.
        /// </summary>
        public UniqueIntGenerator EntityIdGenerator;

        /// <summary>
        /// Events that the EntityManager dispatches.
        /// </summary>
        public EventNotifier EventNotifier = new EventNotifier();

        IEventNotifier IGameEngine.EventNotifier {
            get {
                return EventNotifier;
            }
        }

        /// <summary>
        /// Global entity that contains global data.
        /// </summary>
        private RuntimeEntity _globalEntity;

        /// <summary>
        /// Gets the update number.
        /// </summary>
        /// <value>The update number.</value>
        public int UpdateNumber {
            get;
            private set;
        }

        /// <summary>
        /// ITemplateGroup JSON so that we can create a snapshot of the content.
        /// </summary>
        private string _templateJson;

        /// <summary>
        /// Contains any exceptions that have occurred when running systems.
        /// </summary>
        private ConcurrentWriterBag<Exception> _multithreadingExceptions = new ConcurrentWriterBag<Exception>();

        public GameEngine(string snapshotJson, string templateJson) {
            _templateJson = templateJson;

            // Create our own little island of references with its own set of templates
            GameSnapshot snapshot = GameSnapshotRestorer.Restore(snapshotJson, templateJson,
                Maybe.Just(this));

            EntityIdGenerator = snapshot.EntityIdGenerator;

            _systems = snapshot.Systems;

            // TODO: ensure that when correctly restore UpdateNumber
            //UpdateNumber = updateNumber;

            _globalEntity = (RuntimeEntity)snapshot.GlobalEntity;

            EventNotifier.Submit(EntityAddedEvent.Create(_globalEntity));

            foreach (var entity in snapshot.AddedEntities) {
                AddEntity((RuntimeEntity)entity);
            }

            foreach (var entity in snapshot.ActiveEntities) {
                RuntimeEntity runtimeEntity = (RuntimeEntity)entity;

                // add the entity
                InternalAddEntity(runtimeEntity);

                // TODO: verify that if the modification notifier is already triggered, we can
                // ignore this
                //if (((ContentEntity)entity).HasModification) {
                //    runtimeEntity.ModificationNotifier.Notify();
                //}

                // done via InternalAddEntity
                //if (deserializedEntity.HasStateChange) {
                //    deserializedEntity.Entity.DataStateChangeNotifier.Notify();
                //}
            }

            foreach (var entity in snapshot.RemovedEntities) {
                RuntimeEntity runtimeEntity = (RuntimeEntity)entity;

                // add the entity
                InternalAddEntity(runtimeEntity);

                // TODO: verify that if the modification notifier is already triggered, we can
                // ignore this
                //if (((ContentEntity)entity).HasModification) {
                //    runtimeEntity.ModificationNotifier.Notify();
                //}

                // done via InternalAddEntity
                //if (deserializedEntity.HasStateChange) {
                //    deserializedEntity.Entity.DataStateChangeNotifier.Notify();
                //}

                RemoveEntity(runtimeEntity);
            }

            _executionGroups = new List<ExecutionGroup>();
            var executionGroups = SystemExecutionGroup.GetExecutionGroups(snapshot.Systems);
            foreach (var executionGroup in executionGroups) {
                List<MultithreadedSystem> multithreadedSystems = new List<MultithreadedSystem>();
                foreach (var system in executionGroup.Systems) {
                    MultithreadedSystem multithreaded = CreateMultithreadedSystem(system);
                    multithreadedSystems.Add(multithreaded);
                }
                _executionGroups.Add(new ExecutionGroup(multithreadedSystems));
            }

            _nextState = GameEngineNextState.SynchronizeState;
        }

        private class ExecutionGroup {
            public List<MultithreadedSystem> Systems;
            public ExecutionGroup(List<MultithreadedSystem> systems) {
                Systems = systems;
            }

            public void BookkeepingBeforeRunningSystems() {
                for (int i = 0; i < Systems.Count; ++i) {
                    Systems[i].BookkeepingBeforeRunningSystems();
                }
            }

            public void RunSystems(List<IGameInput> input) {
                for (int i = 0; i < Systems.Count; ++i) {
                    Systems[i].RunSystem(input);
                }
            }
        }

        /// <summary>
        /// Creates a multithreaded system from the given base system.
        /// </summary>
        private MultithreadedSystem CreateMultithreadedSystem(ISystem baseSystem) {
            baseSystem.EventDispatcher = EventNotifier;
            baseSystem.GlobalEntity = _globalEntity;

            if (baseSystem is ITriggerFilterProvider) {
                MultithreadedSystem multithreadingSystem = new MultithreadedSystem(this, (ITriggerFilterProvider)baseSystem);
                foreach (var entity in _entities) {
                    multithreadingSystem.Restore(entity);
                }
                return multithreadingSystem;
            }
            else {
                throw new NotImplementedException("No support for systems which are not deriving for ITriggerFilterProvider as of yet for system " + baseSystem);
            }
        }

        /// <summary>
        /// Internal method to add an entity to the entity manager and register it with all
        /// associated systems. This executes the add immediately.
        /// </summary>
        /// <param name="toAdd">The entity to add.</param>
        private void InternalAddEntity(RuntimeEntity toAdd) {
            toAdd.GameEngine = this;

            // notify listeners that we both added and created the entity
            Log<GameEngine>.Info("Submitting internal EntityAddedEvent for " + toAdd);
            EventNotifier.Submit(EntityAddedEvent.Create(toAdd));
            EventNotifier.Submit(ShowEntityEvent.Create(toAdd));

            // register listeners
            toAdd.ModificationNotifier.Listener += OnEntityModified;
            toAdd.DataStateChangeNotifier.Listener += OnEntityDataStateChanged;

            // notify ourselves of data state changes so that it the entity is pushed to systems
            toAdd.DataStateChangeNotifier.Notify();

            // ensure it contains metadata for our keys
            toAdd.Metadata.UnorderedListMetadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();

            // add it our list of entities
            _entities.Add(toAdd, GetEntitiesListFromMetadata(toAdd));
        }

        private void UpdateEntitiesWithStateChanges() {
            // update our list of added and removed entities
            _addedEntities.Clear();
            _removedEntities.Clear();
            _notifiedAddingEntities.IterateAndClear(entity => _addedEntities.Add(entity));
            _notifiedRemovedEntities.IterateAndClear(entity => _removedEntities.Add(entity));

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
            _notifiedStateChangeEntities.IterateAndClear(entity => _stateChangeEntities.Add(entity));

            // Remove entities
            for (int i = 0; i < _removedEntities.Count; ++i) {
                RuntimeEntity toRemove = _removedEntities[i];

                // remove listeners
                toRemove.ModificationNotifier.Listener -= OnEntityModified;
                toRemove.DataStateChangeNotifier.Listener -= OnEntityDataStateChanged;

                // remove all data from the entity and then push said changes out
                foreach (DataAccessor accessor in toRemove.SelectData()) {
                    toRemove.RemoveData(accessor);
                }
                toRemove.DataStateChangeUpdate();

                // remove the entity from the list of entities
                _entities.Remove(toRemove, GetEntitiesListFromMetadata(toRemove));
                EventNotifier.Submit(DestroyedEntityEvent.Create(toRemove));

                // notify listeners we removed an event
                EventNotifier.Submit(EntityRemovedEvent.Create(toRemove));
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

            // update the global entity
            _globalEntity.ApplyModifications();
            _globalEntity.DataStateChangeUpdate();
        }

        private void MultithreadRunSystems(List<IGameInput> input) {
            // run systems in a parallel context
            if (EnableMultithreading) {
                var result = Parallel.ForEach(_executionGroups,
                    group => group.BookkeepingBeforeRunningSystems());
                Contract.Requires(result.IsCompleted, "Bookkeeping failed to complete");

                result = Parallel.ForEach(_executionGroups,
                    group => group.RunSystems(input));
                Contract.Requires(result.IsCompleted, "Bookkeeping failed to complete");
            }

            // run systems in a single-threaded context
            else {
                for (int i = 0; i < _executionGroups.Count; ++i) {
                    _executionGroups[i].BookkeepingBeforeRunningSystems();
                }

                for (int i = 0; i < _executionGroups.Count; ++i) {
                    _executionGroups[i].RunSystems(input);
                }
            }

            // throw exceptions if any occurred while we were running the engine
            var exceptions = _multithreadingExceptions.ToList();
            if (exceptions.Count > 0) {
                throw new AggregateException(exceptions.ToArray());
            }
        }

        private void DoUpdate(List<IGameInput> commands) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long frameBegin = stopwatch.ElapsedTicks;

            MultithreadRunSystems(commands);
            long multithreadEnd = stopwatch.ElapsedTicks;

            stopwatch.Stop();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();

            builder.AppendFormat("Frame updating took {0} ticks (before {1}, concurrent {2})", stopwatch.ElapsedTicks, frameBegin, multithreadEnd - frameBegin);

            for (int i = 0; i < _executionGroups.Count; ++i) {
                for (int j = 0; j < _executionGroups[i].Systems.Count; ++j) {
                    builder.AppendLine();
                    builder.Append(_executionGroups[i].Systems[j].PerformanceData.Format);
                }
            }
            Log<GameEngine>.Info(builder.ToString());
        }

        private Task _updateWaitTask = null;
        private Task _synchronizeStateTask = null;

        public Task Update(IEnumerable<IGameInput> input) {
            if (_nextState != GameEngineNextState.Update) {
                throw new InvalidOperationException("Invalid call to Update; was expecting " + _nextState);
            }

            if (_updateWaitTask != null &&
                _updateWaitTask.IsCompleted == false &&
                _updateWaitTask.IsFaulted == false &&
                _updateWaitTask.IsCanceled == false) {
                throw new InvalidOperationException("Currently running Update task has not finished");
            }

            _updateWaitTask = Task.Factory.StartNew(() => {
                DoUpdate(input.ToList());
                _nextState = GameEngineNextState.SynchronizeState;
            });

            return _updateWaitTask;
        }

        public Task SynchronizeState() {
            if (_nextState != GameEngineNextState.SynchronizeState) {
                throw new InvalidOperationException("Invalid call to SynchronizeState; was expecting " + _nextState);
            }

            if (_synchronizeStateTask != null &&
                _synchronizeStateTask.IsCompleted == false &&
                _synchronizeStateTask.IsFaulted == false &&
                _synchronizeStateTask.IsCanceled == false) {
                throw new InvalidOperationException("Cannot call SynchronizeState before the " +
                    "returned Task has completed");
            }

            _synchronizeStateTask = Task.Factory.StartNew(() => {
                UpdateEntitiesWithStateChanges();
                _nextState = GameEngineNextState.Update;
            });

            return _synchronizeStateTask;
        }

        public void DispatchEvents() {
            EventNotifier.DispatchEvents();
        }

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        internal void AddEntity(RuntimeEntity instance) {
            _notifiedAddingEntities.Add(instance);
            EventNotifier.Submit(HideEntityEvent.Create(instance));
        }

        /// <summary>
        /// Removes the given entity from the world.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        internal void RemoveEntity(RuntimeEntity instance) {
            _notifiedRemovedEntities.Add(instance);
            EventNotifier.Submit(HideEntityEvent.Create(instance));
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
        private List<IEntity> GetEntitiesToAdd() {
            return _notifiedAddingEntities.ToList().Select(e => (IEntity)e).ToList();
        }

        /// <summary>
        /// Returns all entities that will be removed in the next update.
        /// </summary>
        private List<IEntity> GetEntitiesToRemove() {
            return _notifiedRemovedEntities.ToList().Select(e => (IEntity)e).ToList();
        }

        #region MultithreadedSystemSharedContext Implementation
        ConcurrentWriterBag<Exception> MultithreadedSystemSharedContext.Exceptions {
            get { return _multithreadingExceptions; }
        }

        List<RuntimeEntity> MultithreadedSystemSharedContext.AddedEntities {
            get { return _addedEntities; }
        }

        List<RuntimeEntity> MultithreadedSystemSharedContext.RemovedEntities {
            get { return _removedEntities; }
        }

        List<RuntimeEntity> MultithreadedSystemSharedContext.StateChangedEntities {
            get { return _stateChangeEntities; }
        }
        #endregion

        private GameSnapshot GetRawSnapshot() {
            if (_nextState != GameEngineNextState.Update) {
                throw new InvalidOperationException("You can only get a snapshot after synchronizing state");
            }

            GameSnapshot snapshot = new GameSnapshot();

            snapshot.GlobalEntity = _globalEntity;

            foreach (var adding in _notifiedAddingEntities.ToList()) {
                snapshot.AddedEntities.Add(adding);
            }

            List<RuntimeEntity> removing = _notifiedRemovedEntities.ToList();
            foreach (var entity in _entities) {
                bool isRemoving = removing.Contains(entity);

                if (isRemoving) {
                    snapshot.RemovedEntities.Add(entity);
                }
                else {
                    snapshot.ActiveEntities.Add(entity);
                }

            }

            snapshot.Systems = _systems;

            return snapshot;
        }

        public IGameSnapshot TakeSnapshot() {
            string snapshotJson = SerializationHelpers.Serialize(GetRawSnapshot(),
                RequiredConverters.GetConverters(),
                RequiredConverters.GetContextObjects(Maybe<GameEngine>.Empty));

            return GameSnapshotRestorer.Restore(snapshotJson, _templateJson, Maybe<GameEngine>.Empty);
        }

        public int GetVerificationHash() {
            string json = SerializationHelpers.Serialize(GetRawSnapshot(),
                RequiredConverters.GetConverters(),
                RequiredConverters.GetContextObjects(Maybe<GameEngine>.Empty));
            return json.GetHashCode();
        }

        public void Dispose() {
            _multithreadingExceptions.Dispose();
            _notifiedAddingEntities.Dispose();
            _notifiedModifiedEntities.Dispose();
            _notifiedRemovedEntities.Dispose();
            _notifiedStateChangeEntities.Dispose();

            foreach (RuntimeEntity entity in _entities) {
                entity.Dispose();
            }
            _globalEntity.Dispose();
        }
    }
}