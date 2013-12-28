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
        private Dictionary<int, List<IGameCommand>> _commands;
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
        private int TurnDelay = 2;

        public GameServerHandler(NetworkContext context) {
            _context = context;
            _commands = new Dictionary<int, List<IGameCommand>>();
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

            List<IGameCommand> commands;
            if (_commands.TryGetValue(_lastSentTurn, out commands)) {
                _commands.Remove(_lastSentTurn);
            }
            else {
                commands = new List<IGameCommand>();
            }

            _context.SendMessage(NetworkMessageRecipient.All, new EndTurnNetworkMessage() {
                Commands = commands,
                OnUpdate = _lastSentTurn + TurnDelay
            });
        }

        public void HandleNetworkMessage(Player sender, INetworkMessage message) {
            if (message is SubmitCommandsNetworkMessage) {
                SubmitCommandsNetworkMessage m = (SubmitCommandsNetworkMessage)message;

                List<IGameCommand> allCommands;
                if (_commands.TryGetValue(_lastSentTurn, out allCommands) == false) {
                    allCommands = new List<IGameCommand>();
                    _commands[_lastSentTurn] = allCommands;
                }

                allCommands.AddRange(m.Commands);
            }

            else if (message is AdjustTurnDelayNetworkMessage) {
                AdjustTurnDelayNetworkMessage m = (AdjustTurnDelayNetworkMessage)message;
                TurnDelay = m.NewDelay;
            }
        }
    }

    internal class GameClientHandler : BaseNetworkMessageHandler<EndTurnNetworkMessage> {
        private Queue<List<IGameCommand>> _updates = new Queue<List<IGameCommand>>();

        public bool HasUpdate() {
            return _updates.Count > 0;
        }

        public List<IGameCommand> PopUpdate() {
            return _updates.Dequeue();
        }

        protected override void HandleNetworkMessage(Player sender, EndTurnNetworkMessage message) {
            _updates.Enqueue(message.Commands);
        }
    }
}