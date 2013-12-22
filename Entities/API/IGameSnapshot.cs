using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// Specifies where an entity should be added to when adding one to a IGameSnapshot.
    /// </summary>
    public enum EntityAddTarget {
        Active,
        Removed,
        Added
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
        /// Adds a new entity to the snapshot.
        /// </summary>
        /// <param name="addTarget">What collection the entity should be added to.</param>
        /// <param name="prettyName">The pretty name of the entity.</param>
        /// <returns>A new entity.</returns>
        IEntity CreateEntity(EntityAddTarget addTarget, string prettyName = "");

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
        List<ISystem> Systems {
            get;
        }
    }
}