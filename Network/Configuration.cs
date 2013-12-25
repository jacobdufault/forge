using Lidgren.Network;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Network {
    internal static class Configuration {
        public const int Port = 6009;

        public static NetPeerConfiguration GetConfiguration(bool server) {
            NetPeerConfiguration config = new NetPeerConfiguration("Neon.Networking");

            if (server) {
                config.Port = Port;
                config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            }

            return config;
        }
    }

    /*
    public class NetworkPeer {
        private NetPeer _peer;
        private NetworkMessageDispatcher _dispatcher;

        public NetworkPeer(NetPeer peer) {
            _peer = peer;
            _dispatcher = new NetworkMessageDispatcher();
        }

        public void Update() {
            NetIncomingMessage msg;
            while ((msg = _peer.ReadMessage()) != null) {
                switch (msg.MessageType) {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;

                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }
                _peer.Recycle(msg);
            }
        }

        public void SendMessage(INetworkMessage message) {
            NetOutgoingMessage sendMsg = _peer.CreateMessage();
            sendMsg.Write("Hello");
            sendMsg.Write(42);

            _peer.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
        }
    }
    */
    /*
    public class NetworkServer : NetworkPeer {
        private NetServer _server;

        public NetworkServer(string appIdentifier)
            : this(new NetServer(Common.GetConfiguration(appIdentifier, server: true))) {
        }

        private NetworkServer(NetServer server)
            : base(server) {
            _server = server;
            _server.Start();
        }
    }

    public class NetworkClient { }
    */
}