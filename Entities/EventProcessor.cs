using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neon.Entities {
    /// <summary>
    /// An event is something that has happened in the entity system that some external system needs
    /// to be notified about.
    /// </summary>
    /// <remarks>
    /// Events are not designed to be simulation safe and do not make any guarantees about
    /// simulation state. Events should never modify the simulation; however, it is fine for them to
    /// read data from the simulation. The typical example of the event processor is for notifying
    /// external systems about interesting things that have occurred in the simulation.
    /// </remarks>
    public interface IEvent {
    }

    /// <summary>
    /// A event handler that has been registered.
    /// </summary>
    /// <remarks>
    /// This is used internally when removing an event handler to keep track of which handler was
    /// actually registered.
    /// </remarks>
    public struct RegisteredEventHandler {
        internal Type EventType;
        internal Action<IEvent> Handler;
    }

    /// <summary>
    /// Handles event dispatch. Events are queued up until some point in time and then they are
    /// dispatched.
    /// </summary>
    public class EventProcessor {
        /// <summary>
        /// Event handlers.
        /// </summary>
        private Dictionary<Type, List<Action<IEvent>>> _handlers = new Dictionary<Type, List<Action<IEvent>>>();

        /// <summary>
        /// The queued set of events that have occurred; any thread can write to this list.
        /// </summary>
        private List<IEvent> _events = new List<IEvent>();

        /// <summary>
        /// Events that are currently being dispatched. This is only read from (its values are
        /// retrieved from _events).
        /// </summary>
        private List<IEvent> _dispatchingEvents = new List<IEvent>();

        /// <summary>
        /// Called when an event has been dispatched to this event processor.
        /// </summary>
        internal Notifier<EventProcessor> EventAddedNotifier;

        /// <summary>
        /// Initializes a new instance of the EventProcessor class.
        /// </summary>
        internal EventProcessor() {
            EventAddedNotifier = new Notifier<EventProcessor>(this);
        }

        /// <summary>
        /// Call event handlers for the given event.
        /// </summary>
        /// <param name="eventInstance">The event instance to invoke the handlers for</param>
        private void CallEventHandlers(IEvent eventInstance) {
            List<Action<IEvent>> handlers;
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
        public void Submit(IEvent eventInstance) {
            lock (this) {
                _events.Add(eventInstance);
            }
            EventAddedNotifier.Notify();
        }

        /// <summary>
        /// Add an event handler that is called when the given event type has been triggered.
        /// </summary>
        /// <typeparam name="T">The type of event</typeparam>
        /// <param name="handler">The handler</param>
        public RegisteredEventHandler OnEvent<T>(Action<T> handler) where T : IEvent {
            return OnEvent(typeof(T), evnt => {
                handler((T)evnt);
            });
        }

        /// <summary>
        /// Add an event handler that is called when the given event type has been triggered.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="handler">The event handler.</param>
        public RegisteredEventHandler OnEvent(Type eventType, Action<IEvent> handler) {
            lock (this) {
                // get our handlers for the given type
                List<Action<IEvent>> handlers;
                if (_handlers.TryGetValue(eventType, out handlers) == false) {
                    handlers = new List<Action<IEvent>>();
                    _handlers[eventType] = handlers;
                }

                // add the handler to the list of handlers
                handlers.Add(handler);

                return new RegisteredEventHandler() {
                    EventType = eventType,
                    Handler = handler
                };
            }
        }

        /// <summary>
        /// Removes an event handler.
        /// </summary>
        /// <param name="eventHandler"></param>
        public void RemoveOnEvent(RegisteredEventHandler eventHandler) {
            lock (this) {
                // get our handlers for the given type
                List<Action<IEvent>> handlers;
                if (_handlers.TryGetValue(eventHandler.EventType, out handlers)) {
                    // removing the handler succeeded
                    if (handlers.Remove(eventHandler.Handler)) {
                        return;
                    }
                }

                throw new Exception("The event handler for " + eventHandler + " was not registered, or has been removed multiple times");
            }
        }
    }

    /// <summary>
    /// Manages a collection of EventProcessors by allowing for convenient, thread-safe dispatch.
    /// </summary>
    internal class EventProcessorManager {
        /// <summary>
        /// The list of event processors which are need notifications. This can be written to by any
        /// number of threads, so locks are applied when writing.
        /// </summary>
        private List<EventProcessor> _dirtyEventProcessors = new List<EventProcessor>();

        /// <summary>
        /// The list of event processors that we are currently dispatching. This should be empty
        /// except when we are dispatching. It is read-only (it gets values from
        /// _dirtyEventProcessors).
        /// </summary>
        private List<EventProcessor> _dispatchingEventProcessors = new List<EventProcessor>();

        private void EventAddedNotifier_Listener(EventProcessor eventProcessor) {
            lock (this) {
                _dirtyEventProcessors.Add(eventProcessor);
            }
        }

        /// <summary>
        /// Begin to monitor an event processor for dispatch notifications.
        /// </summary>
        public void BeginMonitoring(EventProcessor processor) {
            processor.EventAddedNotifier.Listener += EventAddedNotifier_Listener;
        }

        /// <summary>
        /// Stops monitoring an event processor.
        /// </summary>
        public void StopMonitoring(EventProcessor processor) {
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