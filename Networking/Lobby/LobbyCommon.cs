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
using System.Collections.Generic;

namespace Neon.Networking.Lobby {
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
        /// Message handler used to determine if we've received a LobbyLaunchedNetworkMessage.
        /// </summary>
        private LobbyLaunchedHandler _lobbyLaunchedHandler;

        private PlayerManager _playerManager;

        internal LobbyCommon(NetworkContext context) {
            _context = context;

            _lobbyLaunchedHandler = new LobbyLaunchedHandler();
            _context.AddMessageHandler(_lobbyLaunchedHandler);

            _playerManager = new PlayerManager(_context);
            _context.AddMessageHandler(_playerManager);
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
                _context.RemoveMessageHandler(_lobbyLaunchedHandler);
                _lobbyLaunchedHandler = null;
            }

            if (_playerManager != null) {
                _context.RemoveMessageHandler(_playerManager);
                if (_context.IsServer) {
                    _context.RemoveConnectionMonitor(_playerManager);
                }
                _playerManager = null;
            }
        }

        /// <summary>
        /// Returns true if the lobby has launched. Make sure to dispose of the lobby.
        /// </summary>
        /// <returns>True if the lobby has launched, false if it hasn't.</returns>
        public bool HasLaunched {
            get {
                return _lobbyLaunchedHandler.IsLaunched;
            }
        }
    }
}