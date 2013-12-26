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
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Network.Chat {
    /// <summary>
    /// Processes ChatNetworkMessages and adds them to a displayable message list depending on if
    /// the local player should be allowed to see the message.
    /// </summary>
    internal class ChatMessageHandler : INetworkMessageHandler {
        /// <summary>
        /// The local player.
        /// </summary>
        private NetworkPlayer _localPlayer;

        /// <summary>
        /// The relation determination object used to determine the relation between the sender of
        /// the chat message and the local player.
        /// </summary>
        private IPlayerRelationDetermination _relations;

        /// <summary>
        /// All received chat messages.
        /// </summary>
        public List<ReceivedChatMessage> AllMessages;

        /// <summary>
        /// All chat messages that should be displayed.
        /// </summary>
        public List<ReceivedChatMessage> DisplayableMessages;

        public ChatMessageHandler(NetworkContext context, IPlayerRelationDetermination relations) {
            _localPlayer = context.LocalPlayer;
            _relations = relations;

            AllMessages = new List<ReceivedChatMessage>();
            DisplayableMessages = new List<ReceivedChatMessage>();
        }

        public Type[] HandledTypes {
            get { return new[] { typeof(ChatNetworkMessage) }; }
        }

        public void HandleNetworkMessage(NetworkPlayer sender, INetworkMessage message) {
            ChatNetworkMessage m = (ChatNetworkMessage)message;

            var receivedMessage = new ReceivedChatMessage() {
                RecieveTime = DateTime.Now,
                Sender = m.Sender,
                Content = m.Content
            };
            AllMessages.Add(receivedMessage);

            if (ShouldDisplay(m.RequiredSenderToLocalRelation, m.Sender)) {
                DisplayableMessages.Add(receivedMessage);
            }
        }

        /// <summary>
        /// Returns true if either the required relation if empty or if the sender has the required
        /// relation w.r.t. to the player.
        /// </summary>
        private bool ShouldDisplay(Maybe<PlayerRelation> requiredRelation, NetworkPlayer sender) {
            if (requiredRelation.IsEmpty) {
                return true;
            }

            // Get the relation that the sender has towards us
            PlayerRelation currentRelation = _relations.GetDirectedRelation(sender, _localPlayer);
            return currentRelation == requiredRelation.Value;
        }
    }
}