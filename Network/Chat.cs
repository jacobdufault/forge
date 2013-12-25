using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network.Chat {
    /// <summary>
    /// Specifies the intended recipient of a chat message.
    /// </summary>
    public enum ChatRecipient {
        /// <summary>
        /// Send a message to everyone.
        /// </summary>
        All,

        /// <summary>
        /// Send a message to only friendly players.
        /// </summary>
        Friendly,

        /// <summary>
        /// Send a message to only enemy players.
        /// </summary>
        Enemy
    }

    public class ChatNetworkMessage : INetworkMessage {
        /// <summary>
        /// The content of the chat message.
        /// </summary>
        public string Content;

        /// <summary>
        /// The player that sent the message.
        /// </summary>
        public NetworkPlayer Sender;

        /// <summary>
        /// The players that the message is intended for.
        /// </summary>
        public ChatRecipient Recipient;
    }

    /*
    /// <summary>
    /// Core API for sending chat messages. Subscribe to ChatNetworkMessage to receive chat
    /// messages.
    /// </summary>
    public static class Chat {
        /// <summary>
        /// Send a chat message.
        /// </summary>
        /// <param name="context">The networking context that will be used to send the
        /// message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="recipient">The players that should receive the message.</param>
        public static void SendMessage(NetworkContext context, string message,
            ChatRecipient recipient) {
            context.SendMessage(new ChatNetworkMessage() {
                Content = message,
                Recipient = recipient,
                Sender = context.LocalPlayer
            });
        }
    }
    */
}