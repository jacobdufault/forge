using Neon.FileSaving;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// Facilitates the loading and saving of IContentDatabases (levels).
    /// </summary>
    public static class LevelManager {
        /// <summary>
        /// Attempts to load level that is contained within the given string.
        /// </summary>
        /// <param name="level">The level to load.</param>
        /// <returns>An empty maybe if no level could be loaded, otherwise the loaded
        /// level.</returns>
        public static Maybe<ISavedLevel> Load(SavedStateReader level) {
            // TODO: ensure that we are support the level version
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a content database to the given file. The database is assumed to represent both
        /// the current and original states, with no input issued thus far. This method is most
        /// appropriate for usage inside of, ie, an editor, and not for actually saving a game.
        /// </summary>
        public static void Save(SavedStateReader level, IContentDatabase content) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a level.
        /// </summary>
        /// <param name="original">The original content database that the level was initially
        /// started from, ie, the initial state of the level.</param>
        /// <param name="current">The current content database; this is strictly not necessary, but
        /// facilitates faster level loading.</param>
        /// <param name="input">The input that was used to transform the original database into the
        /// current one.</param>
        public static string Save(IContentDatabase original, IContentDatabase current,
            List<IssuedInput> input) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Stores an IGameInput instance along with the update that the input was issued.
    /// </summary>
    public class IssuedInput {
        /// <summary>
        /// The update number that this input was issued at.
        /// </summary>
        public readonly int UpdateNumber;

        /// <summary>
        /// The input instance itself.
        /// </summary>
        public readonly IGameInput Input;

        /// <summary>
        /// Creates a new IssuedInput instance.
        /// </summary>
        public IssuedInput(int updateNumber, IGameInput input) {
            UpdateNumber = updateNumber;
            Input = input;
        }
    }

    /// <summary>
    /// Represents a level that has been saved.
    /// </summary>
    public interface ISavedLevel {
        /// <summary>
        /// The current state of the game.
        /// </summary>
        IContentDatabase CurrentState {
            get;
        }

        /// <summary>
        /// The original state of the game before any updates occurred.
        /// </summary>
        IContentDatabase OriginalState {
            get;
        }

        /// <summary>
        /// The input that can be used to transform the original state into the current state.
        /// </summary>
        List<IssuedInput> Input {
            get;
        }
    }
}