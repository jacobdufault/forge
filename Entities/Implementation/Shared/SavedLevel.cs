using Neon.Entities.Implementation.Content;
using Neon.Entities.Serialization;
using Neon.Utilities;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    [ProtoContract]
    internal class SavedLevel : ISavedLevel {
        [ProtoMember(1)]
        private GameSnapshot _currentState = new GameSnapshot();
        [ProtoMember(2)]
        private GameSnapshot _originalState = new GameSnapshot();
        [ProtoMember(3)]
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