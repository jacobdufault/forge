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

namespace Neon.Networking.Pausing {
    /// <summary>
    /// API for interacting with the pausing subsystem.
    /// </summary>
    public class PauseManager {
        /// <summary>
        /// Network context that we use to transmit pause messages.
        /// </summary>
        private NetworkContext _context;

        /// <summary>
        /// Message handler that processes the pause messages.
        /// </summary>
        private PauseMessageHandler _handler;

        public PauseManager(NetworkContext context) {
            _handler = new PauseMessageHandler();
            _context.AddMessageHandler(_handler);
        }

        public void Dispose() {
            if (_handler != null) {
                _context.RemoveMessageHandler(_handler);
                _handler = null;
            }
        }

        /// <summary>
        /// Returns the current pause status for the game. Setting this value emits a network
        /// message that changes the pause status to the given value for all computers in the
        /// network.
        /// </summary>
        public bool IsPaused {
            get {
                return _handler.IsPaused;
            }
            set {
                _context.SendMessage(NetworkMessageRecipient.All, new SetPauseStatusNetworkMessage() {
                    Paused = value
                });
            }
        }

        /// <summary>
        /// If the game is paused, then this returns who paused the game. If the game is not paused,
        /// then this returns nothing.
        /// </summary>
        public Maybe<NetworkPlayer> PausedBy {
            get {
                return _handler.PausedBy;
            }
        }
    }
}