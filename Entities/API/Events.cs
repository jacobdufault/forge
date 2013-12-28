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
    /// Event that notifies listener that a new data instance has been added to the entity.
    /// </summary>
    public class AddedDataEvent : BaseEvent<AddedDataEvent> {
        private AddedDataEvent() {
        }

        /// <summary>
        /// The entity that had the data removed.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// The type of data that has been added.
        /// </summary>
        public Type AddedDataType;

        /// <summary>
        /// Returns an instance of the AddedDataEvent class.
        /// </summary>
        /// <param name="entity">The entity that the data was removed from.</param>
        /// <param name="addedDataType">Type of the added data.</param>
        internal static AddedDataEvent Create(IEntity entity, Type addedDataType) {
            var instance = GetInstance();
            instance.Entity = entity;
            instance.AddedDataType = addedDataType;
            return instance;
        }
    }

    /// <summary>
    /// Event that notifies listener that a new data instance has been added to the entity.
    /// </summary>
    public class RemovedDataEvent : BaseEvent<RemovedDataEvent> {
        private RemovedDataEvent() {
        }

        /// <summary>
        /// The entity that had the data removed.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// The type of data that has been added.
        /// </summary>
        public Type RemovedDataType;

        /// <summary>
        /// Initializes a new instance of the RemovedDataEvent class.
        /// </summary>
        /// <param name="entity">The entity that the data was removed from.</param>
        /// <param name="removedDataType">Type of the removed data.</param>
        internal static RemovedDataEvent Create(IEntity entity, Type removedDataType) {
            var instance = GetInstance();
            instance.Entity = entity;
            instance.RemovedDataType = removedDataType;
            return instance;
        }
    }

    /// <summary>
    /// Event that notifies the listener that a new Entity has been added to the EntityManager.
    /// </summary>
    public class EntityAddedEvent : BaseEvent<EntityAddedEvent> {
        private EntityAddedEvent() {
        }

        /// <summary>
        /// The entity that was added.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// Initializes a new instance of the EntityAddedEvent class.
        /// </summary>
        /// <param name="entity">The entity that was added.</param>
        internal static EntityAddedEvent Create(IEntity entity) {
            var instance = GetInstance();
            instance.Entity = entity;
            return instance;
        }
    }

    /// <summary>
    /// Event that notifies the listener that a new Entity has been removed from the EntityManager.
    /// </summary>
    public class EntityRemovedEvent : BaseEvent<EntityRemovedEvent> {
        private EntityRemovedEvent() {
        }

        /// <summary>
        /// The entity that was removed.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// Initializes a new instance of the EntityRemovedEvent class.
        /// </summary>
        /// <param name="entity">The entity that was removed.</param>
        internal static EntityRemovedEvent Create(IEntity entity) {
            var instance = GetInstance();
            instance.Entity = entity;
            return instance;
        }
    }

    /// <summary>
    /// Event notifying listeners that the entity should be hidden.
    /// </summary>
    public class HideEntityEvent : BaseEvent<HideEntityEvent> {
        private HideEntityEvent() {
        }

        public IEntity Entity;

        internal static HideEntityEvent Create(IEntity entity) {
            var instance = GetInstance();
            instance.Entity = entity;
            return instance;
        }
    }

    /// <summary>
    /// Event notifying listeners that the entity should be visible.
    /// </summary>
    public class ShowEntityEvent : BaseEvent<ShowEntityEvent> {
        private ShowEntityEvent() {
        }

        public IEntity Entity;

        internal static ShowEntityEvent Create(IEntity entity) {
            var instance = GetInstance();
            instance.Entity = entity;
            return instance;
        }
    }

    /// <summary>
    /// Event notifying listeners that the entity has been destroyed.
    /// </summary>
    public class DestroyedEntityEvent : BaseEvent<DestroyedEntityEvent> {
        private DestroyedEntityEvent() {
        }

        public IEntity Entity;

        internal static DestroyedEntityEvent Create(IEntity entity) {
            var instance = GetInstance();
            instance.Entity = entity;
            return instance;
        }
    }
}