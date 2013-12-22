// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Neon.Entities.Implementation.ContextObjects;
using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
            TemplateSerializationContainer container = serializer.Deserialize<TemplateSerializationContainer>(reader);
            group.Templates = container.Templates;
            return group;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            TemplateGroup group = (TemplateGroup)value;

            TemplateSerializationContainer container = new TemplateSerializationContainer() {
                Templates = group.Templates
            };
            serializer.Serialize(writer, container);
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