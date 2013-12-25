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

using Neon.Network.Core;

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

    public sealed class ChatNetworkMessage : INetworkMessage {
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