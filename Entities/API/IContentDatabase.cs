using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities {
    /// <summary>
    /// The ContentDatabase stores a serialized game state. It provides a common interface that both
    /// the engine and the editor use for accessing saved games and replays.
    /// </summary>
    /// <remarks>
    /// All implementations of this class must extend the MarshalByRefObject class, as
    /// IContentDatabase instances are passed around AppDomains. Similarly, all data that can be
    /// exposed by the content database, such as entities, also need to extend MarshalByRefObject.
    /// </remarks>
    public interface IContentDatabase {
        /// <summary>
        /// Adds a new IEntity to the given IContentDatabase this modifier is attached to.
        /// </summary>
        /// <remarks>
        /// After calling this method, Entities will contain an instance of the returned value.
        /// </remarks>
        /// <returns>The newly added IEntity.</returns>
        IEntity AddEntity();

        /// <summary>
        /// Adds a new ITemplate to the given IContentDatabase this modifier is attached to.
        /// </summary>
        /// <remarks>
        /// After this method returns, Templates will contain an instance of the returned value.
        /// </remarks>
        /// <returns>The newly added ITemplate.</returns>
        ITemplate AddTemplate();

        /// <summary>
        /// The singleton entity. It is automatically created and cannot be destroyed, but it can be
        /// modified.
        /// </summary>
        IEntity SingletonEntity {
            get;
        }

        /// <summary>
        /// All currently alive entities.
        /// </summary>
        List<IEntity> Entities {
            get;
        }

        /// <summary>
        /// All templates in the state.
        /// </summary>
        List<ITemplate> Templates {
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