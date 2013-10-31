using System;

namespace Neon.Entities {
    /// <summary>
    /// Creates instances of IEntitys.
    /// </summary>
    public static class EntityFactory {
        /// <summary>
        /// Used when an IEntity instance is requested without any initial data.
        /// </summary>
        public static Func<IEntity> Generator;

        static EntityFactory() {
            Generator = () => new Entity();
        }

        /// <summary>
        /// Creates a new data instance by using the given entity generator.
        /// </summary>
        /// <returns>A new entity instance.</returns>
        public static IEntity Create() {
            return Generator();
        }

        /// <summary>
        /// Creates a new entity from the given template.
        /// </summary>
        /// <param name="template">The template to create an entity from.</param>
        /// <returns>An entity instance</returns>
        public static IEntity Create(EntityTemplate template) {
            // create a blank entity
            IEntity entity = Create();

            // copy all of the data from the template into the entity
            foreach (var dataInstance in template) {
                Data added = entity.AddData(new DataAccessor(dataInstance.GetType()));
                added.CopyFrom(dataInstance);
            }

            return entity;
        }
    }
}