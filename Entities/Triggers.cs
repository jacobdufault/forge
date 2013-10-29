using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// All triggers should extend this interface.
    /// </summary>
    /// <remarks>
    /// Client code should not directly extend this, as it does not give any behavior by itself.
    /// </remarks>
    public interface ISystem {
    }

    /// <summary>
    /// Base filter for triggers which require a filter.
    /// as it does not provide any trigger APIs by itself.
    /// </summary>
    /// <remarks>
    /// Client code should not extend this.
    /// </remarks>
    public interface ITriggerBaseFilter : ISystem {
        /// <summary>
        /// Computes the entity filter.
        /// </summary
        /// <returns>A list of Data types that the entity needs to have to pass the filter.</returns>
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
        /// This can occur for a number of reasons, such as a data state change or the Entity
        /// being destroyed.
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
        /// If you need to know when the entities are added or are no longer going to be
        /// updated, also implement ILifecycleTrigger.
        /// </remarks>
        void OnUpdate(IEntity entity);
    }

    /// <summary>
    /// A trigger that is called once per update loop.
    /// </summary>
    public interface ITriggerGlobalPreUpdate : ISystem {
        /// <summary>
        /// Called once per update loop. This is expected to use the EntityManager's
        /// singleton data.
        /// </summary>
        void OnGlobalPreUpdate(IEntity singletonEntity);
    }

    /// <summary>
    /// A trigger that is called once per update loop.
    /// </summary>
    public interface ITriggerGlobalPostUpdate : ISystem {
        /// <summary>
        /// Called once per update loop. This is expected to use the EntityManager's
        /// singleton data.
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
        void OnInput(IStructuredInput input, IEntity entity);
    }

    /// <summary>
    /// A trigger that globally reacts to input and is expected to use
    /// the EntityManager's singleton data.
    /// </summary>
    public interface ITriggerGlobalInput : ISystem {
        /// <summary>
        /// The type of structured input that the trigger is interested in.
        /// </summary>
        Type IStructuredInputType {
            get;
        }

        /// <summary>
        /// Called whenever the given input type is received.
        /// </summary>
        void OnGlobalInput(IStructuredInput input, IEntity singletonEntity);
    }
}
