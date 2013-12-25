using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
    /// <summary>
    /// A message transmitted over the network. Network messages are always transmitted in order and
    /// reliably.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public interface INetworkMessage {
    }
}