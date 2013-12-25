using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {

    /// <summary>
    /// Networking object used to run the game.
    /// </summary>
    public class GameNetwork {
        private NetworkMessageDispatcher _dispatcher;
        private NetworkContext _context;

        internal GameNetwork(NetworkContext context) {
            _context = context;
            _dispatcher = new NetworkMessageDispatcher();
        }
    }
}