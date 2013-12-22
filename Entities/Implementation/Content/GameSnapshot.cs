using Neon.Collections;
using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Neon.Entities.Implementation.Content {
    internal class EntityContainerConverter : JsonConverter {
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
                    ((RuntimeEntity)entity).Initialize(format);
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

    [JsonConverter(typeof(EntityContainerConverter))]
    internal class EntitySerializationContainer {
        public List<IEntity> Entities;
    }

    /// <summary>
    /// This type is used to deserialize a GameSnapshot instance. It just deserializes the
    /// GameSnapshot and TemplateGroup together in the same deserialization call so that the
    /// internal references inside of the TemplateGroup have the same ITemplate references as the
    /// internal ITemplate references in the GameSnapshot.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GameSnapshotRestorer {
        [JsonProperty("Snapshot")]
        private GameSnapshot _gameSnapshot;

        [JsonProperty("Templates")]
        private TemplateGroup _templates;

        /// <summary>
        /// Combines snapshot and template JSON together into the serialized format that the
        /// GameSnapshotRestorer can read.
        /// </summary>
        private static string CombineJson(string snapshot, string template) {
            string s = "{ \"Snapshot\": " + snapshot + ", \"Templates\": " + template + " }";
            return s;
        }

        /// <summary>
        /// Restores a GameSnapshot using the given GameSnapshot JSON and the given TemplateGroup
        /// JSON.
        /// </summary>
        public static GameSnapshot Restore(string snapshotJson, string templateJson,
            Maybe<GameEngine> gameEngine) {
            string json = CombineJson(snapshotJson, templateJson);

            var restorer = SerializationHelpers.Deserialize<GameSnapshotRestorer>(json,
                RequiredConverters.GetConverters(), RequiredConverters.GetContexts(gameEngine));
            return restorer._gameSnapshot;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class GameSnapshot : IGameSnapshot {
        public GameSnapshot() {
            _entityIdGenerator = new UniqueIntGenerator();
            _templateIdGenerator = new UniqueIntGenerator();

            SingletonEntity = new ContentEntity(_entityIdGenerator.Next(), "Global Singleton");
            ActiveEntities = new List<IEntity>();
            AddedEntities = new List<IEntity>();
            RemovedEntities = new List<IEntity>();
            Systems = new List<ISystem>();
        }

        [JsonProperty("EntityIdGenerator")]
        private UniqueIntGenerator _entityIdGenerator;
        [JsonProperty("TemplateIdGenerator")]
        private UniqueIntGenerator _templateIdGenerator;

        public IEntity SingletonEntity {
            get;
            set;
        }

        public List<IEntity> ActiveEntities {
            get;
            private set;
        }

        public List<IEntity> AddedEntities {
            get;
            private set;
        }

        public List<IEntity> RemovedEntities {
            get;
            private set;
        }

        [JsonProperty("SingletonEntity")]
        private EntitySerializationContainer _singletonEntityContainer;
        [JsonProperty("AddedEntities")]
        private EntitySerializationContainer _addedEntitiesContainer;
        [JsonProperty("ActiveEntities")]
        private EntitySerializationContainer _activeEntitiesContainer;
        [JsonProperty("RemovedEntities")]
        private EntitySerializationContainer _removedEntitiesContainer;

        [JsonProperty("Systems")]
        public List<ISystem> Systems {
            get;
            set;
        }

        [OnSerializing]
        private void CreateConverter(StreamingContext context) {
            _singletonEntityContainer = new EntitySerializationContainer() {
                Entities = new List<IEntity>() { SingletonEntity }
            };
            _addedEntitiesContainer = new EntitySerializationContainer() {
                Entities = AddedEntities
            };
            _activeEntitiesContainer = new EntitySerializationContainer() {
                Entities = ActiveEntities
            };
            _removedEntitiesContainer = new EntitySerializationContainer() {
                Entities = RemovedEntities
            };
        }

        [OnDeserializing]
        private void CreateContext(StreamingContext context) {
            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Create<EntityConversionContext>();
        }

        /// <summary>
        /// This method is called after deserialization is finished. It goes through all
        /// DataReferences and restores them.
        /// </summary>
        [OnDeserialized]
        private void RestoreDataReferences(StreamingContext context) {
            SingletonEntity = _singletonEntityContainer.Entities[0];
            AddedEntities = _addedEntitiesContainer.Entities;
            ActiveEntities = _activeEntitiesContainer.Entities;
            RemovedEntities = _removedEntitiesContainer.Entities;

            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Remove<EntityConversionContext>();
        }

        public IEntity CreateEntity(EntityAddLocation to, string prettyName = "") {
            ContentEntity added = new ContentEntity(_entityIdGenerator.Next(), prettyName);

            switch (to) {
                case EntityAddLocation.Active:
                    ActiveEntities.Add(added);
                    break;
                case EntityAddLocation.Added:
                    AddedEntities.Add(added);
                    break;
                case EntityAddLocation.Removed:
                    RemovedEntities.Add(added);
                    break;
            }

            return added;
        }

        IEntity IGameSnapshot.SingletonEntity {
            get { return SingletonEntity; }
        }

        IEnumerable<IEntity> IGameSnapshot.ActiveEntities {
            get { return ActiveEntities.Cast<IEntity>(); }
        }

        IEnumerable<IEntity> IGameSnapshot.RemovedEntities {
            get { return RemovedEntities.Cast<IEntity>(); }
        }

        IEnumerable<IEntity> IGameSnapshot.AddedEntities {
            get { return AddedEntities.Cast<IEntity>(); }
        }

        List<ISystem> IGameSnapshot.Systems {
            get { return Systems; }
        }
    }
}