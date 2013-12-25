using Lidgren.Network;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Network {
    /// <summary>
    /// Holds important information about the current network connection and additionally about
    /// INetworkMessage listeners.
    /// </summary>
    public class NetworkContext {
        internal NetworkMessageDispatcher Dispatcher;

        /// <summary>
        /// The local player.
        /// </summary>
        internal NetworkPlayer LocalPlayer;
        internal NetworkPlayer[] _localPlayerEnumerable;

        private NetClient _client;
        private NetServer _server;
        private string _serverPassword;
        private List<INetworkConnectionMonitor> _connectionMonitors;

        public NetPeer Peer {
            get {
                return (NetPeer)_client ?? (NetPeer)_server;
            }
        }

        public bool IsServer {
            get {
                return _server != null;
            }
        }

        public bool IsClient {
            get {
                return _client != null;
            }
        }

        public bool IsPlayerServer(NetworkPlayer player) {
            if (IsServer) {
                return player == (NetworkPlayer)_server.Tag;
            }

            if (IsClient) {
                return player == (NetworkPlayer)_client.ServerConnection.Tag;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Kicks the given player. This function is only operable if the context is a server
        /// (otherwise an exception is thrown).
        /// </summary>
        /// <param name="player">The player to kick.</param>
        public void Kick(NetworkPlayer player) {
            if (IsServer == false) {
                throw new InvalidOperationException("Only servers can kick players");
            }

            if (player == LocalPlayer) {
                throw new InvalidOperationException("Cannot kick the local player");
            }

            GetConnection(player).Disconnect("Kicked by host");
        }

        internal NetConnection GetConnection(NetworkPlayer player) {
            NetPeer peer = (NetPeer)_server ?? (NetPeer)_client;

            for (int i = 0; i < peer.ConnectionsCount; ++i) {
                NetConnection connection = peer.Connections[i];

                if (player == (NetworkPlayer)connection.Tag) {
                    return connection;
                }
            }

            throw new InvalidOperationException("No connection for player " + player);
        }

        /// <summary>
        /// Creates a new server.
        /// </summary>
        /// <param name="player">The player that is running this server.</param>
        /// <param name="password">The password that clients have to have to connect.</param>
        /// <returns>A network context for the created server.</returns>
        public static NetworkContext CreateServer(NetworkPlayer player, string password) {
            NetworkContext context = new NetworkContext(player);

            context._serverPassword = password;
            context._server = new NetServer(Configuration.GetConfiguration(server: true));
            context._server.Start();

            Log<NetworkContext>.Info("Created server network context");

            return context;
        }

        /// <summary>
        /// Hail message format used when connecting to a server.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private class HailMessage {
            /// <summary>
            /// The player connecting.
            /// </summary>
            [JsonProperty]
            public NetworkPlayer Player;

            /// <summary>
            /// The password to use when connecting.
            /// </summary>
            [JsonProperty]
            public string Password;
        }

        /// <summary>
        /// Creates a new client connection connected to the given IP end point.
        /// </summary>
        /// <param name="ip">The IP to connect to.</param>
        /// <param name="player">This computer's player.</param>
        /// <returns></returns>
        public static Maybe<NetworkContext> CreateClient(string ip, NetworkPlayer player, string password) {
            /*
            try {
                NetClient client = new NetClient(Configuration.GetConfiguration(server: false));
                NetOutgoingMessage hailMsg = client.CreateMessage();

                HailMessage hail = new HailMessage() {
                    Player = player,
                    Password = password
                };
                hailMsg.Write(JsonConvert.SerializeObject(hail));

                client.Connect(ip, hailMsg);

                NetworkContext context = new NetworkContext(player) {
                    _client = client
                };

                return Maybe.Just(context);
            }
            catch (Exception) {
                return Maybe<NetworkContext>.Empty;
            }
            */

            NetClient client = new NetClient(Configuration.GetConfiguration(server: false));
            client.Start();

            NetOutgoingMessage hailMsg = client.CreateMessage();

            HailMessage hail = new HailMessage() {
                Player = player,
                Password = password
            };
            string serializedHail = SerializationHelpers.Serialize(hail);
            hailMsg.Write(serializedHail);

            Log<NetworkContext>.Info("Trying to connect to " + ip + " on port " + Configuration.Port + " with hailing message " + serializedHail);
            client.Connect(ip, Configuration.Port, hailMsg);

            while (true) {
                NetIncomingMessage msg;

                while ((msg = client.ReadMessage()) != null) {
                    if (msg.MessageType != NetIncomingMessageType.StatusChanged) {
                        Log<NetworkContext>.Error("Unexpected message type " + msg.MessageType + " while connecting");
                        continue;
                    }

                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    Log<NetworkContext>.Info("Status changed to " + status);
                    goto gotConnectionAttemptResult;
                }
            }
        gotConnectionAttemptResult:

            if (client.ConnectionStatus != NetConnectionStatus.Connected) {
                return Maybe<NetworkContext>.Empty;
            }

            // Read in the hail message to populate our server connection with the server player
            {
                NetIncomingMessage msg = client.ServerConnection.RemoteHailMessage;
                NetworkPlayer serverPlayer = SerializationHelpers.Deserialize<NetworkPlayer>(msg.ReadString());
                client.ServerConnection.Tag = serverPlayer;
            }
            NetworkContext context = new NetworkContext(player) {
                _client = client
            };
            return Maybe.Just(context);
        }

        private NetworkContext(NetworkPlayer player) {
            Dispatcher = new NetworkMessageDispatcher();
            LocalPlayer = player;
            _localPlayerEnumerable = new[] { player };

            _connectionMonitors = new List<INetworkConnectionMonitor>();

            Log<NetworkContext>.Info("Created network context for " + player);
        }

        internal void AddConnectionMonitor(INetworkConnectionMonitor monitor) {
            _connectionMonitors.Add(monitor);
        }

        internal void RemoveConnectionMonitor(INetworkConnectionMonitor monitor) {
            if (_connectionMonitors.Remove(monitor) == false) {
                throw new InvalidOperationException("No such monitor to remove");
            }
        }

        /// <summary>
        /// Update the network context; ie, invoke handlers for received network messages if we're a
        /// client or broadcast out received messages if we're a server.
        /// </summary>
        public void Update() {
            NetPeer peer = (NetPeer)_client ?? (NetPeer)_server;

            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null) {
                Log<NetworkContext>.Info("NetworkContext.Update: got message type " + msg.MessageType);

                switch (msg.MessageType) {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Log<NetworkContext>.Warn(msg.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        Log<NetworkContext>.Info("\\" + status + " = " + msg.ReadString());
                        switch (status) {
                            case NetConnectionStatus.Connected:
                                foreach (var montitor in _connectionMonitors) {
                                    montitor.OnConnected((NetworkPlayer)msg.SenderConnection.Tag);
                                }
                                break;

                            case NetConnectionStatus.Disconnected:
                                foreach (var monitor in _connectionMonitors) {
                                    monitor.OnDisconnected((NetworkPlayer)msg.SenderConnection.Tag);
                                }
                                break;
                        }
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        HailMessage hail = SerializationHelpers.Deserialize<HailMessage>(msg.ReadString());

                        // bad password; deny the connection
                        if (hail.Password != _serverPassword) {
                            Log<NetworkContext>.Info("Connection denied (from " + msg.SenderEndPoint + ")");
                            msg.SenderConnection.Deny("Bad password");
                        }

                        // good password; approve the connection
                        else {
                            Log<NetworkContext>.Info("Connection approved (from " + msg.SenderEndPoint + ")");
                            msg.SenderConnection.Tag = hail.Player;

                            NetOutgoingMessage outgoingHail = _server.CreateMessage();
                            outgoingHail.Write(SerializationHelpers.Serialize(LocalPlayer));
                            msg.SenderConnection.Approve(outgoingHail);
                        }
                        break;

                    case NetIncomingMessageType.Data:
                        string serializedMessage = msg.ReadString();
                        Log<NetworkContext>.Info("  " + serializedMessage);
                        NetworkMessageFormat message = SerializationHelpers.Deserialize<NetworkMessageFormat>(serializedMessage);

                        if (IsServer && message.IfServerRebroadcast) {
                            var outgoingMessage = CreateMessage(message.Sender, message.NetworkMessage, broadcast: false);
                            _server.SendToAll(outgoingMessage, NetDeliveryMethod.ReliableOrdered);
                        }

                        Dispatcher.InvokeHandlers(message.Sender, message.NetworkMessage);
                        break;

                    default:
                        throw new InvalidOperationException("Unhandled type: " + msg.MessageType);
                }
                peer.Recycle(msg);
            }
        }

        /// <summary>
        /// Message format for NetIncomingMessageType.Data
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private struct NetworkMessageFormat {
            [JsonProperty]
            public bool IfServerRebroadcast;

            [JsonProperty]
            public NetworkPlayer Sender;

            [JsonProperty]
            public INetworkMessage NetworkMessage;
        }

        /// <summary>
        /// Send the given message to the given recipients.
        /// </summary>
        /// <param name="recipient">The computers that should receive the message.</param>
        /// <param name="message">The message to send.</param>
        public void SendMessage(NetworkMessageRecipient recipient, INetworkMessage message) {
            switch (recipient) {
                case NetworkMessageRecipient.All:
                    if (IsClient) {
                        var msg = CreateMessage(LocalPlayer, message, broadcast: true);
                        _client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
                    }
                    else {
                        var msg = CreateMessage(LocalPlayer, message, broadcast: false);
                        Log<NetworkContext>.Info("Sending message to all connections " + string.Join(", ", _server.Connections.Select(c => ((NetworkPlayer)c.Tag).Name).ToArray()));

                        _server.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                        Dispatcher.InvokeHandlers(LocalPlayer, message);
                    }
                    break;

                case NetworkMessageRecipient.Clients: {
                        if (IsServer == false) {
                            throw new InvalidOperationException("Only the server can send messages to clients");
                        }

                        var msg = CreateMessage(LocalPlayer, message, broadcast: false);
                        NetPeer peer = (NetPeer)_client ?? (NetPeer)_server;

                        // only send the message if we have someone to send it to
                        if (peer.ConnectionsCount > 0) {
                            peer.SendMessage(msg, peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                        }
                        break;
                    }

                case NetworkMessageRecipient.Server:
                    if (IsClient) {
                        var msg = CreateMessage(LocalPlayer, message, broadcast: false);
                        _client.ServerConnection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
                    }
                    else {
                        Dispatcher.InvokeHandlers(LocalPlayer, message);
                    }
                    break;
            }
        }

        /// <summary>
        /// Send the given message to only the specified recipient.
        /// </summary>
        /// <param name="recipient">The player that should receive the message.</param>
        /// <param name="message">Who to send the message to.</param>
        public void SendMessage(NetworkPlayer recipient, INetworkMessage message) {
            NetOutgoingMessage msg = CreateMessage(LocalPlayer, message, broadcast: false);
            NetConnection connection = GetConnection(recipient);
            connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Creates an outgoing message with the given sender, message, broadcast state.
        /// </summary>
        /// <param name="sender">The player who is sending this message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="broadcast">If the server receives this message, should it broadcast it out
        /// to all clients?</param>
        private NetOutgoingMessage CreateMessage(NetworkPlayer sender, INetworkMessage message, bool broadcast) {
            string serialized = SerializationHelpers.Serialize(new NetworkMessageFormat() {
                IfServerRebroadcast = broadcast,
                Sender = sender,
                NetworkMessage = message
            });

            NetPeer peer = (NetPeer)_client ?? (NetPeer)_server;
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write(serialized);
            return msg;
        }
    }

    /// <summary>
    /// Specifies who should receive a network message.
    /// </summary>
    public enum NetworkMessageRecipient {
        /// <summary>
        /// All computers process the message. However, each computer processes each message in the
        /// same order.
        /// </summary>
        /// <remarks>
        /// This message type requires that the message be sent to the server, and then the server
        /// rebroadcast the message.
        /// </remarks>
        All,

        /// <summary>
        /// The message should be processed by only the server.
        /// </summary>
        Server,

        /// <summary>
        /// The message should be processed by all clients and *not* the server. This message type
        /// can only be sent by the server.
        /// </summary>
        Clients,
    }
}