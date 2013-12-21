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

    internal class TemplateContainerConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            TemplateSerializationContainer container =
                (TemplateSerializationContainer)existingValue ?? new TemplateSerializationContainer();

            List<ContentTemplateSerializationFormat> serializedTemplates = serializer.Deserialize<List<ContentTemplateSerializationFormat>>(reader);

            // We need to get our conversion context
            GeneralStreamingContext generalContext = (GeneralStreamingContext)serializer.Context.Context;
            TemplateConversionContext conversionContext = generalContext.Get<TemplateConversionContext>();
            GameEngineContext engineContext = generalContext.Get<GameEngineContext>();

            SparseArray<ITemplate> templateInstances = conversionContext.CreatedTemplates;

            // Restore our created template instances
            foreach (ContentTemplateSerializationFormat format in serializedTemplates) {
                int templateId = format.TemplateId;
                ITemplate template = conversionContext.GetTemplateInstance(templateId, engineContext);

                if (template is ContentTemplate) {
                    ((ContentTemplate)template).Initialize(format);
                }
                else {
                    ((RuntimeTemplate)template).Initialize(format);
                }
            }

            container.Templates = conversionContext.CreatedTemplates.Select(p => p.Value).ToList();
            return container;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            TemplateSerializationContainer container = (TemplateSerializationContainer)value;

            List<ContentTemplateSerializationFormat> formats = new List<ContentTemplateSerializationFormat>();
            foreach (ITemplate template in container.Templates) {
                ContentTemplate content = template as ContentTemplate ?? new ContentTemplate(template);
                formats.Add(content.GetSerializedFormat());
            }

            serializer.Serialize(writer, formats);
        }
    }

    [JsonConverter(typeof(TemplateContainerConverter))]
    internal class TemplateSerializationContainer {
        public List<ITemplate> Templates;
    }

    [JsonConverter(typeof(EntityContainerConverter))]
    internal class EntitySerializationContainer {
        public List<IEntity> Entities;
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
            Templates = new List<ITemplate>();
        }

        [JsonProperty("EntityIdGenerator")]
        private UniqueIntGenerator _entityIdGenerator;
        [JsonProperty("TemplateIdGenerator")]
        private UniqueIntGenerator _templateIdGenerator;

        [JsonProperty("SingletonEntity")]
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

        public List<ITemplate> Templates {
            get;
            set;
        }

        [JsonProperty("Templates")]
        private TemplateSerializationContainer _templateSerializationContainer;

        [OnSerializing]
        private void CreateConverter(StreamingContext context) {
            _templateSerializationContainer = new TemplateSerializationContainer() {
                Templates = Templates
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
            generalContext.Create<DataReferenceContextObject>();
            generalContext.Create<TemplateConversionContext>();
            generalContext.Create<EntityConversionContext>();
        }

        /// <summary>
        /// This method is called after deserialization is finished. It goes through all
        /// DataReferences and restores them.
        /// </summary>
        [OnDeserialized]
        private void RestoreDataReferences(StreamingContext context) {
            AddedEntities = _addedEntitiesContainer.Entities;
            ActiveEntities = _activeEntitiesContainer.Entities;
            RemovedEntities = _removedEntitiesContainer.Entities;
            Templates = _templateSerializationContainer.Templates;

            // TODO: we can probably just directly use IEntity or ITemplate (or add support for
            // IQueryableEntity) instead of going through a manual serialization process
            SparseArray<IEntity> entities = new SparseArray<IEntity>();
            entities[SingletonEntity.UniqueId] = SingletonEntity;
            foreach (var added in AddedEntities) {
                entities[added.UniqueId] = added;
            }
            foreach (var removed in RemovedEntities) {
                entities[removed.UniqueId] = removed;
            }
            foreach (var active in ActiveEntities) {
                entities[active.UniqueId] = active;
            }

            SparseArray<ITemplate> templates = new SparseArray<ITemplate>();
            foreach (var template in Templates) {
                templates[template.TemplateId] = template;
            }

            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            List<BaseDataReferenceType> references = generalContext.Get<DataReferenceContextObject>().DataReferences;
            for (int i = 0; i < references.Count; ++i) {
                BaseDataReferenceType reference = references[i];

                int targetId = reference.ReferencedId;
                if (reference.IsEntityReference) {
                    reference.ResolveEntityId(entities[targetId]);
                }
                else {
                    reference.ResolveEntityId(templates[targetId]);
                }
            }

            generalContext.Remove<DataReferenceContextObject>();
            generalContext.Remove<TemplateConversionContext>();
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

        public ITemplate CreateTemplate() {
            ContentTemplate template = new ContentTemplate(_templateIdGenerator.Next());
            Templates.Add(template);
            return template;
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

        IEnumerable<ITemplate> IGameSnapshot.Templates {
            get { return Templates.Cast<ITemplate>(); }
        }
    }
}