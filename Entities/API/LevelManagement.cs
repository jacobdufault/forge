using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neon.Entities {
    /// <summary>
    /// Facilitates the loading and saving of IContentDatabases (levels).
    /// </summary>
    public static class LevelManager {
        /// <summary>
        /// Returns a new empty level.
        /// </summary>
        public static ISavedLevel CreateLevel() {
            return new SavedLevel();
        }

        /// <summary>
        /// Saves the given level to the given destination stream.
        /// </summary>
        /// <param name="level">The level to save.</param>
        public static string Save(ISavedLevel level) {
            return SerializationHelpers.Serialize((SavedLevel)level,
                RequiredConverters.GetConverters(), RequiredConverters.GetContexts(Maybe<GameEngine>.Empty));
        }

        /// <summary>
        /// Restores a level from a stream that was used to save it.
        /// </summary>
        /// <param name="json">The saved JSON to restore the level from.</param>
        /// <returns>A fully restored level.</returns>
        public static ISavedLevel Load(string json) {
            ISavedLevel restored = SerializationHelpers.Deserialize<SavedLevel>(json, RequiredConverters.GetConverters(), RequiredConverters.GetContexts(Maybe<GameEngine>.Empty));
            return restored;
        }
    }

    /// <summary>
    /// Stores an IGameInput instance along with the update that the input was issued.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class IssuedInput {
        /// <summary>
        /// The update number that this input was issued at.
        /// </summary>
        [JsonProperty("UpdateNumber")]
        public int UpdateNumber {
            get;
            private set;
        }

        /// <summary>
        /// The input instance itself. This is serialized under _serializedInput.
        /// </summary>
        [JsonProperty("Input")]
        public IGameInput Input {
            get;
            private set;
        }

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
        IGameSnapshot CurrentState {
            get;
        }

        /// <summary>
        /// The original state of the game before any updates occurred.
        /// </summary>
        IGameSnapshot OriginalState {
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