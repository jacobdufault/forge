using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Neon.Network {
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