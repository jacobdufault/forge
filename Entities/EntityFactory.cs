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

        /// <summary>
        /// Used when an IEntity instance is requested that is a clone of the given Prefab.
        /// </summary>
        public static Func<IEntityPrefab, IEntity> GeneratorPrefab;

        static EntityFactory() {
            Generator = () => new Entity();
            GeneratorPrefab = prefab => new Entity();
        }

        public static IEntity Create() {
            return Generator();
        }

        public static IEntity Create(IEntityPrefab prefab) {
            return GeneratorPrefab(prefab);
        }
    }
}
