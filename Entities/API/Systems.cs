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

using Newtonsoft.Json;
using System;

namespace Neon.Entities {
    /// <summary>
    /// All systems need to extend the system class. Systems have callbacks automatically registered
    /// by implementing ITrigger* interfaces.
    /// </summary>
    /// <remarks>
    /// Client code should not directly extend this, as it does not give any behavior by itself.
    /// </remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class System {
        /// <summary>
        /// Get the event dispatcher that can be used to notify the external world of events.
        /// </summary>
        protected internal IEventDispatcher EventDispatcher {
            get;
            internal set;
        }
    }

    /// <summary>
    /// Base filter for triggers which require a filter. as it does not provide any trigger APIs by
    /// itself.
    /// </summary>
    /// <remarks>
    /// Client code should not extend this.
    /// </remarks>
    public interface ITriggerBaseFilter {
        /// <summary>
        /// Computes the entity filter.
        /// </summary>
        /// <returns>A list of Data types that the entity needs to have to pass the
        /// filter.</returns>
        Type[] ComputeEntityFilter();
    }

    /// <summary>
    /// A trigger that is activated when an entity has passed the entity filter.
    /// </summary>
    public interface ITriggerAdded : ITriggerBaseFilter {
        /// <summary>
        /// Called when an Entity has passed the filter.
        /// </summary>
        /// <param name="entity">The entity that passed the filter.</param>
        void OnAdded(IEntity entity);
    }

    /// <summary>
    /// A trigger that is activated when an entity is no longer passing the entity filter.
    /// </summary>
    public interface ITriggerRemoved : ITriggerBaseFilter {
        /// <summary>
        /// Called when an Entity, which was once passing the filter, is no longer doing so.
        /// </summary>
        /// <remarks>
        /// This can occur for a number of reasons, such as a data state change or the Entity being
        /// destroyed.
        /// </remarks>
        /// <param name="entity">The entity that is no longer passing the filter.</param>
        void OnRemoved(IEntity entity);
    }

    /// <summary>
    /// A trigger that is notified whenever an Entity is modified.
    /// </summary>
    public interface ITriggerModified : ITriggerBaseFilter {
        /// <summary>
        /// The given entity, which has passed the filter, has been modified.
        /// </summary>
        void OnModified(IEntity entity);
    }

    /// <summary>
    /// A trigger that updates all entities which pass the filter per update loop.
    /// </summary>
    public interface ITriggerUpdate : ITriggerBaseFilter {
        /// <summary>
        /// This is called every update frame for all entities which pass the filter.
        /// </summary>
        /// <remarks>
        /// If you need to know when the entities are added or are no longer going to be updated,
        /// also implement ILifecycleTrigger.
        /// </remarks>
        void OnUpdate(IEntity entity);
    }

    /// <summary>
    /// A trigger that is called once per update loop before the update happens.
    /// </summary>
    public interface ITriggerGlobalPreUpdate {
        /// <summary>
        /// Called once per update loop. This is expected to use the EntityManager's singleton data.
        /// </summary>
        void OnGlobalPreUpdate(IEntity singletonEntity);
    }

    /// <summary>
    /// A trigger that is called once per update loop after the update happens.
    /// </summary>
    public interface ITriggerGlobalPostUpdate {
        /// <summary>
        /// Called once per update loop. This is expected to use the EntityManager's singleton data.
        /// </summary>
        void OnGlobalPostUpdate(IEntity singletonEntity);
    }

    /// <summary>
    /// A trigger that reacts to input for entities which pass the filter.
    /// </summary>
    public interface ITriggerInput : ITriggerBaseFilter {
        /// <summary>
        /// The type of structured input that the trigger is interested in.
        /// </summary>
        Type IStructuredInputType {
            get;
        }

        /// <summary>
        /// Called on all entities which pass the filter.
        /// </summary>
        void OnInput(IGameInput input, IEntity entity);
    }

    /// <summary>
    /// A trigger that globally reacts to input and is expected to use the EntityManager's singleton
    /// data.
    /// </summary>
    public interface ITriggerGlobalInput {
        /// <summary>
        /// The type of structured input that the trigger is interested in.
        /// </summary>
        Type IStructuredInputType {
            get;
        }

        /// <summary>
        /// Called whenever the given input type is received.
        /// </summary>
        void OnGlobalInput(IGameInput input, IEntity singletonEntity);
    }
}