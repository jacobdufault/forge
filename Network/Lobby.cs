using Lidgren.Network;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Network.Lobby {
    /// <summary>
    /// Common code for LobbyMember and LobbyHost.
    /// </summary>
    public abstract class LobbyCommon {
        /// <summary>
        /// The network context that we use for core networking operations.
        /// </summary>
        internal NetworkContext _context;

        public NetworkContext Context {
            get {
                return _context;
            }
        }

        /// <summary>
        /// If we have launched a game, then _gameNetwork will contain the game networking instance.
        /// </summary>
        private GameNetwork _gameNetwork;

        /// <summary>
        /// Message handler used to determine if we've received a LobbyLaunchedNetworkMessage.
        /// </summary>
        private LobbyLaunchedHandler _lobbyLaunchedHandler;

        private PlayerManager _playerManager;

        internal LobbyCommon(NetworkContext context) {
            _context = context;

            _lobbyLaunchedHandler = new LobbyLaunchedHandler();
            _context.Dispatcher.AddHandler(_lobbyLaunchedHandler);

            _playerManager = new PlayerManager(_context);
            _context.Dispatcher.AddHandler(_playerManager);
            if (_context.IsServer) {
                _context.AddConnectionMonitor(_playerManager);
            }
        }

        public void Update() {
            _context.Update();
        }

        /// <summary>
        /// Get all members of the lobby, including the host.
        /// </summary>
        public IEnumerable<NetworkPlayer> GetLobbyMembers() {
            return _playerManager.Players;
        }

        /// <summary>
        /// Is the given player the host of the lobby?
        /// </summary>
        public bool IsHost(NetworkPlayer player) {
            return _context.IsPlayerServer(player);
        }

        /// <summary>
        /// Dispose the lobby.
        /// </summary>
        public virtual void Dispose() {
            if (_lobbyLaunchedHandler != null) {
                _context.Dispatcher.RemoveHandler(_lobbyLaunchedHandler);
                _lobbyLaunchedHandler = null;
            }

            if (_playerManager != null) {
                _context.Dispatcher.RemoveHandler(_playerManager);
                if (_context.IsServer) {
                    _context.RemoveConnectionMonitor(_playerManager);
                }
                _playerManager = null;
            }
        }

        /// <summary>
        /// Get the game-play network context that should be used when playing the game. This
        /// returns a non-empty value when the game has been launched and all peers have loaded the
        /// game.
        /// </summary>
        public Maybe<GameNetwork> GetGameRun() {
            // If we have launched, then create our game network.
            if (_gameNetwork == null && _lobbyLaunchedHandler.IsLaunched) {
                _gameNetwork = new GameNetwork(_context);
                Dispose();
            }

            if (_gameNetwork != null) {
                return Maybe.Just(_gameNetwork);
            }
            return Maybe<GameNetwork>.Empty;
        }
    }

    /// <summary>
    /// A member of a lobby.
    /// </summary>
    public class LobbyMember : LobbyCommon {
        private MapDownloadClientMessageHandler _mapHandler;

        private LobbyMember(NetworkContext context, IMapManager mapManager)
            : base(context) {
            _mapHandler = new MapDownloadClientMessageHandler(context, mapManager);
            context.Dispatcher.AddHandler(_mapHandler);
        }

        public override void Dispose() {
            base.Dispose();

            if (_mapHandler != null) {
                _context.Dispatcher.RemoveHandler(_mapHandler);
                _mapHandler = null;
            }
        }

        /// <summary>
        /// Try to join the lobby at the given IP end point as the given player.
        /// </summary>
        /// <param name="host">The IP address that the lobby server is running at.</param>
        /// <param name="mapManager">The map manager that will be used to check to see if we have a
        /// map and to save a downloaded map.</param>
        /// <param name="player">This player that will be used to uniquely identify
        /// ourselves.</param>
        /// <param name="password">The password that the lobby host has set.</param>
        public static Task<Maybe<LobbyMember>> JoinLobby(string host, NetworkPlayer player, IMapManager mapManager, string password) {
            /*
            return Task.Factory.StartNew(() => {
                try {
                    Maybe<NetworkContext> context = NetworkContext.CreateClient(address, player, password);
                    if (context.Exists) {
                        LobbyMember lobbyMember = new LobbyMember(context.Value, mapManager);
                        return Maybe.Just(lobbyMember);
                    }
                }
                catch (Exception) {
                }

                return Maybe<LobbyMember>.Empty;
            });
            */

            Maybe<LobbyMember> result = Maybe<LobbyMember>.Empty;
            Maybe<NetworkContext> context = NetworkContext.CreateClient(host, player, password);
            if (context.Exists) {
                LobbyMember lobbyMember = new LobbyMember(context.Value, mapManager);
                result = Maybe.Just(lobbyMember);
            }

            return Task.Factory.StartNew(() => result);
        }
    }

    public class LobbyHost : LobbyCommon {
        /// <summary>
        /// Settings used for creating a lobby.
        /// </summary>
        public class LobbySettings {
            /// <summary>
            /// The password required for entering the lobby. Use an empty string for "no" password.
            /// </summary>
            public string Password;

            /// <summary>
            /// The serialized map that the lobby is hosting, ie, the data that lobby members will
            /// download.
            /// </summary>
            public string SerializedMap;

            /// <summary>
            /// Map manager used to get hashes for serialized maps.
            /// </summary>
            public IMapManager MapManager;
        }

        private MapDownloadServerMessageHandler _mapHandler;
        private LobbyHostPlayerReadinessMessageHandler _readinessHandler;
        private GameNetwork _gameNetwork;

        private LobbyHost(NetworkContext context, IMapManager mapManager, string map)
            : base(context) {
            _mapHandler = new MapDownloadServerMessageHandler(context, mapManager, map);
            _context.AddConnectionMonitor(_mapHandler);
            _context.Dispatcher.AddHandler(_mapHandler);

            _readinessHandler = new LobbyHostPlayerReadinessMessageHandler(context);
            _context.AddConnectionMonitor(_readinessHandler);
            _context.Dispatcher.AddHandler(_readinessHandler);
        }

        public override void Dispose() {
            base.Dispose();

            if (_mapHandler != null) {
                _context.RemoveConnectionMonitor(_mapHandler);
                _context.Dispatcher.RemoveHandler(_mapHandler);
                _mapHandler = null;
            }

            if (_readinessHandler != null) {
                _context.RemoveConnectionMonitor(_readinessHandler);
                _context.Dispatcher.RemoveHandler(_readinessHandler);
                _readinessHandler = null;
            }
        }

        public bool TryLaunch() {
            // We already launched; just return true
            if (_readinessHandler == null) {
                return true;
            }

            // We cant launch, so do it.
            if (_readinessHandler.CanLaunch()) {
                _context.SendMessage(NetworkMessageRecipient.All, new LobbyLaunchedNetworkMessage());
                return true;
            }

            // Can't launch yet; someone isn't ready.
            return false;
        }

        /// <summary>
        /// Return players that are not ready.
        /// </summary>
        public IEnumerable<NetworkPlayer> PlayersNotReady {
            get {
                return _readinessHandler.NotReadyPlayers;
            }
        }

        /// <summary>
        /// Host a new lobby.
        /// </summary>
        /// <param name="player">The player that is creating the server.</param>
        /// <param name="settings">The settings to use for the lobby.</param>
        /// <returns>A lobby host if successful.</returns>
        public static LobbyHost CreateLobby(NetworkPlayer player, LobbySettings settings) {
            NetworkContext context = NetworkContext.CreateServer(player, settings.Password);

            return new LobbyHost(context, settings.MapManager, settings.SerializedMap);
        }

        /// <summary>
        /// Change the map.
        /// </summary>
        public void ChangeMap(string serializedMap) {
            _mapHandler.ChangeMap(serializedMap);
        }

        /// <summary>
        /// Kick the given lobby member from the lobby.
        /// </summary>
        /// <param name="member">The member to kick.</param>
        public void Kick(NetworkPlayer member) {
            _context.Kick(member);
        }
    }
}