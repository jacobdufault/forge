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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Forge.Networking.Chat {
    /// <summary>
    /// A network message that is used when sending chat messages.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ChatNetworkMessage : INetworkMessage {
        /// <summary>
        /// The content of the chat message.
        /// </summary>
        [JsonProperty]
        public string Content;

        /// <summary>
        /// The player that sent the message.
        /// </summary>
        [JsonProperty]
        public Player Sender;

        /// <summary>
        /// Specifies what players should see the given message. This will be empty if every player
        /// should see the message, and non-empty if not everyone should see it.
        /// </summary>
        [JsonProperty]
        public Maybe<List<Player>> Receivers;
    }
}