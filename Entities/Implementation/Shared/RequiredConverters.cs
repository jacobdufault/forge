using Neon.Entities.Implementation.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Helper class that just contains a list of all custom converters that should be used whenever
    /// Json.NET is used to serialize/deserialize values.
    /// </summary>
    internal static class RequiredConverters {
        public static JsonConverter[] GetConverters(GameEngine gameEngine) {
            return new JsonConverter[] {
               DataSparseArrayConverter.Instance,
               new TemplateConverter(gameEngine)
            };
        }
    }
}