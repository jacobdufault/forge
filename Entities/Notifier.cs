using Neon.Utilities;
using System;

namespace Neon.Entities {
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
        /// Allows objects to listen for notifications.
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