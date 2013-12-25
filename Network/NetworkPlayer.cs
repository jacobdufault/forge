using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
    /// <summary>
    /// A network player is an abstraction over a network connection.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class NetworkPlayer {
        /// <summary>
        /// The name that the player gave themselves.
        /// </summary>
        [JsonProperty("Name")]
        public string Name;

        /// <summary>
        /// The GUID that uniquely identifies this player. This GUID can be per-session and does not
        /// need to be permanent.
        /// </summary>
        [JsonProperty("Guid")]
        public Guid Guid;

        public override bool Equals(object obj) {
            if (obj is NetworkPlayer == false) {
                return false;
            }

            NetworkPlayer player = (NetworkPlayer)obj;
            return Guid == player.Guid;
        }

        public override int GetHashCode() {
            return Guid.GetHashCode();
        }
    }
}