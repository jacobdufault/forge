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
using System.Runtime.Serialization;

namespace Neon.Entities.Implementation.Content {

    internal class TemplateContainerConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Verifies that all of created templates in the template context have an associated
        /// definition in the list of template definitions.
        /// </summary>
        private void VerifyReferenceDefinitions(GameEngineContext engineContext, TemplateConversionContext templateContext, List<ContentTemplateSerializationFormat> templateDefinitions) {
            // We only verify template definitions if we're restoring an for an engine; it does not
            // matter if the templates are not fully instantiated if we're only in content editing
            // mode, as no game code will be executed.
            // TODO: do we really want to do this?
            if (engineContext.GameEngine.IsEmpty) {
                return;
            }

            // Get all of the ids for templates that we can restore
            HashSet<int> restoredTemplates = new HashSet<int>();
            foreach (var restoredTemplate in templateDefinitions) {
                restoredTemplates.Add(restoredTemplate.TemplateId);
            }

            // For every template that we have already created a reference for, verify that we have
            // an associated definition
            foreach (var pair in templateContext.CreatedTemplates) {
                ITemplate template = pair.Value;

                if (restoredTemplates.Contains(template.TemplateId) == false) {
                    throw new InvalidOperationException("Found template reference with id=" +
                        template.TemplateId + ", but the ITemplateGroup had no corresponding " +
                        "template definition");
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            TemplateSerializationContainer container =
                (TemplateSerializationContainer)existingValue ?? new TemplateSerializationContainer();

            List<ContentTemplateSerializationFormat> serializedTemplates = serializer.Deserialize<List<ContentTemplateSerializationFormat>>(reader);

            // We need to get our conversion context
            GeneralStreamingContext generalContext = (GeneralStreamingContext)serializer.Context.Context;
            TemplateConversionContext conversionContext = generalContext.Get<TemplateConversionContext>();
            GameEngineContext engineContext = generalContext.Get<GameEngineContext>();

            // TODO: if this method is really slow, then we can combine VerifyReferenceDefinitions
            //       and the
            // actual restoration process

            // Verify that we have restored all of our referenced templates
            VerifyReferenceDefinitions(engineContext, conversionContext, serializedTemplates);

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

    [JsonObject(MemberSerialization.OptIn)]
    internal class TemplateGroup : ITemplateGroup {
        public List<ITemplate> Templates = new List<ITemplate>();

        [JsonProperty("Templates")]
        private TemplateSerializationContainer _templateSerializationContainer;

        [JsonProperty("TemplateIdGenerator")]
        public UniqueIntGenerator TemplateIdGenerator = new UniqueIntGenerator();

        [OnSerializing]
        private void OnSerializing(StreamingContext context) {
            _templateSerializationContainer = new TemplateSerializationContainer() {
                Templates = Templates
            };
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            Templates = _templateSerializationContainer.Templates;
            _templateSerializationContainer = null;
        }

        IEnumerable<ITemplate> ITemplateGroup.Templates {
            get { return Templates; }
        }

        ITemplate ITemplateGroup.CreateTemplate() {
            ITemplate template = new ContentTemplate(TemplateIdGenerator.Next());
            Templates.Add(template);
            return template;
        }

        void ITemplateGroup.RemoveTemplate(ITemplate template) {
            for (int i = 0; i < Templates.Count; ++i) {
                if (template.TemplateId == Templates[i].TemplateId) {
                    Templates.RemoveAt(i);
                    return;
                }
            }

            throw new InvalidOperationException("Unable to find template with TemplateId=" +
                template.TemplateId);
        }
    }
}