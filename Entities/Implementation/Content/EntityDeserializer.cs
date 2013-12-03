using Neon.Collections;
using Neon.Entities.Implementation.Content.Specifications;
using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Content {
    /// <summary>
    /// Deserializes a list of entities. This runs the deserialization process in multiple steps, so
    /// that entities can reference any other entity.
    /// </summary>
    internal class EntityDeserializer : IEnumerable<IEntity> {
        /// <summary>
        /// The converter that is used for the deserialization process.
        /// </summary>
        private SerializationConverter _converter;

        /// <summary>
        /// The list of deserialized entities.
        /// </summary>
        private SparseArray<Tuple<ContentEntity, EntitySpecification>> _entities;

        public IEntity AddEntity(SerializedData serializedData) {
            EntitySpecification entitySpec = new EntitySpecification(serializedData);

            ContentEntity entity = new ContentEntity(entitySpec.UniqueId, entitySpec.PrettyName);
            _entities[entity.UniqueId] = Tuple.Create(entity, entitySpec);
            return entity;
        }

        public List<IEntity> AddEntities(List<SerializedData> entitySpecifications) {
            List<IEntity> result = new List<IEntity>();

            foreach (var serializedData in entitySpecifications) {
                EntitySpecification entitySpec = new EntitySpecification(serializedData);

                ContentEntity entity = new ContentEntity(entitySpec.UniqueId, entitySpec.PrettyName);
                _entities[entity.UniqueId] = Tuple.Create(entity, entitySpec);
                result.Add(entity);
            }

            return result;
        }

        /// <summary>
        /// Deserializes the entities using the given converter.
        /// </summary>
        /// <param name="converter">The converter to use; this method requires temporary importer
        /// support for the IEntity type</param>
        public EntityDeserializer(SerializationConverter converter) {
            _converter = converter;
            _entities = new SparseArray<Tuple<ContentEntity, EntitySpecification>>();
        }

        public void Run() {
            // setup the entity importer so that it returns our created (but not necessarily
            // deserialized yet) references
            _converter.AddImporter(typeof(IEntity), data => {
                return _entities[data.AsReal.AsInt];
            });

            // actually deserialize the entities
            foreach (var tuple in _entities) {
                RestoreEntity(tuple.Value.Item1, tuple.Value.Item2);
            }

            // clean up our importer from the converter
            _converter.RemoveImporter(typeof(IEntity));
        }

        public static void AddEntityExporter(SerializationConverter converter) {
            converter.AddExporter<IEntity>(entity => new SerializedData(entity.UniqueId));
        }

        private void RestoreEntity(ContentEntity entity, EntitySpecification savedState) {
            entity.Restore(savedState, _converter);
        }

        public IEnumerator<IEntity> GetEnumerator() {
            foreach (var tuple in _entities) {
                yield return tuple.Value.Item1;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}