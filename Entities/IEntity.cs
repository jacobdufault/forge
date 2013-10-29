using Neon.Collections;
using System;

namespace Neon.Entities {
    /// <summary>
    /// An Entity contains some data.
    /// </summary>
    public interface IEntity {
        /// <summary>
        /// Destroys the entity. The entity is not destroyed immediately, but instead at the end of
        /// the next update loop. Systems will get a chance to process the destruction of the
        /// entity.
        /// </summary>
        void Destroy();

        [Obsolete("Use EventProcessor")]
        event Action OnShow;

        [Obsolete("Use EventProcessor")]
        event Action OnHide;

        [Obsolete("Use EventProcessor")]
        event Action OnRemoved;

        /// <summary>
        /// Gets the event processor that Systems use to notify the external world of interesting
        /// events.
        /// </summary>
        EventProcessor EventProcessor {
            get;
        }

        /// <summary>
        /// Add a Data instance of with the given accessor to the Entity.
        /// </summary>
        /// <remarks>
        /// The aux allocators will be called in this method, giving them a chance to populate the
        /// data with any necessary information.
        /// </remarks>
        /// <param name="accessor"></param>
        /// <returns>The data instance that can be used to initialize the data</returns>
        // TODO: add a test for Remove and Add in sequential frames
        Data AddData(DataAccessor accessor);

        /// <summary>
        /// Removes the given data type from the entity.
        /// </summary>
        /// <remarks>
        /// The data instance is not removed in this frame, but in the next one. In the next frame,
        /// Previous and Modify will both throw NoSuchData exceptions, but Current will return the
        /// current data instance.
        /// </remarks>
        /// <typeparam name="T">The type of data to remove</typeparam>
        // TODO: add test for Remove and Modify in the same frame
        void RemoveData(DataAccessor accessor);

        /// <summary>
        /// If Enabled is set to false, then the Entity will not be processed in any Update or
        /// StructuredInput based systems. However, modification ripples are still applied to the
        /// Entity until it has no more modifications.
        /// </summary>
        bool Enabled {
            get;
            set;
        }

        /// <summary>
        /// Modify the given data instance. The current and previous values are still accessible.
        /// Please note that a data instance can only be modified once; an exception is thrown if
        /// one instance is modified multiple times.
        /// </summary>
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
    }
}