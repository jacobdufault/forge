// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Neon.Networking.Core;
using Neon.Utilities;
using System.ComponentModel;

namespace Neon.Networking.Pausing {
    /// <summary>
    /// Updates the pause status when SetPauseStatusNetworkMessages are received.
    /// </summary>
    internal class PauseMessageHandler : BaseNetworkMessageHandler<SetPauseStatusNetworkMessage> {
        /// <summary>
        /// Getter/setter for if the game is paused.
        /// </summary>
        public bool IsPaused {
            get;
            private set;
        }

        /// <summary>
        /// Returns who paused the game if it is paused.
        /// </summary>
        public Maybe<NetworkPlayer> PausedBy {
            get;
            private set;
        }

        public PauseMessageHandler() {
            IsPaused = true;
            PausedBy = Maybe<NetworkPlayer>.Empty;
        }

        protected override void HandleNetworkMessage(NetworkPlayer sender,
            SetPauseStatusNetworkMessage message) {

            IsPaused = message.Paused;

            if (IsPaused) {
                PausedBy = Maybe.Just(sender);
            }
            else {
                PausedBy = Maybe<NetworkPlayer>.Empty;
            }
        }
    }
}