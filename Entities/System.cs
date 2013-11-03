using Neon.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Neon.Entities {
    internal interface MultithreadedSystemSharedContext {
        IEntity SingletonEntity { get; }
        int ModifiedIndex { get; }
        List<Entity> AddedEntities { get; }
        List<Entity> RemovedEntities { get; }
        List<Entity> StateChangedEntities { get; }

        CountdownEvent SystemDoneEvent { get; }
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
        private EntityCache _entityCache;


        private ITriggerModified _modifiedTrigger;
        private ITriggerGlobalPreUpdate _globalPreUpdateTrigger;
        private ITriggerUpdate _updateTrigger;
        private ITriggerGlobalPostUpdate _globalPostUpdateTrigger;
        private ITriggerInput _inputTrigger;
        private ITriggerAdded _addedTrigger;
        private ITriggerRemoved _removedTrigger;

        public ITriggerBaseFilter Trigger;

        private Filter _filter;

        internal MultithreadedSystem(MultithreadedSystemSharedContext context, ITriggerBaseFilter trigger, List<Entity> entitiesWithModifications) {
            Trigger = trigger;

            _context = context;

            _filter = new Filter(DataAccessorFactory.MapTypesToDataAccessors(trigger.ComputeEntityFilter()));
            _entityCache = new EntityCache(_filter);

            _modifiedTrigger = trigger as ITriggerModified;
            _globalPreUpdateTrigger = trigger as ITriggerGlobalPreUpdate;
            _updateTrigger = trigger as ITriggerUpdate;
            _globalPostUpdateTrigger = trigger as ITriggerGlobalPostUpdate;
            _inputTrigger = trigger as ITriggerInput;
            _addedTrigger = trigger as ITriggerAdded;
            _removedTrigger = trigger as ITriggerRemoved;

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
            if (_modifiedTrigger != null) {
                ((Entity)added).ModificationNotifier.Listener += ModificationNotifier_Listener;
            }
            if (_addedTrigger != null) {
                _addedTrigger.OnAdded(added);
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
            if (_removedTrigger != null) {
                _removedTrigger.OnRemoved(removed);
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
                _context.SystemDoneEvent.Signal();

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

        public void RunSystem(object input) {
            RunSystem((List<IStructuredInput>)input);
        }

        public void RunSystem(List<IStructuredInput> input) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                // process entities that were added to the system
                int addedCount = _context.AddedEntities.Count;
                for (int i = 0; i < addedCount; ++i) {
                    IEntity added = _context.AddedEntities[i];
                    if (_entityCache.UpdateCache(added) == EntityCache.CacheChangeResult.Added) {
                        DoAdd(added);
                    }
                }
                AddedTicks = stopwatch.ElapsedTicks;

                // process entities that were removed from the system
                int removedCount = _context.RemovedEntities.Count;
                for (int i = 0; i < removedCount; ++i) {
                    IEntity removed = _context.RemovedEntities[i];
                    if (_entityCache.Remove(removed)) {
                        DoRemove(removed);
                    }
                }
                RemovedTicks = stopwatch.ElapsedTicks - AddedTicks;

                // process state changes
                for (int i = 0; i < _context.StateChangedEntities.Count; ++i) {
                    IEntity stateChanged = _context.StateChangedEntities[i];
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
                if (_modifiedTrigger != null) {
                    for (int i = 0; i < _modifiedEntities.Length; ++i) {
                        IEntity entity = _modifiedEntities[i];
                        if (_filter.ModificationCheck(entity)) {
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
                    for (int i = 0; i < _entityCache.CachedEntities.Length; ++i) {
                        IEntity updated = _entityCache.CachedEntities[i];
                        _updateTrigger.OnUpdate(updated);
                    }
                }
                UpdateTicks = stopwatch.ElapsedTicks - ModificationTicks - StateChangeTicks - RemovedTicks - AddedTicks;

                if (_globalPostUpdateTrigger != null) {
                    _globalPostUpdateTrigger.OnGlobalPostUpdate(_context.SingletonEntity);
                }

                // process input
                if (_inputTrigger != null) {
                    for (int i = 0; i < input.Count; ++i) {
                        if (_inputTrigger.IStructuredInputType.IsInstanceOfType(input[i])) {
                            for (int j = 0; j < _entityCache.CachedEntities.Length; ++j) {
                                IEntity entity = _entityCache.CachedEntities[j];
                                if (entity.Enabled) {
                                    _inputTrigger.OnInput(input[i], entity);
                                }
                            }
                        }
                    }
                }
            }
            finally {
                _context.SystemDoneEvent.Signal();
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