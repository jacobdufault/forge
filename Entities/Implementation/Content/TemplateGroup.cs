using Neon.Collections;
using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    internal class TemplateGroupConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            TemplateGroup group = (TemplateGroup)existingValue ?? new TemplateGroup();

            GeneralStreamingContext generalContext = (GeneralStreamingContext)serializer.Context.Context;
            generalContext.Create<TemplateConversionContext>();
            TemplateSerializationContainer container = serializer.Deserialize<TemplateSerializationContainer>(reader);
            generalContext.Remove<TemplateConversionContext>();

            group.Templates = container.Templates;
            return group;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            TemplateGroup group = (TemplateGroup)value;

            TemplateSerializationContainer container = new TemplateSerializationContainer() {
                Templates = group.Templates
            };
            GeneralStreamingContext generalContext = (GeneralStreamingContext)serializer.Context.Context;
            generalContext.Create<TemplateConversionContext>();
            serializer.Serialize(writer, container);
            generalContext.Remove<TemplateConversionContext>();
        }
    }

    [JsonConverter(typeof(TemplateGroupConverter))]
    internal class TemplateGroup : ITemplateGroup {
        public List<ITemplate> Templates = new List<ITemplate>();

        IEnumerable<ITemplate> ITemplateGroup.Templates {
            get { return Templates; }
        }

        ITemplate ITemplateGroup.CreateTemplate() {
            ITemplate template = new ContentTemplate();
            Templates.Add(template);
            return template;
        }
    }
}