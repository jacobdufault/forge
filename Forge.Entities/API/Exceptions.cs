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

using Forge.Entities.Implementation.Shared;
using System;

namespace Forge.Entities {
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
        /// <param name="accessor">The data type that was already added.</param>
        internal AlreadyAddedDataException(IEntity context, DataAccessor accessor)
            : base("The entity already has a data instance for type=" + accessor.DataType + " in " +
            context) {
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
            : base(string.Format("No such data for type={0} in context={1}", accessor.DataType,
            context)) {
        }
    }

    /// <summary>
    /// Exception thrown when Previous(accessor) is requested but accessor does not map to a
    /// versioned data type.
    /// </summary>
    [Serializable]
    public class PreviousRequiresVersionedDataException : Exception {
        internal PreviousRequiresVersionedDataException(IQueryableEntity context,
            DataAccessor accessor)
            : base(string.Format("Retrieving previous data requires that the data extends " +
            "Data.IVersioned, but type={0} in context={1} does not", accessor.DataType, context)) {
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
        /// <param name="accessor">The data type that was concurrently modified.</param>
        internal RemodifiedDataException(IEntity context, DataAccessor accessor)
            : base("Already modified data for type=" + accessor.DataType + " in " + context) {
        }
    }
}