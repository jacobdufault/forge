using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// A context object that just contains the current GameEngine that is being used to deserialize
    /// the object.
    /// </summary>
    internal class GameEngineContext : IContextObject {
        public GameEngineContext(Maybe<GameEngine> gameEngine) {
            GameEngine = gameEngine;
        }

        public Maybe<GameEngine> GameEngine;
    }
}