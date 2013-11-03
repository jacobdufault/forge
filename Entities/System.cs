using Neon.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Neon.Entities {
    /// <summary>
    /// Shared state between all multithreaded systems.
    /// </summary>
    internal interface MultithreadedSystemSharedContext {
        /// <summary>
        /// Global singleton entity.
        /// </summary>
        IEntity SingletonEntity { get; }

        /// <summary>
        /// Entities which have been added.
        /// </summary>
        List<Entity> AddedEntities { get; }

        /// <summary>
        /// Entities which have been removed.
        /// </summary>
        List<Entity> RemovedEntities { get; }

        /// <summary>
        /// Entities which have state changes.
        /// </summary>
        List<Entity> StateChangedEntities { get; }

        /// <summary>
        /// Event the system uses to notify the primary thread that it is done processing.
        /// </summary>
        CountdownEvent SystemDoneEvent { get; }
    }

    /// <summary>
    /// Runs an ISystem in another thread.
    /// </summary>
    internal class MultithreadedSystem {
        /// <summary>
        /// Entities which were modified last update. When the system does bookkeeping work, this
        /// will be swapped with _nextModifiedEntities.
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

        /// <summary>
        /// A cache of all entities which have passed the entity filter.
        /// </summary>
        private EntityCache _entityCache;

        /// <summary>
        /// Our shared context.
        /// </summary>
        private MultithreadedSystemSharedContext _shared;

        // Cached triggers
        private ITriggerAdded _triggerAdded;
        private ITriggerRemoved _triggerRemoved;
        private ITriggerModified _triggerModified;
        private ITriggerGlobalPreUpdate _triggerGlobalPreUpdate;
        private ITriggerUpdate _triggerUpdate;
        private ITriggerGlobalPostUpdate _triggerGlobalPostUpdate;
        private ITriggerInput _triggerInput;

        public ITriggerBaseFilter Trigger;

        private Filter _filter;

        internal MultithreadedSystem(MultithreadedSystemSharedContext sharedData, ITriggerBaseFilter trigger, List<Entity> entitiesWithModifications) {
            Trigger = trigger;

            _shared = sharedData;

            _filter = new Filter(DataAccessorFactory.MapTypesToDataAccessors(trigger.ComputeEntityFilter()));
            _entityCache = new EntityCache(_filter);

            _triggerAdded = trigger as ITriggerAdded;
            _triggerRemoved = trigger as ITriggerRemoved;
            _triggerModified = trigger as ITriggerModified;
            _triggerGlobalPreUpdate = trigger as ITriggerGlobalPreUpdate;
            _triggerUpdate = trigger as ITriggerUpdate;
            _triggerGlobalPostUpdate = trigger as ITriggerGlobalPostUpdate;
            _triggerInput = trigger as ITriggerInput;

            foreach (var entity in entitiesWithModifications) {
                if (_filter.Check(entity)) {
                    _nextModifiedEntities.Append(entity);
                }
            }
        }

        public void Restore(IEntity entity) {
            if (_entityCache.UpdateCache(entity) == EntityCache.CacheChangeResult.Added) {
                DoAdd(entity);
            }
        }

        private void DoAdd(IEntity added) {
            if (_triggerModified != null) {
                ((Entity)added).ModificationNotifier.Listener += ModificationNotifier_Listener;
            }
            if (_triggerAdded != null) {
                _triggerAdded.OnAdded(added);
            }
        }

        private void DoRemove(IEntity removed) {
            // if we removed an entity from the cache, then we don't want to hear of any more
            // modification events
            if (_triggerModified != null) {
                ((Entity)removed).ModificationNotifier.Listener -= ModificationNotifier_Listener;

                _modifiedEntities.Remove(removed);
                _removedMutableEntities.Add(removed);
            }
            if (_triggerRemoved != null) {
                _triggerRemoved.OnRemoved(removed);
            }
        }

        public void BookkeepingBeforeRunningSystems() {
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
                _shared.SystemDoneEvent.Signal();

                BookkeepingTicks = stopwatch.ElapsedTicks;
                //stopwatch.Stop();
                //Log<EntityManager>.Info("[AFT] Running bookkeeping on {0} took {1} ticks", _system.Trigger.GetType(), stopwatch.ElapsedTicks);
            }
        }

        /// <summary>
        /// Total number of ticks running the system required.
        /// </summary>
        public long RunSystemTicks;

        /// <summary>
        /// Total number of bookkeeping ticks required.
        /// </summary>
        public long BookkeepingTicks;

        /// <summary>
        /// Ticks required for adding entities when running the system.
        /// </summary>
        public long AddedTicks;

        /// <summary>
        /// Ticks required for removing entities when running the system.
        /// </summary>
        public long RemovedTicks;

        /// <summary>
        /// Ticks required for state change operations when running the system.
        /// </summary>
        public long StateChangeTicks;

        /// <summary>
        /// Ticks required for modification operations when running the system.
        /// </summary>
        public long ModificationTicks;

        /// <summary>
        /// Ticks required for updating the system.
        /// </summary>
        public long UpdateTicks;

        public void RunSystem(object input) {
            RunSystem((List<IStructuredInput>)input);
        }

        public void RunSystem(List<IStructuredInput> input) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                // process entities that were added to the system
                int addedCount = _shared.AddedEntities.Count; // immutable
                for (int i = 0; i < addedCount; ++i) {
                    IEntity added = _shared.AddedEntities[i];
                    if (_entityCache.UpdateCache(added) == EntityCache.CacheChangeResult.Added) {
                        DoAdd(added);
                    }
                }
                AddedTicks = stopwatch.ElapsedTicks;

                // process entities that were removed from the system
                int removedCount = _shared.RemovedEntities.Count; // immutable
                for (int i = 0; i < removedCount; ++i) {
                    IEntity removed = _shared.RemovedEntities[i];
                    if (_entityCache.Remove(removed)) {
                        DoRemove(removed);
                    }
                }
                RemovedTicks = stopwatch.ElapsedTicks - AddedTicks;

                // process state changes
                for (int i = 0; i < _shared.StateChangedEntities.Count; ++i) { // immutable
                    IEntity stateChanged = _shared.StateChangedEntities[i];
                    EntityCache.CacheChangeResult change = _entityCache.UpdateCache(stateChanged);
                    if (change == EntityCache.CacheChangeResult.Added) {
                        DoAdd(stateChanged);
                    }
                    else if (change == EntityCache.CacheChangeResult.Removed) {
                        DoRemove(stateChanged);
                    }
                }
                StateChangeTicks = stopwatch.ElapsedTicks - RemovedTicks - AddedTicks;

                // process modifications
                if (_triggerModified != null) {
                    for (int i = 0; i < _modifiedEntities.Length; ++i) {
                        IEntity entity = _modifiedEntities[i];
                        if (_filter.ModificationCheck(entity)) {
                            _triggerModified.OnModified(entity);
                        }
                    }
                }
                ModificationTicks = stopwatch.ElapsedTicks - StateChangeTicks - RemovedTicks - AddedTicks;

                // run update methods Call the BeforeUpdate methods - *user code*
                if (_triggerGlobalPreUpdate != null) {
                    _triggerGlobalPreUpdate.OnGlobalPreUpdate(_shared.SingletonEntity);
                }

                if (_triggerUpdate != null) {
                    for (int i = 0; i < _entityCache.CachedEntities.Length; ++i) {
                        IEntity updated = _entityCache.CachedEntities[i];
                        _triggerUpdate.OnUpdate(updated);
                    }
                }
                UpdateTicks = stopwatch.ElapsedTicks - ModificationTicks - StateChangeTicks - RemovedTicks - AddedTicks;

                if (_triggerGlobalPostUpdate != null) {
                    _triggerGlobalPostUpdate.OnGlobalPostUpdate(_shared.SingletonEntity);
                }

                // process input
                if (_triggerInput != null) {
                    for (int i = 0; i < input.Count; ++i) {
                        if (_triggerInput.IStructuredInputType.IsInstanceOfType(input[i])) {
                            for (int j = 0; j < _entityCache.CachedEntities.Length; ++j) {
                                IEntity entity = _entityCache.CachedEntities[j];
                                if (entity.Enabled) {
                                    _triggerInput.OnInput(input[i], entity);
                                }
                            }
                        }
                    }
                }
            }
            finally {
                _shared.SystemDoneEvent.Signal();
                RunSystemTicks = stopwatch.ElapsedTicks;
            }
        }

        private void ModificationNotifier_Listener(Entity entity) {
            lock (_nextModifiedEntities) {
                _nextModifiedEntities.Append(entity);
            }
        }


        /// <summary>
        /// Caches entities which pass a filter inside of an unordered list.
        /// </summary>
        private class EntityCache {
            /// <summary>
            /// Key used for retrieving metadata to store items in CachedEntities
            /// </summary>
            private MetadataKey _metadataKey;

            /// <summary>
            /// The filter that the trigger is using
            /// </summary>
            private Filter _filter;

            /// <summary>
            /// The list of entities which are currently in the system.
            /// </summary>
            public UnorderedList<IEntity> CachedEntities;

            /// <summary>
            /// Creates a new system. Entities are added to the system based on if they pass the given
            /// filter.
            /// </summary>
            public EntityCache(Filter filter) {
                _filter = filter;
                _metadataKey = Entity.MetadataRegistry.GetKey();

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
            /// Updates the status of the entity inside of the cache; ie, if the entity is now passing
            /// the filter but was not before, then it will be added to the cache.
            /// </summary>
            /// <returns>The change in cache status for the entity</returns>
            public CacheChangeResult UpdateCache(IEntity entity) {
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
            public bool Remove(IEntity entity) {
                if (CachedEntities.Remove(entity, (UnorderedListMetadata)entity.Metadata[_metadataKey])) {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns the CachedEntities metadata for the given entity.
            /// </summary>
            private UnorderedListMetadata GetMetadata(IEntity entity) {
                // get our unordered list metadata or create it
                UnorderedListMetadata metadata = (UnorderedListMetadata)entity.Metadata[_metadataKey];
                if (metadata == null) {
                    metadata = new UnorderedListMetadata();
                    entity.Metadata[_metadataKey] = metadata;
                }

                return metadata;
            }
        }

    }
}