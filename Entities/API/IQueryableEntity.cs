using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An entity within the game state that can be queried for information about its current data.
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
        ICollection<IData> SelectCurrentData(Predicate<IData> filter = null,
            ICollection<IData> storage = null);

        /// <summary>
        /// Returns an event dispatcher that is used to notify external code of interesting things
        /// that have occurred during the last update, such as the removal of an entity or perhaps a
        /// data addition to an entity.
        /// </summary>
        IEventNotifier EventNotifier {
            get;
        }

        /// <summary>
        /// Gets the current data value for the given type.
        /// </summary>
        IData Current(DataAccessor accessor);

        /// <summary>
        /// Gets the previous data value for the data type.
        /// </summary>
        IData Previous(DataAccessor accessor);

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