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

using System.Collections.Generic;

namespace Forge.Entities {
    /// <summary>
    /// Represents the result of removing an entity.
    /// </summary>
    public enum GameSnapshotEntityRemoveResult {
        /// <summary>
        /// The entity was completely destroyed and no longer exists.
        /// </summary>
        Destroyed,

        /// <summary>
        /// The entity was moved into the Removed collection.
        /// </summary>
        IntoRemoved,
    }

    /// <summary>
    /// The IGameSnapshot stores a serialized state of the engine. It provides a common interface
    /// that both the engine and the editor use for accessing saved games and replays.
    /// </summary>
    /// <remarks>
    /// All implementations of this class must extend the MarshalByRefObject class, as
    /// IContentDatabase instances are passed around AppDomains. Similarly, all data that can be
    /// exposed by the content database, such as entities, also need to extend MarshalByRefObject.
    /// </remarks>
    public interface IGameSnapshot {
        /// <summary>
        /// Adds a new entity to the snapshot (under the Added entities collection).
        /// </summary>
        /// <param name="prettyName">The pretty name of the entity.</param>
        /// <returns>A new entity.</returns>
        IEntity CreateEntity(string prettyName = "");

        /// <summary>
        /// Request for the given entity to be removed from the snapshot.
        /// </summary>
        /// <remarks>
        /// This function does different operations depending on what collection the entity is
        /// currently in. If it is the SingletonEntity, an exception is thrown. If it is in
        /// AddedEntities, the entity is just destroyed completely. If it is in ActiveEntities, the
        /// entity is moved to RemovedEntities. If it is RemovedEntities, an exception is thrown.
        /// </remarks>
        /// <param name="entity">The entity to remove.</param>
        GameSnapshotEntityRemoveResult RemoveEntity(IEntity entity);

        /// <summary>
        /// The singleton entity. It is automatically created and cannot be destroyed, but it can be
        /// modified.
        /// </summary>
        IEntity SingletonEntity {
            get;
        }

        /// <summary>
        /// All entities in the game that were not added or removed in the previous update.
        /// </summary>
        IEnumerable<IEntity> ActiveEntities {
            get;
        }

        /// <summary>
        /// All entities that were removed during the previous update.
        /// </summary>
        IEnumerable<IEntity> RemovedEntities {
            get;
        }

        /// <summary>
        /// All entities that were added during the previous update.
        /// </summary>
        IEnumerable<IEntity> AddedEntities {
            get;
        }

        /// <summary>
        /// All systems that are used when executing the game.
        /// </summary>
        List<BaseSystem> Systems {
            get;
        }
    }
}