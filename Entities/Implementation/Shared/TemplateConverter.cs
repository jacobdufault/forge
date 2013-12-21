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

        /// <summary>
        /// Returns a template instance for the given TemplateId. If an instance for the given id
        /// already exists, then it is returned. Otherwise, either a RuntimeTemplate or
        /// ContentTemplate is created with an associated id based on the GameEngineContext.
        /// </summary>
        /// <param name="templateId">The id of the template to get an instance for.</param>
        /// <param name="context">The GameEngineContext, used to determine if we should create a
        /// ContentTemplate or RuntimeTemplate instance.</param>
        public ITemplate GetTemplateInstance(int templateId, GameEngineContext context) {
            if (CreatedTemplates.Contains(templateId)) {
                return CreatedTemplates[templateId];
            }

            ITemplate template;
            if (context.GameEngine.IsEmpty) {
                template = new ContentTemplate();
            }
            else {
                template = new RuntimeTemplate(context.GameEngine.Value);
            }

            CreatedTemplates[templateId] = template;
            return template;
        }
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
            return conversionContext.GetTemplateInstance(templateId, engineContext);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            ITemplate template = (ITemplate)value;
            writer.WriteValue(template.TemplateId);
        }
    }
}