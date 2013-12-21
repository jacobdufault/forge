using Neon.Entities.Implementation.Content;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Shared {
    [JsonObject(MemberSerialization.OptIn)]
    internal class SavedLevel : ISavedLevel {
        [JsonProperty("CurrentState")]
        private GameSnapshot _currentState = new GameSnapshot();
        [JsonProperty("OriginalState")]
        private GameSnapshot _originalState = new GameSnapshot();
        [JsonProperty("Input")]
        private List<IssuedInput> _input = new List<IssuedInput>();

        IGameSnapshot ISavedLevel.CurrentState {
            get {
                return _currentState;
            }
        }

        IGameSnapshot ISavedLevel.OriginalState {
            get {
                return _originalState;
            }
        }

        List<IssuedInput> ISavedLevel.Input {
            get {
                return _input;
            }
        }
    }
}