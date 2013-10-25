using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Neon.Utility;
using Neon.Collections;

namespace Neon.Entities {
    /// <summary>
    /// A set of operations that are used for managing entities.
    /// </summary>
    public interface IEntityManager {
        /// <summary>
        /// Code to call when we do our next update.
        /// </summary>
        //event Action OnNextUpdate;

        /// <summary>
        /// Our current update number. Useful for debugging purposes.
        /// </summary>
        int UpdateNumber {
            get;
        }

        /// <summary>
        /// Registers the given system with the EntityManager.
        /// </summary>
        void AddSystem(ISystem system);

        /// <summary>
        /// Updates the world. State changes (entity add, entity remove, ...) are propogated to the different
        /// registered listeners. Update listeners will be called and the given commands will be executed.
        /// </summary>
        void UpdateWorld(IEnumerable<IStructuredInput> commands);

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// Destroys the given entity.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        void RemoveEntity(IEntity entity);

        /// <summary>
        /// Singleton entity that contains global data
        /// </summary>
        IEntity SingletonEntity {
            get;
        }
    }

    /// <summary>
    /// The EntityManager requires an associated Entity which is not injected into the
    /// EntityManager.
    /// </summary>
    public class EntityManager : IEntityManager {
        /// <summary>
        /// Event processors which need their events dispatched.
        /// </summary>
        private List<EventProcessor> _dirtyEventProcessors = new List<EventProcessor>();

        /// <summary>
        /// The list of active Entities in the world.
        /// </summary>
        private UnorderedList<IEntity> _entities = new UnorderedList<IEntity>();

        /// <summary>
        /// A list of Entities that need to be added to the world.
        /// </summary>
        private List<Entity> _entitiesToAdd = new List<Entity>();

        /// <summary>
        /// A list of Entities that need to be removed from the world.
        /// </summary>
        private List<Entity> _entitiesToRemove = new List<Entity>();

        /// <summary>
        /// A double buffered list of Entities that have been modified.
        /// </summary>
        private BufferedItem<List<Entity>> _entitiesWithModifications = new BufferedItem<List<Entity>>();

        /// <summary>
        /// Entities which have had their states changed.
        /// </summary>
        private LinkedList<Entity> _entitiesWithStateChanges = new LinkedList<Entity>();

        private List<System> _allSystems = new List<System>();
        private List<System> _systemsWithUpdateTriggers = new List<System>();
        private List<System> _systemsWithInputTriggers = new List<System>();

        class ModifiedTrigger {
            public ITriggerModified Trigger;
            public Filter Filter;

            public ModifiedTrigger(ITriggerModified trigger) {
                Trigger = trigger;
                Filter = new Filter(trigger.ComputeEntityFilter());
            }
        }
        private List<ModifiedTrigger> _modifiedTriggers = new List<ModifiedTrigger>();

        private List<ITriggerGlobalPreUpdate> _globalPreUpdateTriggers = new List<ITriggerGlobalPreUpdate>();
        private List<ITriggerGlobalPostUpdate> _globalPostUpdateTriggers = new List<ITriggerGlobalPostUpdate>();
        private List<ITriggerGlobalInput> _globalInputTriggers = new List<ITriggerGlobalInput>();

        /// <summary>
        /// The key we use to access unordered list metadata from the entity.
        /// </summary>
        private static MetadataKey _entityUnorderedListMetadataKey = Entity.MetadataRegistry.GetKey();

        /// <summary>
        /// The key we use to access our modified listeners for the entity
        /// </summary>
        private static MetadataKey _entityModifiedListenersKey = Entity.MetadataRegistry.GetKey();

        public IEntity SingletonEntity {
            get;
            set;
        }

        public EntityManager(IEntity singletonEntity) {
            SingletonEntity = singletonEntity;
        }

        /// <summary>
        /// Registers the given system with the EntityManager.
        /// </summary>
        public void AddSystem(ISystem baseSystem) {
            Contract.Requires(_entities.Length == 0, "Cannot add a trigger after entities have been added");
            Log<EntityManager>.Info("({0}) Adding system {1}", UpdateNumber, baseSystem);

            if (baseSystem is ITriggerBaseFilter) {
                System system = new System((ITriggerBaseFilter)baseSystem);
                _allSystems.Add(system);

                if (baseSystem is ITriggerLifecycle) {
                    var lifecycle = (ITriggerLifecycle)baseSystem;
                    system.OnAddedToCache += entity => {
                        lifecycle.OnAdded(entity);
                    };
                    system.OnRemovedFromCache += entity => {
                        lifecycle.OnRemoved(entity);
                    };
                }
                if (baseSystem is ITriggerUpdate) {
                    _systemsWithUpdateTriggers.Add(system);
                }
                if (baseSystem is ITriggerModified) {
                    var modified = (ITriggerModified)baseSystem;
                    _modifiedTriggers.Add(new ModifiedTrigger(modified));
                }
                if (baseSystem is ITriggerInput) {
                    _systemsWithInputTriggers.Add(system);
                }
            }


            if (baseSystem is ITriggerGlobalPreUpdate) {
                _globalPreUpdateTriggers.Add((ITriggerGlobalPreUpdate)baseSystem);
            }
            if (baseSystem is ITriggerGlobalPostUpdate) {
                _globalPostUpdateTriggers.Add((ITriggerGlobalPostUpdate)baseSystem);
            }
            if (baseSystem is ITriggerGlobalInput) {
                _globalInputTriggers.Add((ITriggerGlobalInput)baseSystem);
            }
        }

        public int UpdateNumber {
            get;
            private set;
        }

        /// <summary>
        /// Updates the world. State changes (entity add, entity remove, ...) are propagated to the different
        /// registered listeners. Update listeners will be called and the given commands will be executed.
        /// </summary>
        public void UpdateWorld(IEnumerable<IStructuredInput> commands) {
            ++UpdateNumber;

            // we add/remove entities here because an entity could be in the modified list (which is used in the CallModifiedMethods)

            InvokeAddEntities();
            InvokeRemoveEntities();

            InvokeEntityDataStateChanges();
            InvokeModifications();

            InvokeOnCommandMethods(commands);
            InvokeUpdateMethods();

            // update the singleton data
            SingletonEntity.ApplyModifications();

            // update dirty event processors
            InvokeEventProcessors();

            // we destroy entities so that disappear quickly (and not waiting for the start of the next update)
            //DoDestroyEntities();
            //DoRemoveEntities();
        }

        private void InvokeEventProcessors() {
            Log<EntityManager>.Info("({0}) Invoking event processors; numInvoking={1}", UpdateNumber, _dirtyEventProcessors.Count);
            for (int i = 0; i < _dirtyEventProcessors.Count; ++i) {
                _dirtyEventProcessors[i].DispatchEvents();
            }
            _dirtyEventProcessors.Clear();
        }

        /// <summary>
        /// Dispatches all Entity modifications and calls the relevant methods.
        /// </summary>
        private void InvokeModifications() {
            // Alert the modified listeners that the entity has been modified
            List<Entity> modifiedEntities = _entitiesWithModifications.Swap();
            foreach (var modified in modifiedEntities) {
                Log<EntityManager>.Info("({0}) Applying modifications to {1}", UpdateNumber, modified);
                // apply the modifications to the entity; ie, dispatch changes
                modified.ApplyModifications();

                // call the InvokeOnModified functions - *user code*
                // we store which methods are relevant to the entity in the entities metadata for performance reasons
                List<ModifiedTrigger> triggers = (List<ModifiedTrigger>)modified.Metadata[_entityModifiedListenersKey];
                for (int i = 0; i < triggers.Count; ++i) {
                    Log<EntityManager>.Info("({0}) Modification check to see if {1} is interested in {2}", UpdateNumber, triggers[i].Trigger, modified);
                    if (triggers[i].Filter.ModificationCheck(modified)) {
                        Log<EntityManager>.Info("({0}) Invoking {1} on {2}", UpdateNumber, triggers[i].Trigger, modified);
                        triggers[i].Trigger.OnModified(modified);
                    }
                }
            }
            modifiedEntities.Clear();
        }

        /// <summary>
        /// Dispatches the set of commands to all [InvokeOnCommand] methods.
        /// </summary>
        private void InvokeOnCommandMethods(IEnumerable<IStructuredInput> inputSequence) {
            // Call the OnCommand methods - *user code*
            foreach (var input in inputSequence) {
                for (int i = 0; i < _globalInputTriggers.Count; ++i) {
                    var trigger = _globalInputTriggers[i];
                    Log<EntityManager>.Info("({0}) Checking {1} for input {2}", UpdateNumber, trigger, input);
                    if (trigger.IStructuredInputType.IsInstanceOfType(input)) {
                        Log<EntityManager>.Info("({0}) Invoking {1} on input {2}", UpdateNumber, trigger, input);
                        trigger.OnGlobalInput(input, SingletonEntity);
                    }
                }

                for (int i = 0; i < _systemsWithInputTriggers.Count; ++i) {
                    System system = _systemsWithInputTriggers[i];
                    ITriggerInput trigger = (ITriggerInput)system.Trigger;

                    for (int j = 0; j < system.CachedEntities.Length; ++j) {
                        IEntity entity = system.CachedEntities[j];
                        if (entity.Enabled) {
                            trigger.OnInput(input, entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calls all [InvokeBeforeUpdate], [InvokeOnUpdate], and [InvokeAfterUpdate] methods.
        /// </summary>
        private void InvokeUpdateMethods() {
            // Call the BeforeUpdate methods - *user code*
            for (int i = 0; i < _globalPreUpdateTriggers.Count; ++i) {
                _globalPreUpdateTriggers[i].OnGlobalPreUpdate(SingletonEntity);
            }

            // Call the OnUpdate methods - *user code*
            for (int i = 0; i < _systemsWithUpdateTriggers.Count; ++i) {
                System system = _systemsWithUpdateTriggers[i];
                ITriggerUpdate trigger = (ITriggerUpdate)system.Trigger;

                for (int j = 0; j < system.CachedEntities.Length; ++j) {
                    IEntity entity = system.CachedEntities[j];
                    if (entity.Enabled) {
                        trigger.OnUpdate(entity);
                    }
                }
            }

            // Call the AfterUpdate methods - *user code*
            for (int i = 0; i < _globalPostUpdateTriggers.Count; ++i) {
                _globalPostUpdateTriggers[i].OnGlobalPostUpdate(SingletonEntity);
            }
        }

        private void InvokeAddEntities() {
            // Add entities
            for (int i = 0; i < _entitiesToAdd.Count; ++i) {
                Entity toAdd = _entitiesToAdd[i];
                Log<EntityManager>.Info("({0}) Adding {1}", UpdateNumber, toAdd);

                toAdd.Show();

                toAdd.AddedToEntityManager(this);

                // register listeners
                toAdd.OnModified += OnEntityModified;
                toAdd.OnDataStateChanged += OnEntityDataStateChanged;
                toAdd.EventProcessor.OnEventAdded += EventProcessor_OnEventAdded;

                // apply initialization changes
                toAdd.ApplyModifications();

                // ensure it contains metadata for our keys
                toAdd.Metadata[_entityUnorderedListMetadataKey] = new UnorderedListMetadata();
                toAdd.Metadata[_entityModifiedListenersKey] = new List<ModifiedTrigger>();

                // add it our list of active entities
                _entities.Add(toAdd, (UnorderedListMetadata)toAdd.Metadata[_entityUnorderedListMetadataKey]);

                // update optimization caches -- *USER CODE*
                //InvokeDataStateChanges(toAdd);
                if (_entitiesWithStateChanges.Contains(toAdd) == false) {
                    _entitiesWithStateChanges.AddLast(toAdd);
                }
            }

            _entitiesToAdd.Clear();
        }

        private void InvokeEntityDataStateChanges() {
            // Note that this loop is carefully constructed
            // It has to handle a couple of (difficult) things;
            // first, it needs to support the item that is being
            // iterated being removed, and secondly, it needs to
            // support more items being added to it as it iterates
            LinkedListNode<Entity> it = _entitiesWithStateChanges.Last;
            while (it != null) {
                var curr = it;
                it = it.Previous;
                var entity = curr.Value;

                Log<EntityManager>.Info("({0}) Doing data state changes on {1}", UpdateNumber, entity);
                // update data state changes and if there are no more updates needed,
                // then remove it from the dispatch list
                if (entity.DataStateChangeUpdate() == false) {
                    Log<EntityManager>.Info("({0}) No more state changes requested for {1}", UpdateNumber, entity);
                    _entitiesWithStateChanges.Remove(curr);
                }

                // update the entity's internal trigger cache
                List<ModifiedTrigger> triggers = (List<ModifiedTrigger>)entity.Metadata[_entityModifiedListenersKey];
                triggers.Clear();
                for (int i = 0; i < _modifiedTriggers.Count; ++i) {
                    Log<EntityManager>.Info("({0}) Checking to see if modification trigger {1} is interested in {2}", UpdateNumber, _modifiedTriggers[i].Trigger, entity);
                    if (_modifiedTriggers[i].Filter.Check(entity)) {
                        Log<EntityManager>.Info("({0}) It was; adding modified trigger {1} to entity {2}'s modification cache", UpdateNumber, _modifiedTriggers[i].Trigger, entity);
                        triggers.Add(_modifiedTriggers[i]);
                    }
                }

                // update the caches on the entity and call user code - *user code*
                for (int i = 0; i < _allSystems.Count; ++i) {
                    var change = _allSystems[i].UpdateCache(entity);
                    Log<EntityManager>.Info("({0}) Updated cache in {1} for {2}; change was {3}", UpdateNumber, _allSystems[i].Trigger, entity, change);
                }
            }
        }

        private void InvokeRemoveEntities() {
            // Destroy entities
            for (int i = 0; i < _entitiesToRemove.Count; ++i) {
                Entity toDestroy = _entitiesToRemove[i];
                toDestroy.RemoveAllData();

                Log<EntityManager>.Info("({0}) Removing {1}", UpdateNumber, toDestroy);

                // remove listeners
                toDestroy.OnModified -= OnEntityModified;
                toDestroy.OnDataStateChanged -= OnEntityDataStateChanged;

                // remove the entity from caches
                for (int j = 0; j < _allSystems.Count; ++j) {
                    _allSystems[j].Remove(toDestroy);
                }
                List<ModifiedTrigger> triggers = (List<ModifiedTrigger>)toDestroy.Metadata[_entityModifiedListenersKey];
                triggers.Clear();

                // remove the entity from the list of entities
                _entities.Remove(toDestroy, (UnorderedListMetadata)toDestroy.Metadata[_entityUnorderedListMetadataKey]);
                toDestroy.RemovedFromEntityManager();
            }
            _entitiesToRemove.Clear();
        }

        /// <summary>
        /// Registers the given entity with the world.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        public void AddEntity(IEntity instance) {
            Log<EntityManager>.Info("({0}) AddEntity({1}) called", UpdateNumber, instance);
            Entity entity = (Entity)instance;
            _entitiesToAdd.Add(entity);
            _entitiesWithStateChanges.AddLast(entity);
            entity.Hide();
        }

        /// <summary>
        /// Removes the given entity from the world.
        /// </summary>
        /// <param name="instance">The entity instance to remove</param>
        public void RemoveEntity(IEntity instance) {
            Log<EntityManager>.Info("({0}) RemoveEntity({1}) called", UpdateNumber, instance);
            Entity entity = (Entity)instance;
            if (_entitiesToRemove.Contains(entity) == false) {
                _entitiesToRemove.Add(entity);
                entity.Hide();
            }
        }

        /// <summary>
        /// Called when an Entity has been modified.
        /// </summary>
        private void OnEntityModified(Entity sender) {
            Log<EntityManager>.Info("({0}) Got modification notification for {1}... checking for duplicates", UpdateNumber, sender);
            if (_entitiesWithModifications.Get().Contains(sender) == false) {
                Log<EntityManager>.Info("({0}) Adding {1} to modification list", UpdateNumber, sender);
                _entitiesWithModifications.Get().Add(sender);
            }
        }

        /// <summary>
        /// Called when an entity has data state changes
        /// </summary>
        private void OnEntityDataStateChanged(Entity sender) {
            Log<EntityManager>.Info("({0}) Got data state change for {1}... checking for duplicates", UpdateNumber, sender);
            if (_entitiesWithStateChanges.Contains(sender) == false) {
                Log<EntityManager>.Info("({0}) Adding {1} to date state change list", UpdateNumber, sender);
                _entitiesWithStateChanges.AddLast(sender);
            }
        }

        /// <summary>
        /// Called when an event processor has had an event added to it.
        /// </summary>
        private void EventProcessor_OnEventAdded(EventProcessor eventProcessor) {
            _dirtyEventProcessors.Add(eventProcessor);
        }
    }
}