using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
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