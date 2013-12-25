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
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Neon.Network.Lobby {
    /// <summary>
    /// The given player has joined the network.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class PlayerJoinedNetworkMessage : INetworkMessage {
        [JsonProperty]
        public NetworkPlayer Player;
    }

    /// <summary>
    /// The given player has left the network.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class PlayerLeftNetworkMessage : INetworkMessage {
        [JsonProperty]
        public NetworkPlayer Player;
    }

    internal class PlayerManager : INetworkConnectionMonitor, INetworkMessageHandler {
        private NetworkContext _context;
        private List<NetworkPlayer> _players = new List<NetworkPlayer>();

        public IEnumerable<NetworkPlayer> Players {
            get {
                return _players;
            }
        }

        public PlayerManager(NetworkContext context) {
            _context = context;

            if (_context.IsServer) {
                _players.Add(_context.LocalPlayer);
            }
        }

        public void OnConnected(NetworkPlayer connectedPlayer) {
            // this has to go before we notify everyone else of the connected player, otherwise we
            // will transmit to the connected player that they themselves got connected twice
            foreach (var player in _players) {
                _context.SendMessage(connectedPlayer, new PlayerJoinedNetworkMessage() {
                    Player = player
                });
            }

            _context.SendMessage(NetworkMessageRecipient.All, new PlayerJoinedNetworkMessage() {
                Player = connectedPlayer
            });
        }

        public void OnDisconnected(NetworkPlayer player) {
            _context.SendMessage(NetworkMessageRecipient.All, new PlayerLeftNetworkMessage() {
                Player = player
            });
        }

        public Type[] HandledTypes {
            get {
                return new[] {
                    typeof(PlayerJoinedNetworkMessage),
                    typeof(PlayerLeftNetworkMessage)
                };
            }
        }

        public void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            Log<PlayerManager>.Info("PlayerManager got message " + message);

            if (message is PlayerJoinedNetworkMessage) {
                NetworkPlayer player = ((PlayerJoinedNetworkMessage)message).Player;
                _players.Add(player);
            }

            else if (message is PlayerLeftNetworkMessage) {
                NetworkPlayer player = ((PlayerLeftNetworkMessage)message).Player;
                _players.Remove(player);
            }
        }
    }
}