using Neon.Collections;
using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An interface of Entity operations that only query the current state of the entity.
    /// </summary>
    public interface IQueryableEntity {
        /// <summary>
        /// Selects that data inside of the entity that passes the given filter.
        /// </summary>
        /// <param name="filter">The predicate to check items to see if they should be contained
        /// inside of the result.</param>
        /// <param name="storage">An optional collection to append result to, instead of creating a
        /// new one. The collection will *not* be cleared by this method.</param>
        /// <returns>A list of data instances that pass the filter.</returns>
        ICollection<Data> SelectCurrentData(Predicate<Data> filter = null, ICollection<Data> storage = null);

        /// <summary>
        /// Gets the event processor that Systems use to notify the external world of interesting
        /// events.
        /// </summary>
        EventProcessor EventProcessor {
            get;
        }

        /// <summary>
        /// Gets the current data value for the given type.
        /// </summary>
        Data Current(DataAccessor accessor);

        /// <summary>
        /// Gets the previous data value for the data type.
        /// </summary>
        Data Previous(DataAccessor accessor);

        /// <summary>
        /// Checks to see if this Entity contains the given type of data and if that data can be
        /// modified.
        /// </summary>
        /// <remarks>
        /// Interestingly, if the data has been removed, ContainsData will return false but Current
        /// will return an instance (though Previous and Modify will both throw
        /// exceptions) .
        /// </remarks>
        bool ContainsData(DataAccessor accessor);

        /// <summary>
        /// Returns if the Entity was modified in the previous update.
        /// </summary>
        bool WasModified(DataAccessor accessor);

        /// <summary>
        /// The unique id for the entity. This unique-id is relative to the actual implementation
        /// type for the entity. For example, if there are two separate types which implement
        /// IQueryableEntity, each can share unique ids, but w.r.t. their own types, the ids will be
        /// unique.
        /// </summary>
        int UniqueId {
            get;
        }

        /// <summary>
        /// A non-unique string that represents a "human readable" name for the entity. This carries
        /// no weight in the simulation, and is only meant for diagnostics.
        /// </summary>
        string PrettyName {
            get;
            set;
        }

        /// <summary>
        /// Metadata container that allows arbitrary data to be stored within the Entity.
        /// </summary>
        /// <remarks>
        /// Use QueryableEntity.MetadataRegistry to retrieve keys for this Metadata container.
        /// </remarks>
        MetadataContainer<object> Metadata {
            get;
        }
    }

    public static class QueryableEntity {
        /// <summary>
        /// Used to retrieve keys for storing things in instance-specific metadata containers.
        /// </summary>
        public static MetadataRegistry MetadataRegistry = new MetadataRegistry();
    }
}