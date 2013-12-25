using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Network {
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