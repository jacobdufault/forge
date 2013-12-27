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

using System;

namespace Neon.Networking.Core {

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