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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Forge.Entities {
    /// <summary>
    /// An entity within the game state that can be queried for information about its current data.
    /// </summary>
    [JsonConverter(typeof(QueryableEntityConverter))]
    public interface IQueryableEntity {
        /// <summary>
        /// Selects data inside of the entity that passes the given filter.
        /// </summary>
        /// <param name="includeRemoved">Should data that has been removed, but is still in the
        /// queryable entity, be considered for selection? This means that Contains(accessor) will
        /// return false but WasRemoved(accessor) will return true.</param>
        /// <param name="filter">The predicate to check items to see if they should be contained
        /// inside of the result.</param>
        /// <param name="storage">An optional collection to append result to, instead of creating a
        /// new one. The collection will *not* be cleared by this method.</param>
        /// <returns>A list of data instances that pass the filter.</returns>
        ICollection<DataAccessor> SelectData(bool includeRemoved = false,
            Predicate<DataAccessor> filter = null, ICollection<DataAccessor> storage = null);

        /// <summary>
        /// Gets the current data value for the given type.
        /// </summary>
        Data.IData Current(DataAccessor accessor);

        /// <summary>
        /// Gets the previous data value for the data type.
        /// </summary>
        Data.Versioned Previous(DataAccessor accessor);

        /// <summary>
        /// Checks to see if this Entity contains the given type of data and if that data can be
        /// modified.
        /// </summary>
        /// <remarks>
        /// Interestingly, if the data has been removed, ContainsData will return false but Current
        /// will return an instance (though Previous and Modify will both throw exceptions).
        /// </remarks>
        bool ContainsData(DataAccessor accessor);

        /// <summary>
        /// A non-unique string that represents a "human readable" name for the entity. This carries
        /// no weight in the simulation, and is only meant for diagnostics.
        /// </summary>
        string PrettyName {
            get;
            set;
        }
    }
}