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

namespace Forge.Entities {
    /// <summary>
    /// An interface of Entity operations that allow the entity to be both queried and written to.
    /// </summary>
    /// <remarks>
    /// There are numerous extension methods for this interface which make working with it easier,
    /// such as generic wrappers for automatically retrieving DataAccessors and casting to the
    /// correct return type. It is suggested that the extension methods are used instead of these
    /// more primitive ones. The primitive methods are necessary, however, when data type
    /// information is not explicitly known at compile-time.
    /// </remarks>
    [JsonConverter(typeof(QueryableEntityConverter))]
    public interface IEntity : IQueryableEntity {
        /// <summary>
        /// Each entity has a unique identifier. The identifier is *never* shared by any other
        /// entity during the entire simulation of the game.
        /// </summary>
        int UniqueId {
            get;
        }

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
        Data.IData AddOrModify(DataAccessor accessor);

        /// <summary>
        /// Add a Data instance of with the given accessor to the Entity.
        /// </summary>
        /// <returns>The data instance that can be used to initialize the data</returns>
        Data.IData AddData(DataAccessor accessor);

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
        Data.IData Modify(DataAccessor accessor);

        /// <summary>
        /// Returns if the given data was modified in the previous update.
        /// </summary>
        bool WasModified(DataAccessor accessor);

        /// <summary>
        /// Returns true if the given data was added to the entity in the previous update.
        /// </summary>
        bool WasAdded(DataAccessor accessor);

        /// <summary>
        /// Returns true if the data was removed from the entity in the previous update.
        /// </summary>
        bool WasRemoved(DataAccessor accessor);
    }
}