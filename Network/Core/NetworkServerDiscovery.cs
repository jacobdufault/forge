using Lidgren.Network;
using log4net;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Neon.Network.Core {
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerDiscoveryFormat {
        /// <summary>
        /// The hosting player.
        /// </summary>
        [JsonProperty]
        public NetworkPlayer Host;

        /// <summary>
        /// The title of the game.
        /// </summary>
        [JsonProperty]
        public string Title;
    }

    /// <summary>
    /// Contains information about a running server.
    /// </summary>
    public struct DiscoveredServer {
        /// <summary>
        /// The player that is hosting the server.
        /// </summary>
        public NetworkPlayer Host;

        /// <summary>
        /// The title of the server.
        /// </summary>
        public string Title;

        /// <summary>
        /// The IP that can be used to connect to the server.
        /// </summary>
        public IPEndPoint IP;

        public override bool Equals(object obj) {
            if (obj is DiscoveredServer == false) {
                return false;
            }

            var server = (DiscoveredServer)obj;
            return Host == server.Host &&
                Title == server.Title &&
                IP == server.IP;
        }

        public override int GetHashCode() {
            return Host.GetHashCode() ^ Title.GetHashCode() ^ IP.GetHashCode();
        }
    }

    /// <summary>
    /// This class makes it simple to automatically discover servers that are running on the local
    /// network.
    /// </summary>
    public static class NetworkServerDiscovery {
        /// <summary>
        /// The NetClient we use to run the discovery service.
        /// </summary>
        private static NetClient _client;

        static NetworkServerDiscovery() {
            _client = new NetClient(Configuration.GetConfiguration(server: false));
            _client.Start();

            _discoveredLocalServers = new HashSet<DiscoveredServer>();
        }

        /// <summary>
        /// The servers that we have discovered.
        /// </summary>
        private static HashSet<DiscoveredServer> _discoveredLocalServers;

        /// <summary>
        /// Attempts to discover all running servers on the local network. DiscoveredLocalServers
        /// will contain the result; it may change over time.
        /// </summary>
        public static void DiscoverServers() {
            _client.DiscoverLocalPeers(Configuration.Port);
        }

        /// <summary>
        /// Clears out the list of discovered servers.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearDiscoveredServers() {
            _discoveredLocalServers.Clear();
        }

        /// <summary>
        /// Returns a list containing all servers that have been discovered thus far.
        /// </summary>
        public static IEnumerable<DiscoveredServer> DiscoveredLocalServers {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get {
                return new List<DiscoveredServer>(_discoveredLocalServers);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Update() {
            NetIncomingMessage msg;
            while ((msg = _client.ReadMessage()) != null) {
                LogManager.GetLogger(typeof(NetworkServerDiscovery)).Info("Discovery service got message type " + msg.MessageType);
                switch (msg.MessageType) {
                    case NetIncomingMessageType.DiscoveryResponse:
                        var response = SerializationHelpers.Deserialize<ServerDiscoveryFormat>(msg.ReadString());
                        IPEndPoint serverAddress = msg.SenderEndPoint;

                        _discoveredLocalServers.Add(new DiscoveredServer() {
                            Host = response.Host,
                            Title = response.Title,
                            IP = serverAddress
                        });
                        break;

                    default:
                        LogManager.GetLogger(typeof(NetworkServerDiscovery)).Error("Bad message type in NetworkServerDiscovery; got " + msg.MessageType);
                        break;
                }
            }
        }
    }
}