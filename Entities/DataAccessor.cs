using Neon.Utility;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Entities {
    /// <summary>
    /// Maps different types of Data to a sequential set of integers.
    /// </summary>
    internal static class DataFactory {
        /// <summary>
        /// The next integer to use.
        /// </summary>
        private static int _nextId = 0;

        /// <summary>
        /// The mapping from Data types to their integers
        /// </summary>
        private static Dictionary<Type, int> _ids = new Dictionary<Type, int>();

        /// <summary>
        /// Returns the identifier/integer for the given type, constructing if it necessary.
        /// </summary>
        /// <param name="type">The type to get.</param>
        /// <returns>The identifier/integer</returns>
        public static int GetId(Type type) {
            Contract.Requires(typeof(Data).IsAssignableFrom(type));

            if (_ids.ContainsKey(type) == false) {
                _ids[type] = Interlocked.Increment(ref _nextId);
            }

            return _ids[type];
        }

        /// <summary>
        /// Looks up the type of data is used for the given accessor.
        /// </summary>
        /// <remarks>
        /// The current implementation of this method runs in O(n) time (instead of O(1)).
        /// </remarks>
        /// <param name="accessor">The data accessor.</param>
        /// <returns>The type used for the given accessor, or an exception if there is none.</returns>
        public static Type GetTypeFromAccessor(DataAccessor accessor) {
            foreach (var item in _ids) {
                if (item.Value == accessor.Id) {
                    return item.Key;
                }
            }

            throw new Exception("No such type of accessor=" + accessor.Id);
        }
    }

    /// <summary>
    /// Provides a convenient and efficient way to access a type of Data.
    /// </summary>
    public struct DataAccessor {
        /// <summary>
        /// Creates a new DataAccessor that accesses the specified Data type.
        /// </summary>
        /// <param name="dataType">The type of Data to retrieve; note that this parameter must be a subtype of Data</param>
        public DataAccessor(Type dataType)
            : this() {
            Id = DataFactory.GetId(dataType);
        }

        /// <summary>
        /// Directly construct a DataAccessor with the given id.
        /// </summary>
        /// <param name="id">The id of the DataAccessor</param>
        internal DataAccessor(int id)
            : this() {
            Id = id;
        }

        /// <summary>
        /// Returns the mapped id for this accessor.
        /// </summary>
        public int Id {
            get;
            private set;
        }
    }

    /// <summary>
    /// Map a data type to its respective accessor, at compile time.
    /// </summary>
    /// <typeparam name="T">The type of Data to map.</typeparam>
    public class DataMap<T> where T : Data {
        static DataMap() {
            Accessor = new DataAccessor(typeof(T));
        }

        /// <summary>
        /// Gets the accessor for the specified data type.
        /// </summary>
        public static DataAccessor Accessor {
            get;
            private set;
        }
    }
}