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
using Neon.Network.Game;
using System.Collections.Generic;

namespace Neon.Network.Lobby {
    public sealed class LobbyHost : LobbyCommon {
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