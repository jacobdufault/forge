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

using Forge.Networking.Core;
using Forge.Utilities;
using System;
using System.Collections.Generic;

namespace Forge.Networking.Chat {
    /// <summary>
    /// Processes ChatNetworkMessages and adds them to a displayable message list depending on if
    /// the local player should be allowed to see the message.
    /// </summary>
    internal class ChatMessageHandler : INetworkMessageHandler {
        /// <summary>
        /// The local player.
        /// </summary>
        private Player _localPlayer;

        /// <summary>
        /// All received chat messages.
        /// </summary>
        public List<ReceivedChatMessage> AllMessages;

        /// <summary>
        /// All chat messages that should be displayed.
        /// </summary>
        public List<ReceivedChatMessage> DisplayableMessages;

        public ChatMessageHandler(NetworkContext context) {
            _localPlayer = context.LocalPlayer;

            AllMessages = new List<ReceivedChatMessage>();
            DisplayableMessages = new List<ReceivedChatMessage>();
        }

        public Type[] HandledTypes {
            get { return new[] { typeof(ChatNetworkMessage) }; }
        }

        public void HandleNetworkMessage(Player sender, INetworkMessage message) {
            ChatNetworkMessage m = (ChatNetworkMessage)message;

            var receivedMessage = new ReceivedChatMessage() {
                RecieveTime = DateTime.Now,
                Sender = m.Sender,
                Content = m.Content
            };
            AllMessages.Add(receivedMessage);

            if (ShouldDisplay(m.Receivers)) {
                DisplayableMessages.Add(receivedMessage);
            }
        }

        /// <summary>
        /// Returns true if this computer should display the message for the given set of message
        /// receivers.
        /// </summary>
        private bool ShouldDisplay(Maybe<List<Player>> receivers) {
            if (receivers.IsEmpty) {
                return true;
            }

            return receivers.Value.Contains(_localPlayer);
        }
    }
}