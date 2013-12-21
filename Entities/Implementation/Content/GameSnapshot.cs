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
                ITemplate template = GetTemplateInstance(format.TemplateId, templateInstances,
                    engineContext);

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

        private ITemplate GetTemplateInstance(int templateId, SparseArray<ITemplate> templates,
            GameEngineContext context) {
            if (templates.Contains(templateId)) {
                return templates[templateId];
            }

            ITemplate template;
            if (context.GameEngine.IsEmpty) {
                template = new ContentTemplate();
            }
            else {
                template = new RuntimeTemplate(context.GameEngine.Value);
            }

            templates[templateId] = template;
            return template;
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

    [JsonObject(MemberSerialization.OptIn)]
    internal class GameSnapshot : IGameSnapshot {
        public GameSnapshot() {
            _entityIdGenerator = new UniqueIntGenerator();
            _templateIdGenerator = new UniqueIntGenerator();

            SingletonEntity = new ContentEntity(_entityIdGenerator.Next(), "Global Singleton");
            ActiveEntities = new List<ContentEntity>();
            AddedEntities = new List<ContentEntity>();
            RemovedEntities = new List<ContentEntity>();
            Systems = new List<ISystem>();
            Templates = new List<ITemplate>();
        }

        [JsonProperty("EntityIdGenerator")]
        private UniqueIntGenerator _entityIdGenerator;
        [JsonProperty("TemplateIdGenerator")]
        private UniqueIntGenerator _templateIdGenerator;

        [JsonProperty("SingletonEntity")]
        public ContentEntity SingletonEntity {
            get;
            set;
        }

        [JsonProperty("ActiveEntities")]
        public List<ContentEntity> ActiveEntities {
            get;
            private set;
        }

        [JsonProperty("AddedEntities")]
        public List<ContentEntity> AddedEntities {
            get;
            private set;
        }

        [JsonProperty("RemovedEntities")]
        public List<ContentEntity> RemovedEntities {
            get;
            private set;
        }

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
        }

        [OnDeserializing]
        private void CreateContext(StreamingContext context) {
            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Create<DataReferenceContextObject>();
            generalContext.Create<TemplateConversionContext>();
        }

        /// <summary>
        /// This method is called after deserialization is finished. It goes through all
        /// DataReferences and restores them.
        /// </summary>
        [OnDeserialized]
        private void RestoreDataReferences(StreamingContext context) {
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

            Templates = _templateSerializationContainer.Templates;
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