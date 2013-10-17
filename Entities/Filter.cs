using Neon.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Neon.Entities {
    public class Filter {
        /// <summary>
        /// The list of Data types that the filter has to have.
        /// </summary>
        private DataAccessor[] _accessors;

        /// <summary>
        /// Creates a new filter from a set of predicates. For the filter to
        /// be true, every single predicate must also be true.
        /// </summary>
        /// <param name="predicates">The predicates</param>
        public Filter(params DataAccessor[] accessors) {
            _accessors = accessors;
        }

        /// <summary>
        /// Check the given Entity to see if it passes this Filter.
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <returns>True if it passes the filter, false otherwise.</returns>
        public bool Check(IEntity entity) {
            for (int i = 0; i < _accessors.Length; ++i) {
                if (entity.ContainsData(_accessors[i]) == false || entity.WasRemoved(_accessors[i])) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check to see if the entity has any modifications that this filter cares about.
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <returns>If it has a modification that the filter is interested in</returns>
        public bool ModificationCheck(IEntity entity) {
            for (int i = 0; i < _accessors.Length; ++i) {
                if (entity.WasModified(_accessors[i])) {
                    return true;
                }
            }

            return false;
        }
    }
}
