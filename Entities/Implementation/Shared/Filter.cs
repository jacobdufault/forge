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

namespace Forge.Entities.Implementation.Shared {
    /// <summary>
    /// A filter ensures that an entity contains a set of data types.
    /// </summary>
    internal class Filter {
        /// <summary>
        /// The list of Data types that the filter has to have.
        /// </summary>
        private DataAccessor[] _accessors;

        /// <summary>
        /// Creates a new filter that has to contain a given set of DataAccessors.
        /// </summary>
        /// <param name="accessors">The required data types that an entity must contain to pass the
        /// filter.</param>
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
                if (entity.ContainsData(_accessors[i]) == false) {
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