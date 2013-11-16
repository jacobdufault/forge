using Neon.Collections;

namespace Neon.Entities {
    /// <summary>
    /// An interface of Entity operations that allow the entity to be both queried and written to.
    /// </summary>
    public interface IEntity : IQueryableEntity {
        /// <summary>
        /// Destroys the entity. The entity is not destroyed immediately, but instead at the end of
        /// the next update loop. Systems will get a chance to process the destruction of the
        /// entity.
        /// </summary>
        void Destroy();

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
    }
}