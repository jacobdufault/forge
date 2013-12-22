using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.ContextObjects {
    internal class EntityConversionContext : IContextObject {
        public SparseArray<IEntity> CreatedEntities = new SparseArray<IEntity>();

        /// <summary>
        /// Returns an entity instance for the given entity UniqueId. If an instance for the given
        /// id already exists, then it is returned. Otherwise, either a RuntimeEntity or
        /// ContentEntity is created.
        /// </summary>
        /// <param name="entityId">The id of the entity to get an instance for.</param>
        /// <param name="context">The GameEngineContext, used to determine if we should create a
        /// ContentTemplate or RuntimeTemplate instance.</param>
        public IEntity GetEntityInstance(int entityId, GameEngineContext context) {
            if (CreatedEntities.Contains(entityId)) {
                return CreatedEntities[entityId];
            }

            IEntity entity;
            if (context.GameEngine.IsEmpty) {
                entity = new ContentEntity();
            }
            else {
                entity = new RuntimeEntity();
            }

            CreatedEntities[entityId] = entity;
            return entity;
        }
    }
}