using Forge.Entities.Implementation.ContextObjects;
using Forge.Entities.Implementation.Runtime;
using Forge.Entities.Implementation.Shared;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Forge.Entities.Implementation.Content {

    /// <summary>
    /// This type is used to deserialize a GameSnapshot instance. It just deserializes the
    /// GameSnapshot and TemplateGroup together in the same deserialization call so that the
    /// internal references inside of the TemplateGroup have the same ITemplate references as the
    /// internal ITemplate references in the GameSnapshot.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GameSnapshotRestorer {
        [JsonProperty("Snapshot")]
        public GameSnapshot GameSnapshot {
            get;
            private set;
        }

        [JsonProperty("Templates")]
        public TemplateGroup Templates {
            get;
            private set;
        }

        [OnDeserializing]
        private void AddTemplateContext(StreamingContext context) {
            var generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Create<TemplateConversionContext>();
        }

        [OnDeserialized]
        private void RemoveTemplateContext(StreamingContext context) {
            var generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Remove<TemplateConversionContext>();
        }

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
        public static GameSnapshotRestorer Restore(string snapshotJson, string templateJson,
            Maybe<GameEngine> gameEngine) {
            string json = CombineJson(snapshotJson, templateJson);

            return SerializationHelpers.Deserialize<GameSnapshotRestorer>(json,
                RequiredConverters.GetConverters(),
                RequiredConverters.GetContextObjects(gameEngine));
        }
    }
}