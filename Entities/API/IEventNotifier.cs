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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Neon.Entities {
    internal interface IEvent {
        /// <summary>
        /// Reuse this event instance at a later point in time.
        /// </summary>
        void Reuse();
    }

    /// <summary>
    /// Base event type that all events must derive from.
    /// </summary>
    /// <remarks>
    /// BaseEvent provides a number of helpful static methods (available only to the derived type)
    /// that make creating a factory for BaseEvent extremely simple. However, it is imperative that
    /// the factory methods be used, otherwise a memory leak will occur. The factory methods are
    /// thread-safe.
    /// </remarks>
    public abstract class BaseEvent<TDerived> : IEvent
        where TDerived : BaseEvent<TDerived> {

        /// <summary>
        /// Events that have been constructed but are not in use.
        /// </summary>
        private static ConcurrentStack<TDerived> _availableEvents = new ConcurrentStack<TDerived>();

        void IEvent.Reuse() {
            _availableEvents.Push((TDerived)this);
        }

        /// <summary>
        /// Helper method to get an instance of the event. The instance may be populated with data,
        /// so make sure to fully initialize it. If there is no instance that can be reused, a new
        /// one is allocated using the default constructor.
        /// </summary>
        protected static TDerived GetInstance() {
            TDerived instance;
            if (_availableEvents.TryPop(out instance)) {
                return instance;
            }

            return (TDerived)Activator.CreateInstance(typeof(TDerived), nonPublic: true);
        }
    }

    /// <summary>
    /// An IEventNotifier instance allows for objects to listen to other objects for interesting
    /// events based on the given IEvent type. The event dispatcher is a generalization of C#'s
    /// support for event, plus additional support for delayed event dispatch.
    /// </summary>
    public interface IEventNotifier {
        /// <summary>
        /// Add a function that will be called a event of type TEvent has been dispatched to this
        /// dispatcher.
        /// </summary>
        /// <typeparam name="TEvent">The event type to listen for.</typeparam>
        /// <param name="onEvent">The code to invoke.</param>
        void OnEvent<TEvent>(Action<TEvent> onEvent) where TEvent : BaseEvent<TEvent>;

        /// <summary>
        /// Removes an event listener that was previously added with AddListener.
        /// </summary>
        //bool RemoveListener<TEvent>(Action<TEvent> onEvent);
    }
}