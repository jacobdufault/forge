using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Network {

    /// <summary>
    /// Network message sent by the lobby server to verify that all clients have the given map.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class LobbyMapVerifyNetworkMessage : INetworkMessage {
        /// <summary>
        /// The hash code for the map, used to check to see if we need to download it.
        /// </summary>
        [JsonProperty]
        public string MapHash;
    }

    /// <summary>
    /// Network message sent by a lobby client to request a map download.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class LobbyMapDownloadRequestedNetworkMessage : INetworkMessage {
    }

    /// <summary>
    /// Network message sent to lobby clients by the lobby server after the lobby server has
    /// received a LobbyMapDownloadedRequestedNetworkMessage.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class LobbyMapDownloadNetworkMessage : INetworkMessage {
        [JsonProperty]
        public string Map;
    }

    /// <summary>
    /// Processes map download request messages and also sends map verification messages to new
    /// clients. Supports changing the current map (which causes a rebroadcast for map
    /// verification) .
    /// </summary>
    internal class MapDownloadServerMessageHandler : INetworkMessageHandler, INetworkConnectionMonitor {
        private NetworkContext _context;

        private string _serializedMap;
        private string _mapHash;

        private IMapManager _mapManager;

        public void ChangeMap(string serializedMap) {
            _serializedMap = serializedMap;
            _mapHash = _mapManager.GetHash(_serializedMap);

            _context.SendMessage(NetworkMessageRecipient.Clients, new LobbyMapVerifyNetworkMessage() {
                MapHash = _mapHash
            });
        }

        public MapDownloadServerMessageHandler(NetworkContext context, IMapManager mapManager, string map) {
            _context = context;
            _mapManager = mapManager;

            ChangeMap(map);
        }

        public Type[] HandledTypes {
            get {
                return new[] {
                    typeof(LobbyMapDownloadRequestedNetworkMessage),
                };
            }
        }

        public void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            if (message is LobbyMapDownloadRequestedNetworkMessage) {
                _context.SendMessage(sender, new LobbyMapDownloadNetworkMessage() {
                    Map = _serializedMap
                });
            }
        }

        public void OnConnected(NetworkPlayer player) {
            _context.SendMessage(player, new LobbyMapVerifyNetworkMessage() {
                MapHash = _mapHash
            });
        }

        public void OnDisconnected(NetworkPlayer player) {
        }
    }

    /// <summary>
    /// Interface used to check if a map exists and optionally save a downloaded map if one does
    /// not.
    /// </summary>
    public interface IMapManager {
        Task<bool> HasMap(string mapHash);
        string GetHash(string serializedMap);
        void Save(string serializedMap);
    }

    /// <summary>
    /// Map download handler for the lobby client. Receives map verification messages and map
    /// download messages. If the verification message fails, then a map download request message is
    /// sent to the server.
    /// </summary>
    internal class MapDownloadClientMessageHandler : INetworkMessageHandler {
        private NetworkContext _context;
        private IMapManager _mapManager;

        public MapDownloadClientMessageHandler(NetworkContext context, IMapManager mapManager) {
            _context = context;
            _mapManager = mapManager;
        }

        public Type[] HandledTypes {
            get {
                return new[] {
                    typeof(LobbyMapDownloadNetworkMessage),
                    typeof(LobbyMapVerifyNetworkMessage)
                };
            }
        }

        public void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            if (message is LobbyMapDownloadNetworkMessage) {
                var m = (LobbyMapDownloadNetworkMessage)message;
                _mapManager.Save(m.Map);
            }

            if (message is LobbyMapVerifyNetworkMessage) {
                var m = (LobbyMapVerifyNetworkMessage)message;
                _mapManager.HasMap(m.MapHash).ContinueWith(result => {
                    bool hasMap = result.Result;
                    if (hasMap == false) {
                        _context.SendMessage(NetworkMessageRecipient.Server,
                            new LobbyMapDownloadRequestedNetworkMessage());
                    }
                });
            }
        }
    }
}