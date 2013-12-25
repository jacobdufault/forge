using Neon.Network.Lobby;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
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