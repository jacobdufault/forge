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
    internal class TemplateConversionContext : IContextObject {
        public SparseArray<ITemplate> CreatedTemplates = new SparseArray<ITemplate>();
    }

    /// <summary>
    /// During the import/export process, all ITemplate instances are just exported as their
    /// TemplateIds. Then, a custom serialization converter is used to export the actual contents of
    /// the ITemplate instances. This results in a very clean and consistent file format that works
    /// with any type of object graph.
    /// </summary>
    internal class TemplateConverter : JsonConverter {
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
            TemplateConversionContext conversionContext = generalContext.Get<TemplateConversionContext>();
            GameEngineContext engineContext = generalContext.Get<GameEngineContext>();

            if (reader.TokenType != JsonToken.Integer) {
                throw new InvalidOperationException("Unexpected token type " + reader.TokenType);
            }

            // The token is referencing an integer, which means its just referencing a template.
            // We'll have to use our conversion context to get an instance of the template so that
            // it can be restored later.
            int templateId = (int)(long)reader.Value;
            return GetReference(conversionContext.CreatedTemplates, templateId, engineContext);
        }

        private ITemplate GetReference(SparseArray<ITemplate> templates, int templateId,
            GameEngineContext engineContext) {
            if (templates.Contains(templateId)) {
                return templates[templateId];
            }

            ITemplate template;
            if (engineContext.GameEngine.IsEmpty) {
                template = new ContentTemplate();
            }
            else {
                template = new RuntimeTemplate(engineContext.GameEngine.Value);
            }

            templates[templateId] = template;
            return template;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            ITemplate template = (ITemplate)value;
            writer.WriteValue(template.TemplateId);
        }
    }
}