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
    internal class LobbyLaunchedHandler : INetworkMessageHandler {
        public Type[] HandledTypes {
            get { return new[] { typeof(LobbyLaunchedNetworkMessage) }; }
        }

        public bool IsLaunched;

        public void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            Contract.Requires(message is LobbyLaunchedNetworkMessage);
            IsLaunched = true;
        }
    }

    /// <summary>
    /// Network message sent when the lobby has been launched.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class LobbyLaunchedNetworkMessage : INetworkMessage {
        /// <summary>
        /// The players that are in the game.
        /// </summary>
        [JsonProperty("Players")]
        public List<NetworkPlayer> Players;
    }
}