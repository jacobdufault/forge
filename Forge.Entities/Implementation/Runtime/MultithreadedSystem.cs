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
using Forge.Entities.Implementation.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Forge.Entities.Implementation.Runtime {
    /// <summary>
    /// Shared state between all multithreaded systems.
    /// </summary>
    internal interface MultithreadedSystemSharedContext {
        /// <summary>
        /// Entities which have been added.
        /// </summary>
        List<RuntimeEntity> AddedEntities { get; }

        /// <summary>
        /// Entities which have been removed.
        /// </summary>
        List<RuntimeEntity> RemovedEntities { get; }

        /// <summary>
        /// Entities which have state changes.
        /// </summary>
        List<RuntimeEntity> StateChangedEntities { get; }

        /// <summary>
        /// Contains any exceptions that occurred while running systems.
        /// </summary>
        ConcurrentWriterBag<Exception> Exceptions { get; }
    }

    /// <summary>
    /// Runs an ISystem in another thread.
    /// </summary>
    internal sealed class MultithreadedSystem : IDisposable {
        /// <summary>
        /// Entities that were added to the system that need to be dispatched to the system.
        /// </summary>
        /// <remarks>
        /// This is populated in the bookkeeping phase and is not touched during execution.
        /// </remarks>
        private List<IEntity> _dispatchAdded = new List<IEntity>();

        /// <summary>
        /// Entities that were removed from the system that need to be dispatched to the system.
        /// </summary>
        /// <remarks>
        /// This is populated in the bookkeeping phase and is not touched during execution.
        /// </remarks>
        private List<IEntity> _dispatchRemoved = new List<IEntity>();

        /// <summary>
        /// Entities that were modified in the last update that need to be dispatched to the system.
        /// </summary>
        /// <remarks>
        /// This is populated in the bookkeeping phase and is not touched during execution.
        /// </remarks>
        private List<IEntity> _dispatchModified = new List<IEntity>();

        /// <summary>
        /// Entities that have been modified since bookkeeping last ran.
        /// </summary>
        private ConcurrentWriterBag<IEntity> _notifiedModifiedEntities = new ConcurrentWriterBag<IEntity>();

        /// <summary>
        /// A cache of all entities which have passed the entity filter.
        /// </summary>
        private EntityCache _entityCache;

        /// <summary>
        /// Our shared context.
        /// </summary>
        private MultithreadedSystemSharedContext _shared;

        // Cached triggers
        private Trigger.Added _triggerAdded;
        private Trigger.Removed _triggerRemoved;
        private Trigger.Modified _triggerModified;
        private Trigger.GlobalPreUpdate _triggerGlobalPreUpdate;
        private Trigger.Update _triggerUpdate;
        private Trigger.GlobalPostUpdate _triggerGlobalPostUpdate;
        private Trigger.Input _triggerInput;
        private Trigger.GlobalInput _triggerGlobalInput;

        /// <summary>
        /// Filter we use for filtering entities
        /// </summary>
        private Filter _filter;

        /// <summary>
        /// The system that this MultithreadedSystem maps to.
        /// </summary>
        public ISystem System;

        /// <summary>
        /// Performance data
        /// </summary>
        public PerformanceInformation PerformanceData;

        internal MultithreadedSystem(MultithreadedSystemSharedContext sharedData, ITriggerFilterProvider trigger) {
            PerformanceData = new PerformanceInformation(trigger);

            _shared = sharedData;

            _filter = new Filter(DataAccessorFactory.MapTypesToDataAccessors(trigger.RequiredDataTypes()));
            _entityCache = new EntityCache(_filter);

            System = trigger;
            _triggerAdded = trigger as Trigger.Added;
            _triggerRemoved = trigger as Trigger.Removed;
            _triggerModified = trigger as Trigger.Modified;
            _triggerGlobalPreUpdate = trigger as Trigger.GlobalPreUpdate;
            _triggerUpdate = trigger as Trigger.Update;
            _triggerGlobalPostUpdate = trigger as Trigger.GlobalPostUpdate;
            _triggerInput = trigger as Trigger.Input;
            _triggerGlobalInput = trigger as Trigger.GlobalInput;
        }

        public void Restore(RuntimeEntity entity) {
            if (_entityCache.UpdateCache(entity) == EntityCache.CacheChangeResult.Added) {
                DoAdd(entity);
            }
        }

        /// <summary>
        /// Called when an entity that is contained within the cache has been modified.
        /// </summary>
        /// <remarks>
        /// This function is only called if we have a modification trigger to invoke.
        /// </remarks>
        private void ModificationNotifier_Listener(RuntimeEntity entity) {
            // Notice that we cannot check to see if the entity passes the modification filter here,
            // because this callback is just informing us that the entity has been modified; it does
            // not mean that the entity is done being modified.
            //
            // In other words, the entity may fail the modification filter check now, but at a later
            // point in time the check may succeed.
            _notifiedModifiedEntities.Add(entity);
        }

        private void DoAdd(RuntimeEntity added) {
            if (_triggerModified != null) {
                added.ModificationNotifier.Listener += ModificationNotifier_Listener;
            }
        }

        private void DoRemove(RuntimeEntity removed) {
            // if we removed an entity from the cache, then we don't want to hear of any more
            // modification events
            if (_triggerModified != null) {
                removed.ModificationNotifier.Listener -= ModificationNotifier_Listener;

                // We have to remove the entity from our list of entities to dispatch, as during the
                // frame when OnRemoved is called OnModified (and also OnUpdate) will not be
                // invoked.
                _dispatchModified.Remove(removed);
            }
        }

        /// <summary>
        /// Runs bookkeeping on the system. All systems concurrently run this function. This
        /// function makes an *extremely* important guarantee that there will be no external API
        /// calls made that can modify the state of other systems that are currently executing.
        /// </summary>
        public void BookkeepingBeforeRunningSystems() {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                // copy our modified entities into our dispatch modified list we do this before
                // state changes so that we only have to remove from _dispatchModified and not
                // _notifiedModifiedEntities
                if (_triggerModified != null) {
                    _dispatchModified.Clear();
                    _notifiedModifiedEntities.IterateAndClear(entity => _dispatchModified.Add(entity));
                }

                // process entities that were added to the system
                for (int i = 0; i < _shared.AddedEntities.Count; ++i) {
                    RuntimeEntity added = _shared.AddedEntities[i];
                    if (_entityCache.UpdateCache(added) == EntityCache.CacheChangeResult.Added) {
                        DoAdd(added);
                        _dispatchAdded.Add(added);
                    }
                }
                PerformanceData.AddedTicks = stopwatch.ElapsedTicks;

                // process state changes
                for (int i = 0; i < _shared.StateChangedEntities.Count; ++i) { // immutable
                    RuntimeEntity stateChanged = _shared.StateChangedEntities[i];
                    switch (_entityCache.UpdateCache(stateChanged)) {
                        case EntityCache.CacheChangeResult.Added:
                            DoAdd(stateChanged);
                            _dispatchAdded.Add(stateChanged);
                            break;
                        case EntityCache.CacheChangeResult.Removed:
                            DoRemove(stateChanged);
                            _dispatchRemoved.Add(stateChanged);
                            break;
                    }
                }
                PerformanceData.StateChangeTicks = stopwatch.ElapsedTicks - PerformanceData.AddedTicks;

                // process entities that were removed from the system
                for (int i = 0; i < _shared.RemovedEntities.Count; ++i) {
                    RuntimeEntity removed = _shared.RemovedEntities[i];
                    if (_entityCache.Remove(removed)) {
                        DoRemove(removed);
                        _dispatchRemoved.Add(removed);
                    }
                }
                PerformanceData.RemovedTicks = stopwatch.ElapsedTicks - PerformanceData.AddedTicks - PerformanceData.StateChangeTicks;
            }
            catch (Exception e) {
                _shared.Exceptions.Add(e);
            }
            finally {
                PerformanceData.BookkeepingTicks = stopwatch.ElapsedTicks;
            }
        }

        /// <summary>
        /// Dispatches notifications to the system. All MultithreadedSystems run this function in
        /// parallel. There are no guarantees as to the state of the external world when this
        /// function is executing, as client code is being called.
        /// </summary>
        /// <param name="input">The structured input that should be delivered to the client
        /// system.</param>
        public void RunSystem(List<IGameInput> input) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                // dispatch all added entities
                if (_triggerAdded != null) {
                    for (int i = 0; i < _dispatchAdded.Count; ++i) {
                        _triggerAdded.OnAdded(_dispatchAdded[i]);
                    }
                }
                _dispatchAdded.Clear();

                // dispatch all removed entities
                if (_triggerRemoved != null) {
                    for (int i = 0; i < _dispatchRemoved.Count; ++i) {
                        _triggerRemoved.OnRemoved(_dispatchRemoved[i]);
                    }
                }
                _dispatchRemoved.Clear();

                // dispatch all modified entities
                if (_triggerModified != null) {
                    for (int i = 0; i < _dispatchModified.Count; ++i) {
                        IEntity entity = _dispatchModified[i];
                        if (_filter.ModificationCheck(entity)) {
                            _triggerModified.OnModified(entity);
                        }
                    }
                }
                PerformanceData.ModificationTicks = stopwatch.ElapsedTicks - PerformanceData.StateChangeTicks - PerformanceData.RemovedTicks - PerformanceData.AddedTicks;

                // call the global pre-update method, if applicable
                if (_triggerGlobalPreUpdate != null) {
                    _triggerGlobalPreUpdate.OnGlobalPreUpdate();
                }

                // call the update method, if applicable
                if (_triggerUpdate != null) {
                    for (int i = 0; i < _entityCache.CachedEntities.Length; ++i) {
                        IEntity updated = _entityCache.CachedEntities[i];
                        _triggerUpdate.OnUpdate(updated);
                    }
                }
                PerformanceData.UpdateTicks = stopwatch.ElapsedTicks - PerformanceData.ModificationTicks - PerformanceData.StateChangeTicks - PerformanceData.RemovedTicks - PerformanceData.AddedTicks;

                // call the global post-update method, if applicable
                if (_triggerGlobalPostUpdate != null) {
                    _triggerGlobalPostUpdate.OnGlobalPostUpdate();
                }

                // call input methods, if applicable
                if (_triggerInput != null) {
                    for (int i = 0; i < input.Count; ++i) {
                        // see if one of the input types match the given input
                        bool accept = false;
                        for (int j = 0; j < _triggerInput.InputTypes.Length; ++j) {
                            if (_triggerInput.InputTypes[j].IsInstanceOfType(input[i])) {
                                accept = true;
                                break;
                            }
                        }

                        // accept is true when one of the input types matched; aka, we execute
                        // OnInput
                        if (accept) {
                            for (int j = 0; j < _entityCache.CachedEntities.Length; ++j) {
                                IEntity entity = _entityCache.CachedEntities[j];
                                _triggerInput.OnInput(input[i], entity);
                            }
                        }
                    }
                }

                // call global input, if applicable
                if (_triggerGlobalInput != null) {
                    for (int i = 0; i < input.Count; ++i) {
                        bool accept = false;
                        for (int j = 0; j < _triggerGlobalInput.InputTypes.Length; ++j) {
                            if (_triggerGlobalInput.InputTypes[j].IsInstanceOfType(input[i])) {
                                accept = true;
                                break;
                            }
                        }

                        if (accept) {
                            _triggerGlobalInput.OnGlobalInput(input[i]);
                        }
                    }
                }
            }
            finally {
                PerformanceData.RunSystemTicks = stopwatch.ElapsedTicks;
            }
        }

        /// <summary>
        /// Caches entities which pass a filter inside of an unordered list.
        /// </summary>
        private class EntityCache {
            /// <summary>
            /// Key used for retrieving metadata to store items in CachedEntities
            /// </summary>
            private int _metadataKey;

            /// <summary>
            /// The filter that the trigger is using
            /// </summary>
            private Filter _filter;

            /// <summary>
            /// The list of entities which are currently in the system.
            /// </summary>
            public UnorderedList<IEntity> CachedEntities;

            /// <summary>
            /// Creates a new system. Entities are added to the system based on if they pass the
            /// given filter.
            /// </summary>
            public EntityCache(Filter filter) {
                _filter = filter;
                _metadataKey = EntityManagerMetadata.GetUnorderedListMetadataIndex();

                CachedEntities = new UnorderedList<IEntity>();
            }

            /// <summary>
            /// The result of an UpdateCache operation.
            /// </summary>
            public enum CacheChangeResult {
                Added,
                Removed,
                NoChange
            }

            /// <summary>
            /// Updates the status of the entity inside of the cache; ie, if the entity is now
            /// passing the filter but was not before, then it will be added to the cache.
            /// </summary>
            /// <returns>The change in cache status for the entity</returns>
            public CacheChangeResult UpdateCache(RuntimeEntity entity) {
                UnorderedListMetadata metadata = GetMetadata(entity);

                bool passed = _filter.Check(entity);
                bool contains = CachedEntities.Contains(entity, metadata);

                // The entity is not in the cache it now passes the filter, so add it to the cache
                if (contains == false && passed) {
                    CachedEntities.Add(entity, metadata);
                    return CacheChangeResult.Added;
                }

                // The entity is in the cache but it no longer passes the filter, so remove it
                if (contains && passed == false) {
                    CachedEntities.Remove(entity, metadata);
                    return CacheChangeResult.Removed;
                }

                // no change to the cache
                return CacheChangeResult.NoChange;
            }

            /// <summary>
            /// Ensures that an Entity is not in the cache.
            /// </summary>
            /// <returns>True if the entity was previously in the cache and was removed, false if it
            /// was not in the cache and was therefore not removed.</returns>
            public bool Remove(RuntimeEntity entity) {
                if (CachedEntities.Remove(entity, GetMetadata(entity))) {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns the CachedEntities metadata for the given entity.
            /// </summary>
            private UnorderedListMetadata GetMetadata(RuntimeEntity entity) {
                // get our unordered list metadata or create it
                UnorderedListMetadata metadata;
                if (entity.Metadata.UnorderedListMetadata.TryGetValue(_metadataKey, out metadata) == false) {
                    metadata = new UnorderedListMetadata();
                    entity.Metadata.UnorderedListMetadata[_metadataKey] = metadata;
                }

                return metadata;
            }
        }

        public void Dispose() {
            _notifiedModifiedEntities.Dispose();
        }
    }
}