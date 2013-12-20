using Neon.Entities.Implementation.Content;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    internal class SavedLevel : ISavedLevel {
        private GameSnapshot _currentState = new GameSnapshot();
        private GameSnapshot _originalState = new GameSnapshot();
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