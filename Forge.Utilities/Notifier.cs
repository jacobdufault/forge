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

namespace Forge.Utilities {
    /// <summary>
    /// Wraps the notification pattern, where something happens multiple times but the listeners
    /// should only be notified once.
    /// </summary>
    /// <remarks>
    /// The Notifier API is thread-safe.
    /// </remarks>
    /// <typeparam name="ParamType">The type of the parameter.</typeparam>
    public class Notifier<ParamType> {
        /// <summary>
        /// Have we already notified the listeners?
        /// </summary>
        private AtomicActivation _activated;

        /// <summary>
        /// Parameter to notify listeners with.
        /// </summary>
        private ParamType _notificationParam;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notifier{ParamType}"/> class.
        /// </summary>
        /// <param name="param">The parameter to notify listeners with.</param>
        public Notifier(ParamType param) {
            _notificationParam = param;
            _activated = new AtomicActivation();
        }

        /// <summary>
        /// Resets this notifier so that it will notify listeners again.
        /// </summary>
        public void Reset() {
            _activated.Reset();
        }

        /// <summary>
        /// Notify the listeners if they have not already been notified.
        /// </summary>
        public void Notify() {
            if (_activated.TryActivate()) {
                if (_listener != null) {
                    _listener(_notificationParam);
                }
            }
        }

        private Action<ParamType> _listener;

        /// <summary>
        /// Allows objects to listen for notifications. If the notifier has already been triggered,
        /// then the added listener is immediately called.
        /// </summary>
        public event Action<ParamType> Listener {
            add {
                lock (this) {
                    _listener = (Action<ParamType>)Delegate.Combine(_listener, value);
                }

                if (_activated.IsActivated) {
                    value(_notificationParam);
                }
            }

            remove {
                lock (this) {
                    _listener = (Action<ParamType>)Delegate.Remove(_listener, value);
                }
            }
        }
    }
}