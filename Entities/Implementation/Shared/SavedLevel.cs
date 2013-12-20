using Neon.Entities.Implementation.Content;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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