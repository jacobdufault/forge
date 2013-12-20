using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Provides import and export support for ITemplates. ITemplates are always serialized using
    /// ContentTemplate, but the actual instance references are either ContentTemplates are
    /// RuntimeTemplates depending on the TemplateConverter's constructor arguments.
    /// </summary>
    internal class TemplateConverter : JsonConverter {
        /// <summary>
        /// If we're doing a runtime import, then we when construct RuntimeTemplates we need an
        /// associated GameEngine to construct them with.
        /// </summary>
        private GameEngine _gameEngine;

        /// <summary>
        /// Initializes a new instance of the TemplateConverter class.
        /// </summary>
        /// <param name="gameEngine">If null, then ITemplate instances are imported as
        /// ContentTemplate references. If a non-null argument is passed, then RuntimeTemplates are
        /// created which are linked to the specified game engine.</param>
        public TemplateConverter(GameEngine gameEngine) {
            _gameEngine = gameEngine;
        }

        public override bool CanConvert(Type objectType) {
            // we handle ITemplate and RuntimeTemplate as ContentTemplate, but we want to make sure
            // that ContentTemplate still gets handled by the default implementation, so we make
            // sure not to process ContentTemplates.
            return (objectType == typeof(ITemplate) || objectType == typeof(RuntimeTemplate)) &&
                objectType != typeof(ContentTemplate);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            // We always export ContentTemplate instances so we always import ContentTemplate
            // instances.

            ContentTemplate content = (ContentTemplate)existingValue ?? new ContentTemplate(-1);
            serializer.Populate(reader, content);

            if (_gameEngine != null) {
                return new RuntimeTemplate(content, _gameEngine);
            }
            return content;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            // We always export ContentTemplate instances (to help make a more stable save file
            // format and to simplify the serialization process, despite this file)
            ContentTemplate content = value as ContentTemplate;
            if (content == null) {
                content = new ContentTemplate((ITemplate)value);
            }

            serializer.Serialize(writer, content);
        }
    }
}