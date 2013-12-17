using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
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
        /// Dispatches all of the events that have been triggered in the previous update on the
        /// calling thread.
        /// </summary>
        void DispatchEvents();

        /// <summary>
        /// Event notifier used to notify listeners of events that have occurred inside of the
        /// engine, such as an entity being added.
        /// </summary>
        IEventNotifier EventNotifier {
            get;
        }

        /// <summary>
        /// Iterates through all data inside of the engine and returns a a snapshot of the game that
        /// reflects everything contained within the engine.
        /// </summary>
        /// <remarks>
        /// Be wary of calling this method too often; it requires that no update is occurring (it
        /// will block until the update is done) and can take a decent amount of time to calculate.
        /// It additionally performs a large number of allocations.
        /// </remarks>
        /// <returns>A content database that contains all content within the engine. All data stored
        /// inside of the database is independent of the data stored inside of the engine, so
        /// changes to the engine will not be reflected inside of the database.</returns>
        IGameSnapshot TakeSnapshot();

        /// <summary>
        /// Returns a hash code of all data inside of the engine. The hash code is computed via
        /// reflection and can be used to attempt to determine if two game engines are out of sync.
        /// </summary>
        byte[] GetVerificationHash();
    }

    /// <summary>
    /// Allocates IGameEngines that can be used to play the state stored inside of a content
    /// database.
    /// </summary>
    public static class GameEngineFactory {
        /// <summary>
        /// Creates a new game engine that can be used to simulate the game using the content from
        /// the given content database. The passed in snapshot will not be modified.
        /// </summary>
        /// <param name="content">The content to allocate the engine from.</param>
        /// <param name="templates">The templates to use when running the engine.</param>
        /// <returns>A game engine that can play the given content.</returns>
        public static IGameEngine CreateEngine(IGameSnapshot content,
            IEnumerable<ITemplate> templates) {
            return new GameEngine((GameSnapshot)content, templates);
        }
    }
}