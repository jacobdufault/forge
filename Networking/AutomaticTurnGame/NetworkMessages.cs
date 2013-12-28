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

using Neon.Networking.Core;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Neon.Networking.AutomaticTurnGame {
    [JsonObject(MemberSerialization.OptIn)]
    internal class EndTurnNetworkMessage : INetworkMessage {
        /// <summary>
        /// Every command (from every computer) that should be issued.
        /// </summary>
        [JsonProperty]
        public List<IGameCommand> Commands;

        /// <summary>
        /// The update the commands should be issued on.
        /// </summary>
        [JsonProperty]
        public int OnUpdate;
    }

    /// <summary>
    /// Message that goes from clients to the server specifying commands that have been issued.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class SubmitCommandsNetworkMessage : INetworkMessage {
        /// <summary>
        /// The requested commands to issue.
        /// </summary>
        [JsonProperty]
        public List<IGameCommand> Commands;
    }

    /// <summary>
    /// Message that only the server processes to adjust the turn delay.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AdjustTurnDelayNetworkMessage : INetworkMessage {
        /// <summary>
        /// The new delay for the game.
        /// </summary>
        [JsonProperty]
        public int NewDelay;
    }
}