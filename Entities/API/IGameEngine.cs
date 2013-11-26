using System;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Entities {
    /// <summary>
    /// Manages the execution of a game. Allows for players to input commands to the game, thereby
    /// modifying how the game plays out.
    /// </summary>
    /// <remarks>
    /// Instances of this class are allocated when loading levels.
    /// </remarks>
    public interface IGameEngine {
        /// <summary>
        /// Runs a game update tick using the given input. Make sure that SynchronizeState call both
        /// starts and completes before calling Update again.
        /// </summary>
        /// <returns>A WaitHandle that is activated (set to true) when the update has
        /// finished.</returns>
        WaitHandle Update(IEnumerable<IGameInput> input);

        /// <summary>
        /// Synchronizes the state of game.
        /// </summary>
        /// <remarks>
        /// The game manager will typically run in a multithreaded context, with the rendering
        /// thread pulling data from the game. If the shared state between the game and the renderer
        /// suddenly changes half-way through a render, then tearing and generally bad things will
        /// happen. Because of this, when updating the game, no shared state between the renderer
        /// and the engine is modified. Instead, it will be modified after this method has been
        /// called.
        /// </remarks>
        /// <returns>A WaitHandle that is activated (set to true) when all state has been
        /// synchronized.</returns>
        WaitHandle SynchronizeState();

        /// <summary>
        /// Iterates through all data inside of the engine and returns a content database that
        /// reflects everything contained within the engine. This method also performs a state
        /// synchronization.
        /// </summary>
        /// <remarks>
        /// Be wary of calling this method too often; it requires that no update is occurring (it
        /// will block until the update is done) and can take a decent amount of time to calculate.
        /// It additionally performs a large number of allocations.
        /// </remarks>
        /// <returns>A content database that contains all content within the engine. All data stored
        /// inside of the database is independent of the data stored inside of the engine, so
        /// changes to the engine will not be reflected inside of the database.</returns>
        IContentDatabase GetContentDatabase();
    }

    /// <summary>
    /// Allocates IGameEngines that can be used to play the state stored inside of a content
    /// database.
    /// </summary>
    public static class GameEngineFactory {
        /// <summary>
        /// Creates a new game engine that can be used to simulate the game using the content from
        /// the given content database.
        /// </summary>
        /// <param name="content">The content to allocate the engine from.</param>
        /// <returns>A game engine that can play the given content.</returns>
        public static IGameEngine CreateEngine(IContentDatabase content) {
            throw new NotImplementedException();
        }
    }
}