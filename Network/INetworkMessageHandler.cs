using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {

    /// <summary>
    /// Client code that is executed upon the receipt of a network message.
    /// </summary>
    public interface INetworkMessageHandler {
        /// <summary>
        /// The types that this message handler can process.
        /// </summary>
        Type[] HandledTypes {
            get;
        }

        /// <summary>
        /// Handle a network message.
        /// </summary>
        /// <param name="sender">The player that sent the message.</param>
        /// <param name="message">The message itself (an instance of a type from
        /// HandledTypes) .</param>
        void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message);
    }

    /// <summary>
    /// Base type that all INetworkMessageHandlers should extend from (for a simplified API).
    /// </summary>
    /// <typeparam name="TNetworkMessage">The type of message that this handler handles.</typeparam>
    public abstract class BaseNetworkMessageHandler<TNetworkMessage> : INetworkMessageHandler {
        protected abstract void HandleNetworkMessage(NetworkPlayer sender, TNetworkMessage message);

        void INetworkMessageHandler.HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            HandleNetworkMessage(sender, (TNetworkMessage)message);
        }

        Type[] INetworkMessageHandler.HandledTypes {
            get {
                return new[] { typeof(TNetworkMessage) };
            }
        }
    }
}