using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using ProtoBuf;
using System;
using System.Collections.Generic;

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
    }

    /// <summary>
    /// Stores an IGameInput instance along with the update that the input was issued.
    /// </summary>
    [ProtoContract]
    public class IssuedInput {
        /// <summary>
        /// The update number that this input was issued at.
        /// </summary>
        [ProtoMember(1)]
        public int UpdateNumber {
            get;
            private set;
        }

        /// <summary>
        /// The input instance itself. This is serialized under _serializedInput.
        /// </summary>
        public IGameInput Input {
            get;
            private set;
        }

        private byte[] _serializedInput;

        [ProtoBeforeSerialization]
        private void OnExport() {
            _serializedInput = SerializationHelpers.SerializeWithType(Input);
        }

        [ProtoAfterDeserialization]
        private void OnImport() {
            Input = (IGameInput)SerializationHelpers.DeserializeWithType(_serializedInput);
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
        /// The templates that are used in the level.
        /// </summary>
        IEnumerable<ITemplate> Templates {
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