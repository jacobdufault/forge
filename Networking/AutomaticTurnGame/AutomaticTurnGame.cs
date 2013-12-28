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
    /// <summary>
    /// Supports turn-based games where there are a high number of game turns per second (>5) that
    /// are automatically ended.
    /// </summary>
    public class AutomaticTurnGame {
        private GameServerHandler _serverHandler;
        private GameClientHandler _clientHandler;
        private NetworkContext _context;

        /// <summary>
        /// How much time has elapsed since the last time we updated?
        /// </summary>
        private double _accumulator;

        /// <summary>
        /// How much time should elapse between updates for us to meet our target updates/second?
        /// </summary>
        private double _updateDeltaMs;

        public int TargetUpdatesPerSecond {
            get {
                return (int)(1000.0 / _updateDeltaMs);
            }
            set {
                _updateDeltaMs = 1000.0 / value;
            }
        }

        public AutomaticTurnGame(NetworkContext context, int targetUpdatesPerSecond) {
            TargetUpdatesPerSecond = targetUpdatesPerSecond;

            _context = context;

            if (_context.IsServer) {
                _serverHandler = new GameServerHandler(_context);
                _context.AddMessageHandler(_serverHandler);
            }

            _clientHandler = new GameClientHandler();
            _context.AddMessageHandler(_clientHandler);
        }

        public void Dispose() {
            if (_serverHandler != null) {
                _context.RemoveMessageHandler(_serverHandler);
                _serverHandler = null;
            }

            if (_clientHandler != null) {
                _context.RemoveMessageHandler(_clientHandler);
                _clientHandler = null;
            }
        }

        public void AdjustTurnDelay(int newDelay) {
            if (newDelay <= 0) {
                throw new ArgumentException("newDelay must be > 0");
            }

            _context.SendMessage(NetworkMessageRecipient.Server, new AdjustTurnDelayNetworkMessage() {
                NewDelay = newDelay
            });
        }

        /// <summary>
        /// Send the given set of game commands to the server. It will be processed at a later point
        /// by every client.
        /// </summary>
        /// <param name="commands">The commands to send.</param>
        public void SendCommand(List<IGameCommand> commands) {
            _context.SendMessage(NetworkMessageRecipient.Server, new SubmitCommandsNetworkMessage() {
                Commands = commands
            });
        }

        /// <summary>
        /// Update the game. Potentially dispatch game commands for execution by all computers if
        /// enough time has elapsed. This method does nothing on clients; only if the local player
        /// is the server does this method execute code.
        /// </summary>
        /// <param name="deltaTime">The amount of time that has elapsed since the last call to
        /// Update.</param>
        public void Update(float deltaTime) {
            if (_context.IsServer) {
                _accumulator += deltaTime;

                while (_accumulator > _updateDeltaMs) {
                    _accumulator -= _updateDeltaMs;
                    _serverHandler.SendCommands();
                }
            }
        }
    }
}