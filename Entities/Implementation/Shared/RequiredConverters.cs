using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        /// <summary>
        /// Returns the converters that are necessary for proper serialization of an ISavedLevel.
        /// </summary>
        public static JsonConverter[] GetConverters() {
            return new JsonConverter[] {
               DataSparseArrayConverter.Instance,
               new StringEnumConverter() // we want to always convert enums by name
            };
        }

        /// <summary>
        /// Returns the context objects that are necessary for proper serialization of ISavedlevel.
        /// </summary>
        public static IContextObject[] GetContexts(Maybe<GameEngine> engine) {
            return new IContextObject[] {
                new GameEngineContext(engine)
            };
        }
    }
}