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
using System;
using System.Collections.Generic;

namespace Forge.Networking.AutomaticTurnGame {
    internal class GameServerHandler : INetworkMessageHandler {
        internal static readonly int DefaultTurnDelay = 2;

        private DelayedMessageAccumulator _commands;
        private NetworkContext _context;
        private int _lastSentTurn;

        /// <summary>
        /// If the local computer issues a command turn N, then the command will actually be
        /// executed on turn (N+TurnDelay). This parameter heavily impacts responsiveness. This
        /// value is only used on the server but can be changed by any computer on the network. A
        /// low value will mean that user input will get processed more quickly, but the game is
        /// more likely to stutter from a missed update.
        /// </summary>
        // TODO: can we self-balance this value as the game runs?
        public int TurnDelay {
            get {
                return _turnDelay;
            }
            private set {
                _turnDelay = value;
                _commands.Resize(value);
            }
        }
        private int _turnDelay;

        public GameServerHandler(NetworkContext context) {
            _context = context;
            _turnDelay = DefaultTurnDelay;
            _commands = new DelayedMessageAccumulator(DefaultTurnDelay);
        }

        public Type[] HandledTypes {
            get {
                return new[] {
                    typeof(AdjustTurnDelayNetworkMessage),
                    typeof(SubmitCommandsNetworkMessage)
                };
            }
        }

        /// <summary>
        /// Sends commands out for the network turn.
        /// </summary>
        public void SendCommands() {
            ++_lastSentTurn;

            _context.SendMessage(NetworkMessageRecipient.All, new EndTurnNetworkMessage() {
                Commands = _commands.Take(),
                OnUpdate = _lastSentTurn
            });
        }

        public void HandleNetworkMessage(Player sender, INetworkMessage message) {
            if (message is SubmitCommandsNetworkMessage) {
                SubmitCommandsNetworkMessage m = (SubmitCommandsNetworkMessage)message;

                int offset = Math.Min(_lastSentTurn, m.SubmittedOn + _turnDelay);
                _commands.Add(offset, m.Commands);
            }

            else if (message is AdjustTurnDelayNetworkMessage) {
                AdjustTurnDelayNetworkMessage m = (AdjustTurnDelayNetworkMessage)message;
                TurnDelay = m.NewDelay;
            }
        }

        /// <summary>
        /// Helper class that simplifies the management of delayed messages.
        /// </summary>
        /// <typeparam name="T">The type of message to store.</typeparam>
        private class DelayedMessageAccumulator {
            private List<IGameCommand>[] _queue;
            private int _head;

            /// <summary>
            /// Construct a new circular queue with the given capacity.
            /// </summary>
            /// <param name="capacity">How big the queue should be.</param>
            public DelayedMessageAccumulator(int capacity) {
                _queue = new List<IGameCommand>[capacity];
                for (int i = 0; i < _queue.Length; ++i) {
                    _queue[i] = new List<IGameCommand>();
                }
            }

            public void Resize(int length) {
                List<IGameCommand>[] newQueue = new List<IGameCommand>[length];
                Array.Copy(_queue, newQueue, Math.Max(_queue.Length, newQueue.Length));

                // the old queue is bigger than the new queue; we need to condense values
                if (newQueue.Length < _queue.Length) {
                    for (int i = newQueue.Length; i < _queue.Length; ++i) {
                        newQueue[newQueue.Length - 1].AddRange(_queue[i]);
                    }
                }

                // the new queue is bigger than the original queue; we need to populate the null
                // values
                else {
                    for (int i = _queue.Length; i < newQueue.Length; ++i) {
                        newQueue[i] = new List<IGameCommand>();
                    }
                }

                _queue = newQueue;
            }

            public List<IGameCommand> Take() {
                List<IGameCommand> element = _queue[_head];
                _queue[_head] = new List<IGameCommand>();
                _head = (_head + 1) % _queue.Length;
                return element;
            }

            public void Add(int offset, List<IGameCommand> commands) {
                int index = (_head + offset) % _queue.Length;
                _queue[index].AddRange(commands);
            }
        }
    }

    internal class GameClientHandler : INetworkMessageHandler {
        private Queue<List<IGameCommand>> _updates = new Queue<List<IGameCommand>>();

        /// <summary>
        /// Read only value to get the current turn delay that the server is using.
        /// </summary>
        public int TurnDelay {
            get;
            private set;
        }

        public GameClientHandler() {
            TurnDelay = GameServerHandler.DefaultTurnDelay;
        }

        /// <summary>
        /// Does the client has an update ready to be executed?
        /// </summary>
        public bool HasUpdate {
            get {
                return _updates.Count > 0;
            }
        }

        /// <summary>
        /// Does the client have a large backlog of updates that are waiting to be executed?
        /// </summary>
        public bool HasLargeBacklog {
            get {
                return _updates.Count > 3;
            }
        }

        public List<IGameCommand> PopUpdate() {
            return _updates.Dequeue();
        }

        public void HandleNetworkMessage(Player sender, INetworkMessage message) {
            if (message is EndTurnNetworkMessage) {
                _updates.Enqueue(((EndTurnNetworkMessage)message).Commands);
            }

            if (message is AdjustTurnDelayNetworkMessage) {
                TurnDelay = ((AdjustTurnDelayNetworkMessage)message).NewDelay;
            }
        }

        public Type[] HandledTypes {
            get {
                return new[] {
                    typeof(AdjustTurnDelayNetworkMessage),
                    typeof(EndTurnNetworkMessage)
                };
            }
        }

    }
}