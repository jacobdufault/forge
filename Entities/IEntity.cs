using Neon.Collections;
using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An Entity contains some data.
    /// </summary>
    public interface IEntity {
        /// <summary>
        /// Selects that data inside of the entity that passes the given filter.
        /// </summary>
        /// <param name="filter">The predicate to check items to see if they should be contained
        /// inside of the result.</param>
        /// <param name="storage">An optional collection to append result to, instead of creating a
        /// new one. The collection will *not* be cleared by this method.</param>
        /// <returns>A list of data instances that pass the filter.</returns>
        ICollection<Data> SelectCurrentData(Predicate<Data> filter, ICollection<Data> storage = null);

        /// <summary>
        /// Destroys the entity. The entity is not destroyed immediately, but instead at the end of
        /// the next update loop. Systems will get a chance to process the destruction of the
        /// entity.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets the event processor that Systems use to notify the external world of interesting
        /// events.
        /// </summary>
        EventProcessor EventProcessor {
            get;
        }

        /// <summary>
        /// Adds the given data type, or modifies an instance of it.
        /// </summary>
        /// <returns>A modifiable instance of data of type T</returns>
        Data AddOrModify(DataAccessor accessor);

        /// <summary>
        /// Add a Data instance of with the given accessor to the Entity.
        /// </summary>
        /// <remarks>
        /// The aux allocators will be called in this method, giving them a chance to populate the
        /// data with any necessary information.
        /// </remarks>
        /// <param name="accessor"></param>
        /// <returns>The data instance that can be used to initialize the data</returns>
        Data AddData(DataAccessor accessor);

        /// <summary>
        /// Removes the given data type from the entity.
        /// </summary>
        /// <remarks>
        /// The data instance is not removed in this frame, but in the next one. In the next frame,
        /// Previous and Modify will both throw NoSuchData exceptions, but Current will return the
        /// current data instance.
        /// </remarks>
        void RemoveData(DataAccessor accessor);

        /// <summary>
        /// Modify the given data instance. The current and previous values are still accessible.
        /// Please note that a data instance can only be modified once; an exception is thrown if
        /// one instance is modified multiple times.
        /// </summary>
        /// <param name="accessor">The data type to modify.</param>
        /// <param name="force">If the modification should be forced; ie, if there is already a
        /// modification then it will be overwritten. This should *NEVER* be used in systems or
        /// general client code; it is available for inspector GUI changes.</param>
        Data Modify(DataAccessor accessor, bool force = false);

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
        /// Metadata container that allows arbitrary data to be stored within the Entity.
        /// </summary>
        MetadataContainer<object> Metadata {
            get;
        }

        /// <summary>
        /// The unique id for the entity
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
        }
    }
}