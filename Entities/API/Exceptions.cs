using System;

namespace Neon.Entities {
    /// <summary>
    /// Exception thrown when a data type is added to an entity, but the entity already contains an
    /// instance of said data type.
    /// </summary>
    [Serializable]
    public class AlreadyAddedDataException : Exception {
        /// <summary>
        /// Creates the exception with the given context and data type.
        /// </summary>
        /// <param name="context">The entity that triggered the exception.</param>
        /// <param name="type">The data type that was already added.</param>
        internal AlreadyAddedDataException(IEntity context, Type type)
            : base("The entity already has a data instance for type=" + type + " in " + context) {
        }
    }

    /// <summary>
    /// Exception thrown when data is attempted to be retrieved from an Entity, but the entity does
    /// not contain an instance of said data type.
    /// </summary>
    [Serializable]
    public class NoSuchDataException : Exception {
        /// <summary>
        /// Creates the exception with the given context and data type.
        /// </summary>
        /// <param name="context">The entity that triggered the exception.</param>
        /// <param name="accessor">The data type that the entity lacks.</param>
        internal NoSuchDataException(IQueryableEntity context, DataAccessor accessor)
            : base("No such data for type=" + DataAccessorFactory.GetTypeFromAccessor(accessor) +
            " in " + context) {
        }
    }

    /// <summary>
    /// An exception that is thrown when a data instance has been modified more than once in an
    /// update loop, but that data is not allowed to be concurrently modified.
    /// </summary>
    [Serializable]
    public class RemodifiedDataException : Exception {
        /// <summary>
        /// Creates the exception with the given context and data type.
        /// </summary>
        /// <param name="context">The entity that triggered the exception.</param>
        /// <param name="type">The data type that was concurrently modified.</param>
        internal RemodifiedDataException(IEntity context, Type type)
            : base("Already modified data for type=" + type + " in " + context) {
        }
    }
}