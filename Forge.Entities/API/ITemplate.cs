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
    /// Used for creating IEntity instances that have a set of data values already initialized.
    /// Templates should not be modified at runtime.
    /// </summary>
    /// <remarks>
    /// For example, a generic Orc type will have an ITemplate that defines an Orc. Spawning code
    /// will then receive the Orc ITemplate, and when it comes time to spawn it will instantiate an
    /// entity from the template, and that entity will be a derivative instance of the original Orc.
    /// </remarks>
    [JsonConverter(typeof(QueryableEntityConverter))]
    public interface ITemplate : IQueryableEntity {
        /// <summary>
        /// Each ITemplate can be uniquely identified by its TemplateId.
        /// </summary>
        int TemplateId {
            get;
        }

        /// <summary>
        /// Instantiates the template to create a new IEntity instance. The IEntity is automatically
        /// registered with the IGameEngine that owns this template reference. The spawned IEntity
        /// will be added to systems on the next update call. The returned entity can be freely
        /// modified; modifications can be viewed as pre-initialization.
        /// </summary>
        IEntity Instantiate();

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException. Templates that are being used in an IGameEngine cannot be
        /// modified.
        /// </remarks>
        /// <param name="data">The data instance to copy from.</param>
        void AddDefaultData(Data.IData data);

        /// <summary>
        /// Remove the given type of data from the template instance. New instances will not longer
        /// have this added to the template.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="accessor">The type of data to remove.</param>
        /// <returns>True if the data was removed.</returns>
        bool RemoveDefaultData(DataAccessor accessor);
    }
}