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

using Lidgren.Network;

namespace Neon.Networking.Core {
    /// <summary>
    /// This class contains Lidgren.Network configuration settings that are used when creating
    /// NetPeer (typically either a NetServer or a NetClient type) instances.
    /// </summary>
    internal static class Configuration {
        /// <summary>
        /// The port that is used when hosting.
        /// </summary>
        public const int Port = 6009;

        /// <summary>
        /// Global application name that is used to distinguish Neon.
        /// </summary>
        public const string AppName = "Neon.Network";

        public static NetPeerConfiguration GetConfiguration(bool server) {
            NetPeerConfiguration config = new NetPeerConfiguration("Neon.Networking");

            if (server) {
                config.Port = Port;
                config.EnableUPnP = true;
                config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
                config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            }
            else {
                config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            }

            return config;
        }
    }
}