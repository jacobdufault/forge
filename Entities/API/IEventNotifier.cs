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

namespace Neon.Entities {
    /// <summary>
    /// Base event type that all events must derive from.
    /// </summary>
    public abstract class BaseEvent {
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
        void OnEvent<TEvent>(Action<TEvent> onEvent) where TEvent : BaseEvent;

        /// <summary>
        /// Removes an event listener that was previously added with AddListener.
        /// </summary>
        //bool RemoveListener<TEvent>(Action<TEvent> onEvent);
    }
}