using Neon.Entities.Implementation.ContextObjects;
using Neon.Utilities;
using Newtonsoft.Json;
using System;

namespace Neon.Entities.Implementation.Shared {

    /// <summary>
    /// Proxy type that specifies the serialization format that IQueryableEntity derived type use
    /// when converting to and from JSON.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class QueryableEntitySerializationProxy {
        /// <summary>
        /// The id of the entity or template that we reference. We serialize this and it will be
        /// resolved later.
        /// </summary>
        [JsonProperty("ReferencedId")]
        public int ReferencedId;

        /// <summary>
        /// Specifies the types of entities that this DataReference can reference.
        /// </summary>
        public enum ReferenceTypes {
            TemplateReference,
            EntityReference
        }

        /// <summary>
        /// The type of entity that we are referencing (either an IEntity or an ITemplate).
        /// </summary>
        [JsonProperty("ReferenceType")]
        public ReferenceTypes ReferenceType;
    }

    /// <summary>
    /// Converts IQueryableEntities to and from JSON. This uses IQueryableEntityProxy as a
    /// serialization format. Deserializing IQueryableEntities requires that the be a
    /// GameEngineContext, a EntityConversionContext, and a TemplateConersionContext within the
    /// GeneralContext container. This converter supports a large number of different types
    /// (IQueryableEntity, IEntity, ITemplate, ContentEntity, ContentTemplate, RuntimeEntity, and
    /// RuntimeTemplate) .
    /// </summary>
    internal class QueryableEntityConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            // Read in our serialized proxy format
            var proxy = serializer.Deserialize<QueryableEntitySerializationProxy>(reader);

            // The proxy was null; the serialized entity was null, so return null
            if (proxy == null) {
                return null;
            }

            // Get some context objects that will be used for resolving both IEntity and ITemplates
            var generalContext = (GeneralStreamingContext)serializer.Context.Context;
            var engineContext = generalContext.Get<GameEngineContext>();

            // Get our id
            int id = proxy.ReferencedId;

            // We are referencing an IEntity
            if (proxy.ReferenceType == QueryableEntitySerializationProxy.ReferenceTypes.EntityReference) {
                var entityContext = generalContext.Get<EntityConversionContext>();
                return entityContext.GetEntityInstance(id, engineContext);
            }

            // We are referencing an ITemplate
            else {
                var templateContext = generalContext.Get<TemplateConversionContext>();
                return templateContext.GetTemplateInstance(id, engineContext);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            QueryableEntitySerializationProxy proxy;

            if (value is IEntity) {
                proxy = new QueryableEntitySerializationProxy() {
                    ReferencedId = ((IEntity)value).UniqueId,
                    ReferenceType = QueryableEntitySerializationProxy.ReferenceTypes.EntityReference
                };
            }
            else {
                proxy = new QueryableEntitySerializationProxy() {
                    ReferencedId = ((ITemplate)value).TemplateId,
                    ReferenceType = QueryableEntitySerializationProxy.ReferenceTypes.TemplateReference
                };
            }

            serializer.Serialize(writer, proxy);
        }
    }
}