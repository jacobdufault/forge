using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
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

    /// <summary>
    /// During the import/export process, all IEntity instances are just exported as their
    /// UniqueIds. Then, a custom serialization converter is used to export the actual contents of
    /// the ITemplate instances. This results in a very clean and consistent file format that works
    /// with any type of object graph.
    /// </summary>
    internal class EntityConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // Make sure to handle null imports
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }

            // We need to get our template conversion context
            GeneralStreamingContext generalContext = (GeneralStreamingContext)serializer.Context.Context;
            EntityConversionContext conversionContext = generalContext.Get<EntityConversionContext>();
            GameEngineContext engineContext = generalContext.Get<GameEngineContext>();

            if (reader.TokenType != JsonToken.Integer) {
                throw new InvalidOperationException("Unexpected token type " + reader.TokenType);
            }

            // The token is referencing an integer, which means its just referencing an entity.
            // We'll have to use our conversion context to get an instance of the entity so that it
            // can be restored later.
            int entityId = (int)(long)reader.Value;
            return conversionContext.GetEntityInstance(entityId, engineContext);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            IEntity entity = (IEntity)value;
            writer.WriteValue(entity.UniqueId);
        }
    }
}