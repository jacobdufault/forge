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

namespace Neon.Network.Core {
    /// <summary>
    /// Specifies which computers an INetworkMessage should be delivered to.
    /// </summary>
    public enum NetworkMessageRecipient {
        /// <summary>
        /// All computers process the message. Each computer will processes each message in the same
        /// order. That is, computer A sends message A, and computer B sends message B, both
        /// computers will process both messages in the same order (whether it be A then B or B then
        /// A is undefined, but both computers will select the same (a,b) or (b,a) group).
        /// </summary>
        /// <remarks>
        /// In regards to implementation details, this message type requires that the message be
        /// sent to the server before any computer can process it (for ordering purposes). The
        /// server then rebroadcasts the message to every computer for execution. This is not
        /// particularly lightweight, but it does simplify networking logic.
        /// </remarks>
        All,

        /// <summary>
        /// The message should be processed by *only* the server. This message type can only be sent
        /// by clients.
        /// </summary>
        Server,

        /// <summary>
        /// The message should be processed by all clients but *not* the server. This message type
        /// can only be sent by the server.
        /// </summary>
        Clients,
    }
}