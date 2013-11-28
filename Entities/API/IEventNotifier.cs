using System;

namespace Neon.Entities {
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
        void OnEvent<TEvent>(Action<TEvent> onEvent);

        /// <summary>
        /// Removes an event listener that was previously added with AddListener.
        /// </summary>
        //bool RemoveListener<TEvent>(Action<TEvent> onEvent);
    }
}