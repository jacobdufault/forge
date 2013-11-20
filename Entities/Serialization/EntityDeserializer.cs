using Neon.Collections;
using Neon.Serialization;
using System.Collections.Generic;

namespace Neon.Entities.Serialization {

    // TODO: replace HasModification with entity.HasModification, and HasStateChange with
    //       entity.NeedsStateChangeUpdate, then remove DeserializedEntity
    public class DeserializedEntity {
        public Entity Entity;
        public bool HasModification;
        public bool HasStateChange;
        public bool IsAdding;
        public bool IsRemoving;
    }

    /// <summary>
    /// Deserializes a list of entities. This runs the deserialization process in multiple steps, so
    /// that entities can reference any other entity.
    /// </summary>
    public class EntityDeserializer : IEnumerable<DeserializedEntity> {
        /// <summary>
        /// The converter that is used for the deserialization process.
        /// </summary>
        private SerializationConverter _converter;

        /// <summary>
        /// Are the entities going to be added to the entity manager?
        /// </summary>
        private bool _addingToEntityManager;

        /// <summary>
        /// The list of deserialized entities.
        /// </summary>
        private IterableSparseArray<DeserializedEntity> _entities;

        /// <summary>
        /// Deserializes the entities using the given converter.
        /// </summary>
        /// <param name="singleton">The singleton entity</param>
        /// <param name="entities">The list of all other entities</param>
        /// <param name="converter">The converter to use; this method requires temporary importer
        /// support for the IEntity type</param>
        /// <param name="addingToEntityManager">Will the entities be added to an
        /// EntityManager?</param>
        public EntityDeserializer(SerializedEntity singleton, List<SerializedEntity> entities,
            SerializationConverter converter, bool addingToEntityManager) {
            _converter = converter;
            _addingToEntityManager = addingToEntityManager;
            _entities = new IterableSparseArray<DeserializedEntity>();

            // prepare the entity references so that we can have circular references to other
            // entities
            _entities[singleton.UniqueId] = new DeserializedEntity() {
                Entity = new Entity(singleton.UniqueId, singleton.PrettyName)
            };
            foreach (var entity in entities) {
                _entities[entity.UniqueId] = new DeserializedEntity() {
                    Entity = new Entity(entity.UniqueId, entity.PrettyName)
                };
            }

            // setup the entity importer so that it returns our created (but not necessarily
            // deserialized yet) references
            converter.AddImporter(typeof(IEntity), data => {
                return _entities[data.AsReal.AsInt].Entity;
            });

            // actually deserialize the entities
            RestoreEntity(singleton, _entities[singleton.UniqueId], _converter, _addingToEntityManager);
            foreach (var entity in entities) {
                RestoreEntity(entity, _entities[entity.UniqueId], _converter, _addingToEntityManager);
            }

            // clean up our importer from the converter
            converter.RemoveImporter(typeof(IEntity));
        }

        public static void RestoreEntity(SerializedEntity entity, DeserializedEntity storage,
            SerializationConverter converter, bool addingToEntityManager) {
            storage.Entity.Restore(entity, converter, out storage.HasModification,
                out storage.HasStateChange, addingToEntityManager);
            storage.IsAdding = entity.IsAdding;
            storage.IsRemoving = entity.IsRemoving;
        }

        public IEnumerator<DeserializedEntity> GetEnumerator() {
            foreach (var tuple in _entities) {
                yield return tuple.Item2;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}