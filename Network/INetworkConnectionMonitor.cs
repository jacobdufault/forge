using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
    /// <summary>
    /// Object that monitors the connection and disconnection of other computers.
    /// </summary>
    public interface INetworkConnectionMonitor {
        /// <summary>
        /// The given player has connected.
        /// </summary>
        void OnConnected(NetworkPlayer player);

        /// <summary>
        /// The given player has disconnected.
        /// </summary>
        void OnDisconnected(NetworkPlayer player);
    }
}