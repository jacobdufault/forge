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
        /// How much time has elapsed since the last time we popped an update?
        /// </summary>
        private float _interpolationAccumulator;

        /// <summary>
        /// Accumulator used for clients when they are updating (so if we receive two updates really
        /// quickly, we don't want to actually execute those updates immediately; we instead want to
        /// wait until we can update them).
        /// </summary>
        private float _clientUpdateAccumulator;

        /// <summary>
        /// How much time has elapsed since the last time we updated?
        /// </summary>
        private float _serverUpdateAccumulator;

        /// <summary>
        /// How much time should elapse between updates for us to meet our target updates/second?
        /// </summary>
        private float _updateDeltaMs;

        /// <summary>
        /// The number of updates we second the game should run at.
        /// </summary>
        public int TargetUpdatesPerSecond {
            get {
                return (int)(1000.0f / _updateDeltaMs);
            }
            set {
                _updateDeltaMs = 1000.0f / value;
            }
        }

        /// <summary>
        /// Create a new AutomaticTurnGame.
        /// </summary>
        /// <param name="context">The networking context.</param>
        /// <param name="targetUpdatesPerSecond">The number of updates/turns that will occur every
        /// second.</param>
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

        /// <summary>
        /// Clean up the game from the NetworkContext.
        /// </summary>
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

        /// <summary>
        /// Configure the lag between giving input and actually receiving that input. A lower value
        /// will cause stuttering on slow networks, but user responsiveness will be higher. A higher
        /// value will cause less stuttering, but lower user responsiveness.
        /// </summary>
        public int TurnDelay {
            get {
                return _clientHandler.TurnDelay;
            }

            set {
                if (value <= 0) {
                    throw new ArgumentException("newDelay must be > 0");
                }

                _context.SendMessage(NetworkMessageRecipient.All, new AdjustTurnDelayNetworkMessage() {
                    NewDelay = value
                });
            }
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
        /// enough time has elapsed.
        /// </summary>
        /// <param name="deltaTime">The amount of time that has elapsed since the last call to
        /// Update.</param>
        public void Update(float deltaTime) {
            _interpolationAccumulator += deltaTime;
            _clientUpdateAccumulator += deltaTime;

            if (_context.IsServer) {
                _serverUpdateAccumulator += deltaTime;

                while (_serverUpdateAccumulator > _updateDeltaMs) {
                    _serverUpdateAccumulator -= _updateDeltaMs;
                    _serverHandler.SendCommands();
                }
            }
        }

        /// <summary>
        /// Returns how far along we are until the next update.
        /// </summary>
        public float InterpolationPercentage {
            get {
                return _interpolationAccumulator / _updateDeltaMs;
            }
        }

        public bool TryUpdate(out List<IGameCommand> commands) {
            // we can only update if the client has received an update from the server
            if (_clientHandler.HasUpdate == false) {
                commands = null;
                return false;
            }

            // we can only update if enough time has elapsed since the last update and there is not
            // a large backlog
            if (_clientHandler.HasLargeBacklog == false &&
                _clientUpdateAccumulator > _updateDeltaMs == false) {
                commands = null;
                return false;
            }

            // we can update!

            // reset interpolation (hopefully it was near 1.0)
            _interpolationAccumulator = 0;

            // reduce the amount of time we have until we can do our next client update
            _clientUpdateAccumulator -= _updateDeltaMs;

            // and actually get the update
            commands = _clientHandler.PopUpdate();
            return true;
        }
    }
}