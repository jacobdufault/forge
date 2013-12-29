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
using System.Collections.Generic;

namespace Forge.Networking.Chat {
    /// <summary>
    /// Contains the core APIS for sending and receiving chat messages.
    /// </summary>
    public sealed class ChatManager {
        /// <summary>
        /// The networking context to use for retrieving our local player and for adding our
        /// ChatNetworkMessage handler.
        /// </summary>
        private NetworkContext _context;

        /// <summary>
        /// The ChatMessageHandler we use to get chat messages from.
        /// </summary>
        private ChatMessageHandler _handler;

        /// <summary>
        /// Construct a new ChatManager using the given networking context and the given object for
        /// mapping network players to a directed player relation graph.
        /// </summary>
        /// <param name="context">The networking context.</param>
        /// <param name="relationGraph">The player relationship mapper.</param>
        public ChatManager(NetworkContext context, IPlayerRelationGraph relationGraph) {
            _context = context;
            _handler = new ChatMessageHandler(_context, relationGraph);
            _context.AddMessageHandler(_handler);
        }

        /// <summary>
        /// Cleans up the ChatManager from the NetworkContext it was constructed with.
        /// </summary>
        public void Dispose() {
            if (_handler != null) {
                _context.RemoveMessageHandler(_handler);
                _handler = null;
            }
        }

        /// <summary>
        /// All of the chat messages that have been received that should be displayed.
        /// </summary>
        public List<ReceivedChatMessage> DisplayableMessages {
            get {
                return _handler.DisplayableMessages;
            }
        }

        /// <summary>
        /// All received chat messages.
        /// </summary>
        public List<ReceivedChatMessage> AllMessages {
            get {
                return _handler.AllMessages;
            }
        }

        /// <summary>
        /// Send a chat message to all players that have the given relationship with the sending
        /// player.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="requiredRelation">The relationship that the local player has to have with
        /// the message candidate in order to send the chat message.</param>
        public void SendMessage(string message, PlayerRelation requiredRelation) {
            _context.SendMessage(NetworkMessageRecipient.All, new ChatNetworkMessage() {
                Content = message,
                Sender = _context.LocalPlayer,
                RequiredSenderToLocalRelation = Maybe.Just(requiredRelation)
            });
        }

        /// <summary>
        /// Sends a chat message to every player.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(string message) {
            _context.SendMessage(NetworkMessageRecipient.All, new ChatNetworkMessage() {
                Content = message,
                Sender = _context.LocalPlayer,
                RequiredSenderToLocalRelation = Maybe<PlayerRelation>.Empty
            });
        }
    }

}