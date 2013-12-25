using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
    /// <summary>
    /// This class serves as a registry for INetworkMessageHandlers. It supports efficient lookup of
    /// message type to message handler responders.
    /// </summary>
    /// <remarks>
    /// In the overall context of Neon.Network, this class serves a critical function as the core
    /// mechanism for the event-based message processing loop. The NetworkContext is the primary
    /// user of this type. NetworkContext receives and sends out INetworkMessages; when it receives
    /// an INetworkMessage, it checks its NetworkMessageDispatcher to see if there are any handlers
    /// that need to be invoked.
    /// </remarks>
    public sealed class NetworkMessageDispatcher {
        /// <summary>
        /// The message handlers; the key is the type of message, and the value is the list of
        /// handlers that can respond to that message type.
        /// </summary>
        private Dictionary<Type, List<INetworkMessageHandler>> _handlers;

        /// <summary>
        /// Create a new NetworkMessageDispatcher that has no registered message handlers.
        /// </summary>
        public NetworkMessageDispatcher() {
            _handlers = new Dictionary<Type, List<INetworkMessageHandler>>();
        }

        /// <summary>
        /// Helper method to get all message handlers that can respond to the given message type.
        /// </summary>
        private List<INetworkMessageHandler> GetHandlers(Type messageType) {
            List<INetworkMessageHandler> result;
            if (_handlers.TryGetValue(messageType, out result) == false) {
                result = new List<INetworkMessageHandler>();
                _handlers[messageType] = result;
            }
            return result;
        }

        /// <summary>
        /// Adds a message handler.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        public void AddHandler(INetworkMessageHandler handler) {
            foreach (Type type in handler.HandledTypes) {
                GetHandlers(type).Add(handler);
            }
        }

        /// <summary>
        /// Remove the message handler. Throws an exception if the handler was not previously added.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveHandler(INetworkMessageHandler handler) {
            foreach (Type type in handler.HandledTypes) {
                if (GetHandlers(type).Remove(handler) == false) {
                    throw new InvalidOperationException("Attempt to remove handler failed");
                }
            }
        }

        /// <summary>
        /// Invoke all registered INetworkMessageHandlers for the given message and sender.
        /// </summary>
        internal void InvokeHandlers(NetworkPlayer sender, INetworkMessage message) {
            var handlers = GetHandlers(message.GetType());

            if (handlers.Count == 0) {
                Log<NetworkMessageDispatcher>.Warn("No handlers for message " + message);
            }

            for (int i = 0; i < handlers.Count; ++i) {
                handlers[i].HandleNetworkMessage(sender, message);
            }
        }
    }
}