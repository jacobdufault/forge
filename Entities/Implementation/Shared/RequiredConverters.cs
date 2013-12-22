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
using Newtonsoft.Json.Converters;

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
               new StringEnumConverter() // we want to always convert enums by name
            };
        }

        /// <summary>
        /// Returns the context objects that are necessary for proper serialization of ISavedlevel.
        /// </summary>
        public static IContextObject[] GetContextObjects(Maybe<GameEngine> engine) {
            return new IContextObject[] {
                new GameEngineContext(engine)
            };
        }
    }
}