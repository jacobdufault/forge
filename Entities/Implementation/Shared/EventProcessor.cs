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

using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Handles event dispatch. Events are queued up until some point in time and then they are
    /// dispatched.
    /// </summary>
    internal class EventNotifier : IEventNotifier {
        /// <summary>
        /// Event handlers.
        /// </summary>
        private Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();

        /// <summary>
        /// The queued set of events that have occurred; any thread can write to this list.
        /// </summary>
        private List<object> _events = new List<object>();

        /// <summary>
        /// Events that are currently being dispatched. This is only read from (its values are
        /// retrieved from _events).
        /// </summary>
        private List<object> _dispatchingEvents = new List<object>();

        /// <summary>
        /// Called when an event has been dispatched to this event processor.
        /// </summary>
        internal Notifier<EventNotifier> EventAddedNotifier;

        /// <summary>
        /// Initializes a new instance of the EventProcessor class.
        /// </summary>
        internal EventNotifier() {
            EventAddedNotifier = new Notifier<EventNotifier>(this);
        }

        /// <summary>
        /// Call event handlers for the given event.
        /// </summary>
        /// <param name="eventInstance">The event instance to invoke the handlers for</param>
        private void CallEventHandlers(object eventInstance) {
            List<Action<object>> handlers;
            if (_handlers.TryGetValue(eventInstance.GetType(), out handlers)) {
                for (int i = 0; i < handlers.Count; ++i) {
                    handlers[i](eventInstance);
                }
            }
        }

        /// <summary>
        /// Dispatches all queued events to the registered handlers.
        /// </summary>
        /// <remarks>
        /// One of this methods contracts is that OnEventAdded will not be called while events are
        /// being dispatched.
        /// </remarks>
        internal void DispatchEvents() {
            // swap _events and _dispatchingEvents
            lock (this) {
                Utils.Swap(ref _events, ref _dispatchingEvents);
            }
            EventAddedNotifier.Reset();

            // dispatch all events in _dispatchingEvents
            for (int i = 0; i < _dispatchingEvents.Count; ++i) {
                CallEventHandlers(_dispatchingEvents[i]);
            }
            _dispatchingEvents.Clear();
        }

        /// <summary>
        /// Dispatch an event. Event listeners will be notified of the event at a later point in
        /// time.
        /// </summary>
        /// <param name="eventInstance">The event instance to dispatch</param>
        public void Submit(BaseEvent eventInstance) {
            lock (this) {
                _events.Add(eventInstance);
            }
            EventAddedNotifier.Notify();
        }

        /// <summary>
        /// Add a function that will be called a event of type TEvent has been dispatched to this
        /// dispatcher.
        /// </summary>
        /// <typeparam name="TEvent">The event type to listen for.</typeparam>
        /// <param name="onEvent">The code to invoke.</param>
        public void OnEvent<TEvent>(Action<TEvent> onEvent) where TEvent : BaseEvent {
            Type eventType = typeof(TEvent);

            lock (this) {
                // get our handlers for the given type
                List<Action<object>> handlers;
                if (_handlers.TryGetValue(eventType, out handlers) == false) {
                    handlers = new List<Action<object>>();
                    _handlers[eventType] = handlers;
                }

                // add the handler to the list of handlers
                Action<object> handler = obj => onEvent((TEvent)obj);
                handlers.Add(handler);
            }
        }

        /*
        /// <summary>
        /// Removes an event listener that was previously added with AddListener.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="onEvent"></param>
        /// <returns></returns>
        public bool RemoveListener<TEvent>(Action<TEvent> onEvent) {
            Type eventType = typeof(TEvent);

            lock (this) {
                // get our handlers for the given type
                List<Action<object>> handlers;
                if (_handlers.TryGetValue(eventType, out handlers)) {
                    // removing the handler succeeded
                    throw new NotImplementedException();
                    // return handlers.Remove(onEvent);
                }

                return false;
            }
        }
        */
    }

    /// <summary>
    /// Manages a collection of EventProcessors by allowing for convenient, thread-safe dispatch.
    /// </summary>
    internal class EventProcessorManager {
        /// <summary>
        /// The list of event processors which are need notifications. This can be written to by any
        /// number of threads, so locks are applied when writing.
        /// </summary>
        private List<EventNotifier> _dirtyEventProcessors = new List<EventNotifier>();

        /// <summary>
        /// The list of event processors that we are currently dispatching. This should be empty
        /// except when we are dispatching. It is read-only (it gets values from
        /// _dirtyEventProcessors) .
        /// </summary>
        private List<EventNotifier> _dispatchingEventProcessors = new List<EventNotifier>();

        private void EventAddedNotifier_Listener(EventNotifier eventProcessor) {
            lock (this) {
                _dirtyEventProcessors.Add(eventProcessor);
            }
        }

        /// <summary>
        /// Begin to monitor an event processor for dispatch notifications.
        /// </summary>
        public void BeginMonitoring(EventNotifier processor) {
            processor.EventAddedNotifier.Listener += EventAddedNotifier_Listener;
        }

        /// <summary>
        /// Stops monitoring an event processor.
        /// </summary>
        public void StopMonitoring(EventNotifier processor) {
            processor.EventAddedNotifier.Listener -= EventAddedNotifier_Listener;

            lock (this) {
                _dirtyEventProcessors.Remove(processor);
            }
        }

        /// <summary>
        /// Dispatches all event processors which have events.
        /// </summary>
        public void DispatchEvents() {
            lock (this) {
                Utils.Swap(ref _dirtyEventProcessors, ref _dispatchingEventProcessors);
            }

            for (int i = 0; i < _dispatchingEventProcessors.Count; ++i) {
                _dispatchingEventProcessors[i].DispatchEvents();
            }
            _dispatchingEventProcessors.Clear();
        }
    }
}