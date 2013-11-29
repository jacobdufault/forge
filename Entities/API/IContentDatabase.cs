using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Shared;
using System.Collections.Generic;

namespace Neon.Entities {
    // TODO: what should this factory be named? EntityFctory and TemplateFacotyr?
    public class ContentDatabaseHelper {
        /// <summary>
        /// Creates a new IEntity instance that can be modified as pleased. The returned object will
        /// respond to modifications immediately; ie, after calling entity.AddData, entity.Current
        /// will return the added instance, effectively eschewing the immutability and update-order
        /// properties that the game engine has to simplify content creation. Make sure to add the
        /// entity instance to an IContentDatabase!
        /// </summary>
        public static IEntity CreateEntity() {
            return new ContentEntity();
        }

        /// <summary>
        /// Creates a new ITemplate instance that can be modified. Make sure to add it to an
        /// IContentDatabase!
        /// </summary>
        public static ITemplate CreateTemplate() {
            return new Template();
        }
    }

    /// <summary>
    /// The IGameSnapshot stores a serialized state of the game. It provides a common interface that
    /// both the engine and the editor use for accessing saved games and replays.
    /// </summary>
    /// <remarks>
    /// All implementations of this class must extend the MarshalByRefObject class, as
    /// IContentDatabase instances are passed around AppDomains. Similarly, all data that can be
    /// exposed by the content database, such as entities, also need to extend MarshalByRefObject.
    /// </remarks>
    public interface IGameSnapshot {
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
        List<IEntity> ActiveEntities {
            get;
        }

        /// <summary>
        /// All entities that were removed during the previous update.
        /// </summary>
        List<IEntity> RemovedEntities {
            get;
        }

        /// <summary>
        /// All entities that were added during the previous update.
        /// </summary>
        List<IEntity> AddedEntities {
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