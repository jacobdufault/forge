using Forge.Entities.Implementation.ContextObjects;
using Forge.Entities.Implementation.Runtime;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Forge.Entities.Implementation.Content {
    [JsonConverter(typeof(EntitySerializationContainer.Converter))]
    internal class EntitySerializationContainer {
        public List<IEntity> Entities;

        private class Converter : JsonConverter {
            public override bool CanConvert(Type objectType) {
                throw new InvalidOperationException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                EntitySerializationContainer container =
                    (EntitySerializationContainer)existingValue ?? new EntitySerializationContainer();

                var serializedEntities = serializer.Deserialize<List<ContentEntitySerializationFormat>>(reader);

                // We need to get our conversion context
                GeneralStreamingContext generalContext = (GeneralStreamingContext)serializer.Context.Context;
                EntityConversionContext conversionContext = generalContext.Get<EntityConversionContext>();
                GameEngineContext engineContext = generalContext.Get<GameEngineContext>();

                // There is only an EventDispatcherContext when we have a GameEngineContext
                IEventDispatcher eventDispatcher = null;
                if (engineContext.GameEngine.Exists) {
                    EventDispatcherContext eventContext = generalContext.Get<EventDispatcherContext>();
                    eventDispatcher = eventContext.Dispatcher;
                }

                // Restore our created entity instances
                List<IEntity> restored = new List<IEntity>();
                foreach (ContentEntitySerializationFormat format in serializedEntities) {
                    int entityId = format.UniqueId;
                    IEntity entity = conversionContext.GetEntityInstance(entityId, engineContext);
                    restored.Add(entity);

                    if (entity is ContentEntity) {
                        ((ContentEntity)entity).Initialize(format);
                    }
                    else {
                        ((RuntimeEntity)entity).Initialize(format, eventDispatcher);
                    }
                }

                container.Entities = restored;
                return container;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                EntitySerializationContainer container = (EntitySerializationContainer)value;

                var formats = new List<ContentEntitySerializationFormat>();
                foreach (IEntity entity in container.Entities) {
                    if (entity is ContentEntity) {
                        formats.Add(((ContentEntity)entity).GetSerializedFormat());
                    }
                    else {
                        formats.Add(((RuntimeEntity)entity).GetSerializedFormat());
                    }
                }

                serializer.Serialize(writer, formats);
            }

        }
    }
}