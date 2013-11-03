using Neon.Collections;

namespace Neon.Entities {
    /// <summary>
    /// Internal storage format for ISystems that perform better with caching.
    /// </summary>
    internal class System {
        private MetadataKey _metadataKey;

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
        /// The filter that the trigger is using
        /// </summary>
        public Filter Filter;

        /// <summary>
        /// Creates a new system. Entities are added to the system based on if they pass the given
        /// filter.
        /// </summary>
        public System(ITriggerBaseFilter trigger) {
            Filter = new Filter(DataAccessorFactory.MapTypesToDataAccessors(trigger.ComputeEntityFilter()));
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
        /// Adds the entity to the list of cached entities if it passes the trigger without invoking triggers.
        /// </summary>
        /// <param name="entity">The entity to attempt to add to the cache.</param>
        /// <returns>True if the entity was added to the cache; false otherwise.</returns>
        public bool Restore(IEntity entity) {
            if (Filter.Check(entity)) {
                CachedEntities.Add(entity, GetMetadata(entity));
                return true;
            }

            return false;
        }

        private UnorderedListMetadata GetMetadata(IEntity entity) {
            // get our unordered list metadata or create it
            UnorderedListMetadata metadata = (UnorderedListMetadata)entity.Metadata[_metadataKey];
            if (metadata == null) {
                metadata = new UnorderedListMetadata();
                entity.Metadata[_metadataKey] = metadata;
            }

            return metadata;
        }

        /// <summary>
        /// Updates the status of the entity inside of the cache; ie, if the entity is now passing
        /// the filter but was not before, then it will be added to the cache.
        /// </summary>
        /// <returns>The change in cache status for the entity</returns>
        public CacheChangeResult UpdateCache(IEntity entity) {
            UnorderedListMetadata metadata = GetMetadata(entity);

            bool passed = Filter.Check(entity);
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
        /// <returns>True if the entity was previously in the cache and was removed, false if it was
        /// not in the cache and was therefore not removed.</returns>
        public bool Remove(IEntity entity) {
            if (CachedEntities.Remove(entity, (UnorderedListMetadata)entity.Metadata[_metadataKey])) {
                if (_removedTrigger != null) {
                    _removedTrigger.OnRemoved(entity);
                }
                return true;
            }
            return false;
        }
    }
}