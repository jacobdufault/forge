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
    /// Wraps a common event pattern where there is only one instance of the event.
    /// </summary>
    /// <typeparam name="DerivedEventType">The type of the class that derives this type.</typeparam>
    public class SingletonEvent<DerivedEventType> : BaseEvent
        where DerivedEventType : SingletonEvent<DerivedEventType>, new() {

        /// <summary>
        /// Internal constructor to prevent more instances of the singleton event type from being
        /// created.
        /// </summary>
        protected SingletonEvent() {
        }

        /// <summary>
        /// The event's instance.
        /// </summary>
        public static DerivedEventType Instance = new DerivedEventType();
    }

    /// <summary>
    /// Event that notifies listener that a new data instance has been added to the entity.
    /// </summary>
    public class AddedDataEvent : BaseEvent {
        /// <summary>
        /// The type of data that has been added.
        /// </summary>
        public Type AddedDataType;

        /// <summary>
        /// Initializes a new instance of the AddedDataEvent class.
        /// </summary>
        /// <param name="addedDataType">Type of the added data.</param>
        internal AddedDataEvent(Type addedDataType) {
            AddedDataType = addedDataType;
        }
    }

    /// <summary>
    /// Event that notifies listener that a new data instance has been added to the entity.
    /// </summary>
    public class RemovedDataEvent : BaseEvent {
        /// <summary>
        /// The type of data that has been added.
        /// </summary>
        public Type RemovedDataType;

        /// <summary>
        /// Initializes a new instance of the RemovedDataEvent class.
        /// </summary>
        /// <param name="removedDataType">Type of the removed data.</param>
        internal RemovedDataEvent(Type removedDataType) {
            RemovedDataType = removedDataType;
        }
    }

    /// <summary>
    /// Event that notifies the listener that a new Entity has been added to the EntityManager.
    /// </summary>
    public class EntityAddedEvent : BaseEvent {
        /// <summary>
        /// The entity that was added.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// Initializes a new instance of the EntityAddedEvent class.
        /// </summary>
        /// <param name="entity">The entity that was added.</param>
        internal EntityAddedEvent(IEntity entity) {
            Entity = entity;
        }
    }

    /// <summary>
    /// Event that notifies the listener that a new Entity has been removed from the EntityManager.
    /// </summary>
    public class EntityRemovedEvent : BaseEvent {
        /// <summary>
        /// The entity that was removed.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// Initializes a new instance of the EntityRemovedEvent class.
        /// </summary>
        /// <param name="entity">The entity that was removed.</param>
        internal EntityRemovedEvent(IEntity entity) {
            Entity = entity;
        }
    }

    /// <summary>
    /// Event notifying listeners that the entity should be hidden.
    /// </summary>
    public class HideEntityEvent : SingletonEvent<HideEntityEvent> { }

    /// <summary>
    /// Event notifying listeners that the entity should be visible.
    /// </summary>
    public class ShowEntityEvent : SingletonEvent<ShowEntityEvent> { }

    /// <summary>
    /// Event notifying listeners that the entity has been destroyed.
    /// </summary>
    public class DestroyedEntityEvent : SingletonEvent<DestroyedEntityEvent> { }
}