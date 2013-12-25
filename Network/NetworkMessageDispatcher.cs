using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
    /// <summary>
    /// Helper class for managing INetowrkMessageHandlers and dispatching INetworkMessages.
    /// </summary>
    internal class NetworkMessageDispatcher {
        private Dictionary<Type, List<INetworkMessageHandler>> _handlers;

        public NetworkMessageDispatcher() {
            _handlers = new Dictionary<Type, List<INetworkMessageHandler>>();
        }

        private List<INetworkMessageHandler> GetHandlers(Type messageType) {
            List<INetworkMessageHandler> result;
            if (_handlers.TryGetValue(messageType, out result) == false) {
                result = new List<INetworkMessageHandler>();
                _handlers[messageType] = result;
            }
            return result;
        }

        public void AddHandler(INetworkMessageHandler handler) {
            foreach (Type type in handler.HandledTypes) {
                GetHandlers(type).Add(handler);
            }
        }

        public void RemoveHandler(INetworkMessageHandler handler) {
            foreach (Type type in handler.HandledTypes) {
                if (GetHandlers(type).Remove(handler) == false) {
                    throw new InvalidOperationException("Attempt to remove handler failed");
                }
            }
        }

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