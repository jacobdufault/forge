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
using System.Threading.Tasks;

namespace Neon.Networking.Lobby {

    /// <summary>
    /// A member of a lobby.
    /// </summary>
    public sealed class LobbyMember : LobbyCommon {
        private MapDownloadClientMessageHandler _mapHandler;

        private LobbyMember(NetworkContext context, IMapManager mapManager)
            : base(context) {
            _mapHandler = new MapDownloadClientMessageHandler(context, mapManager);
            context.AddMessageHandler(_mapHandler);
        }

        public override void Dispose() {
            base.Dispose();

            if (_mapHandler != null) {
                _context.RemoveMessageHandler(_mapHandler);
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
        public static Task<Maybe<LobbyMember>> JoinLobby(string host, Player player, IMapManager mapManager, string password) {
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
}