using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Maps different types of Data to a sequential set of integers.
    /// </summary>
    internal static class DataAccessorFactory {
        /// <summary>
        /// Maps a set of types to their respective DataAccessors.
        /// </summary>
        /// <param name="types">The types to map</param>
        /// <returns>A list of DataAccessors that have a 1 to 1 correspondence to the given
        /// types.</returns>
        public static DataAccessor[] MapTypesToDataAccessors(Type[] types) {
            DataAccessor[] accessors = new DataAccessor[types.Length];
            for (int i = 0; i < types.Length; ++i) {
                accessors[i] = new DataAccessor(types[i]);
            }
            return accessors;
        }

        /// <summary>
        /// The next integer to use.
        /// </summary>
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        /// <summary>
        /// The mapping from Data types to their integers
        /// </summary>
        private static Dictionary<Type, int> _ids = new Dictionary<Type, int>();

        /// <summary>
        /// Returns the id for the given data type. Forwards the call to GetId(Type).
        /// </summary>
        public static int GetId(IData data) {
            return GetId(data.GetType());
        }

        /// <summary>
        /// Returns the identifier/integer for the given type, constructing if it necessary.
        /// </summary>
        /// <param name="type">The type to get.</param>
        /// <returns>The identifier/integer</returns>
        public static int GetId(Type type) {
            Contract.Requires(typeof(IData).IsAssignableFrom(type));

            if (_ids.ContainsKey(type) == false) {
                _ids[type] = _idGenerator.Next();
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
        /// <returns>The type used for the given accessor, or an exception if there is
        /// none.</returns>
        public static Type GetTypeFromAccessor(DataAccessor accessor) {
            foreach (var item in _ids) {
                if (item.Value == accessor.Id) {
                    return item.Key;
                }
            }

            throw new Exception("No such type of accessor=" + accessor.Id);
        }
    }
}