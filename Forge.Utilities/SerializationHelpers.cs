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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Runtime.Serialization;

namespace Forge.Utilities {
    /// <summary>
    /// Helper methods for Newtonsoft.JSON
    /// </summary>
    public static class SerializationHelpers {
        private class RequireOptInContractResolver : DefaultContractResolver {
            public RequireOptInContractResolver(JsonConverter[] converters) {
                _converters = converters;
            }

            private JsonConverter[] _converters;

            protected override JsonObjectContract CreateObjectContract(Type objectType) {
                bool hasConverter = false;
                foreach (JsonConverter converter in _converters) {
                    if (converter.CanConvert(objectType)) {
                        hasConverter = true;
                        break;
                    }
                }

                if (hasConverter == false &&
                    Attribute.IsDefined(objectType, typeof(JsonContainerAttribute)) == false &&
                    Attribute.IsDefined(objectType, typeof(JsonConverterAttribute)) == false) {
                    throw new InvalidOperationException("The type " + objectType.FullName + " needs a JsonObject attribute");
                }

                // Verify that the attribute, if it is a JsonObjectAttribute, has OptIn member
                // serialization
                {
                    JsonObjectAttribute attribute = (JsonObjectAttribute)
                        Attribute.GetCustomAttribute(objectType, typeof(JsonObjectAttribute));
                    if (attribute != null) {
                        if (attribute.MemberSerialization != MemberSerialization.OptIn) {
                            throw new InvalidOperationException("The type " + objectType.FullName + " has a JsonObject attribute, but it does not specify MemberSerialization.OptIn");
                        }
                    }
                }

                return base.CreateObjectContract(objectType);
            }
        }

        /// <summary>
        /// Helper method to create the JsonSerializerSettings that all of the serialization methods
        /// use.
        /// </summary>
        /// <param name="converters">The converters to use in the settings.</param>
        /// <param name="contextObjects">Context objects to use</param>
        /// <returns>An appropriate JsonSerializerSettings instance.</returns>
        private static JsonSerializerSettings CreateSettings(JsonConverter[] converters,
            IContextObject[] contextObjects) {
            converters = converters ?? new JsonConverter[0];
            contextObjects = contextObjects ?? new IContextObject[0];

            return new JsonSerializerSettings() {
                // handle inheritance correctly
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = converters,

                // we require that all types that go through the serialization process either a)
                // have a converter for them, or b) are annotated with
                // JsonObject(MemberSerialization.OptIn).
                ContractResolver = new RequireOptInContractResolver(converters),

                Context = new StreamingContext(StreamingContextStates.All, new GeneralStreamingContext(contextObjects)),

                // opt-in to reference handling
                //PreserveReferencesHandling = PreserveReferencesHandling.All
            };
        }

        /// <summary>
        /// Returns a deep clone of the given object instance.
        /// </summary>
        /// <typeparam name="T">The type of object to clone.</typeparam>
        /// <param name="instance">The original object to clone.</param>
        /// <param name="converters">Specific JSON converters to use when deserializing the
        /// object.</param>
        /// <param name="contextObjects">Initial context objects to use when deserializing</param>
        /// <returns>An identical clone to the given instance.</returns>
        public static T DeepClone<T>(T instance, JsonConverter[] converters,
            IContextObject[] contextObjects) {
            JsonSerializerSettings settings = CreateSettings(converters, contextObjects);

            string json = JsonConvert.SerializeObject(instance, typeof(T), Formatting.Indented, settings);
            return (T)JsonConvert.DeserializeObject(json, typeof(T), settings);
        }

        /// <summary>
        /// Returns a deep clone of the given object instance.
        /// </summary>
        /// <typeparam name="T">The type of object to clone.</typeparam>
        /// <param name="instance">The original object to clone.</param>
        /// <returns>An identical clone to the given instance.</returns>
        public static T DeepClone<T>(T instance) {
            return DeepClone(instance, EmptyConverterArray, EmptyContextObjectArray);
        }

        /// <summary>
        /// Returns the serialized version of the given instance, optionally using the given
        /// converters during the serialization process.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="instance">The object instance itself to serialize.</param>
        /// <param name="converters">The converters to use during the serialization process.</param>
        /// <param name="contextObjects">Context objects to use</param>
        /// <returns>A serialized version of the given object.</returns>
        public static string Serialize<T>(T instance, JsonConverter[] converters,
            IContextObject[] contextObjects) {
            JsonSerializerSettings settings = CreateSettings(converters, contextObjects);

            return JsonConvert.SerializeObject(instance, typeof(T), Formatting.Indented, settings);
        }

        /// <summary>
        /// Returns the serialized version of the given instance, optionally using the given
        /// converters during the serialization process.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="instance">The object instance itself to serialize.</param>
        /// <returns>A serialized version of the given object.</returns>
        public static string Serialize<T>(T instance) {
            return Serialize(instance, EmptyConverterArray, EmptyContextObjectArray);
        }

        /// <summary>
        /// Deserializes the given JSON data (hopefully created using Serialize for maximal
        /// compatibility) into an object instance of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The serialized state of the object.</param>
        /// <param name="converters">Converters to use during the deserialization process.</param>
        /// <param name="contextObjects">Context objects to use</param>
        /// <returns>A deserialized object of type T (or a derived type) that was generated from the
        /// given JSON data.</returns>
        public static T Deserialize<T>(string json, JsonConverter[] converters,
            IContextObject[] contextObjects) {
            JsonSerializerSettings settings = CreateSettings(converters, contextObjects);

            return (T)JsonConvert.DeserializeObject(json, typeof(T), settings);
        }

        /// <summary>
        /// Deserializes the given JSON data (hopefully created using Serialize for maximal
        /// compatibility) into an object instance of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The serialized state of the object.</param>
        /// <returns>A deserialized object of type T (or a derived type) that was generated from the
        /// given JSON data.</returns>
        public static T Deserialize<T>(string json) {
            return Deserialize<T>(json, EmptyConverterArray, EmptyContextObjectArray);
        }

        private static JsonConverter[] EmptyConverterArray = new JsonConverter[0];
        private static IContextObject[] EmptyContextObjectArray = new IContextObject[0];
    }
}