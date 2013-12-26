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
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Neon.Network.Core {
    /// <summary>
    /// Holds important information about the current network connection and additionally about
    /// INetworkMessage listeners.
    /// </summary>
    public sealed class NetworkContext {
        /// <summary>
        /// Enumerable container that merely contains LocalPlayer.
        /// </summary>
        private IEnumerable<NetworkPlayer> _localPlayerEnumerable;

        /// <summary>
        /// If the context is a client, then this is the Lidgren.Network client object.
        /// </summary>
        private NetClient _client;

        /// <summary>
        /// If the context is a server, then this is the Lidgren.Network server object.
        /// </summary>
        private NetServer _server;

        /// <summary>
        /// Returns the internal NetPeer instance that represents the core connection.
        /// </summary>
        private NetPeer Peer {
            get {
                return (NetPeer)_client ?? (NetPeer)_server;
            }
        }

        /// <summary>
        /// If we're a server, then this is the password for the server.
        /// </summary>
        private string _serverPassword;

        /// <summary>
        /// If we're a server, this is the list of objects which want to know when a client has
        /// connected or disconnected.
        /// </summary>
        private List<INetworkConnectionMonitor> _connectionMonitors;

        /// <summary>
        /// Networking dispatcher that is used for sending messages.
        /// </summary>
        private NetworkMessageDispatcher _dispatcher;

        /// <summary>
        /// Private constructor for NetworkContext; NetworkContexts can only be created using the
        /// static helper methods.
        /// </summary>
        /// <param name="localPlayer">The local player</param>
        private NetworkContext(NetworkPlayer localPlayer) {
            _dispatcher = new NetworkMessageDispatcher();
            LocalPlayer = localPlayer;
            _localPlayerEnumerable = new[] { localPlayer };

            _connectionMonitors = new List<INetworkConnectionMonitor>();

            Log<NetworkContext>.Info("Created network context with LocalPlayer=" + localPlayer);
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
        /// Creates a new client connection connected to the given IP end point. This method blocks
        /// until we know if the client has either connected or disconnected.
        /// </summary>
        /// <param name="ip">The IP to connect to.</param>
        /// <param name="player">This computer's player.</param>
        /// <param name="password">The password that the server is expecting.</param>
        /// <returns></returns>
        public static Maybe<NetworkContext> CreateClient(string ip, NetworkPlayer player, string password) {
            NetClient client = new NetClient(Configuration.GetConfiguration(server: false));
            client.Start();

            // Write out our hail message
            {
                NetOutgoingMessage hailMsg = client.CreateMessage();
                HailMessageFormat hail = new HailMessageFormat() {
                    Player = player,
                    Password = password
                };
                string serializedHail = SerializationHelpers.Serialize(hail);
                hailMsg.Write(serializedHail);

                Log<NetworkContext>.Info("Trying to connect to " + ip + " on port " + Configuration.Port + " with hailing message " + serializedHail);

                // Try to connect to the server
                client.Connect(ip, Configuration.Port, hailMsg);
            }

            // Block until we know if we have connected or disconnected.
            while (true) {
                NetIncomingMessage msg;

                while ((msg = client.ReadMessage()) != null) {
                    if (msg.MessageType != NetIncomingMessageType.StatusChanged) {
                        Log<NetworkContext>.Error("While attempting to connect to server, got unexpected message type " + msg.MessageType);
                        continue;
                    }

                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    Log<NetworkContext>.Info("While attempting to connect to server, status changed to " + status);
                    goto gotConnectionAttemptResult;
                }

                Thread.Sleep(0);
            }
        gotConnectionAttemptResult:

            // If the connection status is not connected, then we failed, so just return an empty
            // network context
            if (client.ConnectionStatus != NetConnectionStatus.Connected) {
                return Maybe<NetworkContext>.Empty;
            }

            // We're connected to the server! Read in the hail message to populate our server
            // connection with the server player instance
            {
                NetIncomingMessage msg = client.ServerConnection.RemoteHailMessage;
                NetworkPlayer serverPlayer = SerializationHelpers.Deserialize<NetworkPlayer>(msg.ReadString());
                client.ServerConnection.Tag = serverPlayer;
                NetworkContext context = new NetworkContext(player) {
                    _client = client
                };
                return Maybe.Just(context);
            }
        }

        /// <summary>
        /// Returns true if this NetworkConext is acting as a server.
        /// </summary>
        public bool IsServer {
            get {
                return _server != null;
            }
        }

        /// <summary>
        /// Returns true if this NetworkContext is acting as a client.
        /// </summary>
        public bool IsClient {
            get {
                return _client != null;
            }
        }

        /// <summary>
        /// The local player.
        /// </summary>
        public NetworkPlayer LocalPlayer {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the given NetworkPlayer is the server.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True if the player is the server, otherwise false.</returns>
        public bool IsPlayerServer(NetworkPlayer player) {
            if (IsServer) {
                return player == (NetworkPlayer)_server.Tag;
            }

            if (IsClient) {
                return player == (NetworkPlayer)_client.ServerConnection.Tag;
            }

            throw new InvalidOperationException("Bad internal state; not a server or client");
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

        /// <summary>
        /// Adds the given message handler to the network context.
        /// </summary>
        /// <param name="handler">The network message handler to add.</param>
        public void AddMessageHandler(INetworkMessageHandler handler) {
            _dispatcher.AddHandler(handler);
        }

        /// <summary>
        /// Removes the given message handler from the context. If the dispatcher was not previously
        /// contained in the context, then an exception is thrown.
        /// </summary>
        /// <param name="handler">The network message handler to remove.</param>
        public void RemoveMessageHandler(INetworkMessageHandler handler) {
            _dispatcher.RemoveHandler(handler);
        }

        /// <summary>
        /// Add a new connection monitor listener. This allows for client code to be notified when
        /// another player connects or disconnects from the network.
        /// </summary>
        /// <param name="monitor">The connection monitor to add.</param>
        public void AddConnectionMonitor(INetworkConnectionMonitor monitor) {
            _connectionMonitors.Add(monitor);
        }

        /// <summary>
        /// Remove a previously added connection monitor. If the monitor was not found when removing
        /// it, an exception is thrown.
        /// </summary>
        /// <param name="monitor">The connection monitor to remove.</param>
        public void RemoveConnectionMonitor(INetworkConnectionMonitor monitor) {
            if (_connectionMonitors.Remove(monitor) == false) {
                throw new InvalidOperationException("No such monitor to remove");
            }
        }

        /// <summary>
        /// Update the network context; ie, invoke handlers for received network messages if we're a
        /// client or broadcast out received messages if we're a server.
        /// </summary>
        public void Update() {
            NetPeer peer = Peer;

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
                        var hail = SerializationHelpers.Deserialize<HailMessageFormat>(msg.ReadString());

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

                        _dispatcher.InvokeHandlers(message.Sender, message.NetworkMessage);
                        break;

                    default:
                        throw new InvalidOperationException("Unhandled type: " + msg.MessageType);
                }
                peer.Recycle(msg);
            }
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
                        _dispatcher.InvokeHandlers(LocalPlayer, message);
                    }
                    break;

                case NetworkMessageRecipient.Clients: {
                        if (IsServer == false) {
                            throw new InvalidOperationException("Only the server can send messages to clients");
                        }

                        var msg = CreateMessage(LocalPlayer, message, broadcast: false);
                        NetPeer peer = Peer;

                        // only send the message if we have someone to send it to
                        if (peer.ConnectionsCount > 0) {
                            peer.SendMessage(msg, peer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                        }
                        break;
                    }

                case NetworkMessageRecipient.Server: {
                        if (IsClient == false) {
                            throw new InvalidOperationException("Only clients can send messages to the server");
                        }

                        var msg = CreateMessage(LocalPlayer, message, broadcast: false);
                        _client.ServerConnection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
                        break;
                    }
            }
        }

        /// <summary>
        /// Send the given message to only the specified recipient.
        /// </summary>
        /// <param name="recipient">The player that should receive the message.</param>
        /// <param name="message">Who to send the message to.</param>
        public void SendMessage(NetworkPlayer recipient, INetworkMessage message) {
            // If we're sending the message to ourselves (strange, but fine), then just directly
            // invoke the handlers -- there is not going to be a network connection
            if (recipient == LocalPlayer) {
                _dispatcher.InvokeHandlers(LocalPlayer, message);
            }

            // Otherwise, lookup the network connection and send the message to that connection
            else {
                NetOutgoingMessage msg = CreateMessage(LocalPlayer, message, broadcast: false);
                NetConnection connection = GetConnection(recipient);
                connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
            }
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

            NetPeer peer = Peer;
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write(serialized);
            return msg;
        }

        /// <summary>
        /// Helper method to lookup the network connection based on the given network player.
        /// </summary>
        private NetConnection GetConnection(NetworkPlayer player) {
            NetPeer peer = Peer;

            for (int i = 0; i < peer.ConnectionsCount; ++i) {
                NetConnection connection = peer.Connections[i];

                if (player == (NetworkPlayer)connection.Tag) {
                    return connection;
                }
            }

            throw new InvalidOperationException("No connection for player " + player);
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
        /// Hail message format used when connecting to a server.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private class HailMessageFormat {
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
    }
}