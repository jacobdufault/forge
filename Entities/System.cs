using Neon.Collections;

namespace Neon.Entities {
    /// <summary>
    /// Internal storage format for ISystems that perform better with caching.
    /// </summary>
    internal class System {
        private MetadataKey _metadataKey;
        private Filter _filter;

        /// <summary>
        /// Trigger to invoke when an entity has been added to the cache.
        /// </summary>
        private ITriggerAdded _addedTrigger;

        /// <summary>
        /// Trigger to invoke when an entity has been removed from the cache.
        /// </summary>
        private ITriggerRemoved _removedTrigger;

        /// <summary>
        /// The list of entities which are currently in the system.
        /// </summary>
        public UnorderedList<IEntity> CachedEntities;

        /// <summary>
        /// The trigger used for the system.
        /// </summary>
        public ITriggerBaseFilter Trigger;

        /// <summary>
        /// Creates a new system. Entities are added to the system based on if they pass the given
        /// filter.
        /// </summary>
        public System(ITriggerBaseFilter trigger) {
            _filter = new Filter(DataAccessorFactory.MapTypesToDataAccessors(trigger.ComputeEntityFilter()));
            _metadataKey = Entity.MetadataRegistry.GetKey();

            _addedTrigger = trigger as ITriggerAdded;
            _removedTrigger = trigger as ITriggerRemoved;

            Trigger = trigger;
            CachedEntities = new UnorderedList<IEntity>();
        }

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
            // get our unordered list metadata or create it
            UnorderedListMetadata metadata = (UnorderedListMetadata)entity.Metadata[_metadataKey];
            if (metadata == null) {
                metadata = new UnorderedListMetadata();
                entity.Metadata[_metadataKey] = metadata;
            }

            bool passed = _filter.Check(entity);
            bool contains = CachedEntities.Contains(entity, metadata);

            // The entity is not in the cache it now passes the filter, so add it to the cache
            if (contains == false && passed) {
                CachedEntities.Add(entity, metadata);
                if (_addedTrigger != null) {
                    _addedTrigger.OnAdded(entity);
                }

                return CacheChangeResult.Added;
            }

            // The entity is in the cache but it no longer passes the filter, so remove it
            if (contains && passed == false) {
                CachedEntities.Remove(entity, metadata);
                if (_removedTrigger != null) {
                    _removedTrigger.OnRemoved(entity);
                }

                return CacheChangeResult.Removed;
            }

            // no change to the cache
            return CacheChangeResult.NoChange;
        }

        /// <summary>
        /// Ensures that an Entity is not in the cache.
        /// </summary>
        public void Remove(IEntity entity) {
            if (CachedEntities.Remove(entity, (UnorderedListMetadata)entity.Metadata[_metadataKey])) {
                if (_removedTrigger != null) {
                    _removedTrigger.OnRemoved(entity);
                }
            }
        }
    }
}