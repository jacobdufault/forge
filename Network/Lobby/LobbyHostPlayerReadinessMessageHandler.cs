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

using Neon.Network.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Neon.Network.Lobby {
    /// <summary>
    /// The sending client is ready to launch the game.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class LobbyReadyNetworkMessage : INetworkMessage { }

    /// <summary>
    /// The sending client is not ready to launch the game.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class LobbyNotReadyNetworkMessage : INetworkMessage { }

    /// <summary>
    /// Handles network messages for determining if every player is ready to launch the game.
    /// </summary>
    internal class LobbyHostPlayerReadinessMessageHandler : INetworkMessageHandler, INetworkConnectionMonitor {
        /// <summary>
        /// Players that are ready to launch.
        /// </summary>
        private HashSet<NetworkPlayer> _ready = new HashSet<NetworkPlayer>();

        /// <summary>
        /// Players that are not ready to launch.
        /// </summary>
        private HashSet<NetworkPlayer> _notReady = new HashSet<NetworkPlayer>();

        /// <summary>
        /// The networking context.
        /// </summary>
        private NetworkContext _context;

        public LobbyHostPlayerReadinessMessageHandler(NetworkContext context) {
            _context = context;
        }

        public IEnumerable<NetworkPlayer> ReadyPlayers {
            get {
                return _ready;
            }
        }

        public IEnumerable<NetworkPlayer> NotReadyPlayers {
            get {
                return _notReady;
            }
        }

        /// <summary>
        /// Returns true if every player is ready.
        /// </summary>
        public bool CanLaunch() {
            return _notReady.Count == 0;
        }

        public Type[] HandledTypes {
            get {
                return new[] {
                    typeof(LobbyNotReadyNetworkMessage),
                    typeof(LobbyReadyNetworkMessage),
                };
            }
        }

        public void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            if (message is LobbyNotReadyNetworkMessage) {
                _ready.Remove(sender);
                _notReady.Add(sender);
            }

            else if (message is LobbyReadyNetworkMessage) {
                _notReady.Remove(sender);
                _ready.Add(sender);
            }
        }

        public void OnConnected(NetworkPlayer player) {
            _notReady.Add(player);
        }

        public void OnDisconnected(NetworkPlayer player) {
            _notReady.Remove(player);
            _ready.Remove(player);
        }
    }
}