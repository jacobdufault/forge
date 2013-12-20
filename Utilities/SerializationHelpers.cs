using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Neon.Utilities {
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
        /// <returns>An appropriate JsonSerializerSettings instance.</returns>
        private static JsonSerializerSettings CreateSettings(JsonConverter[] converters) {
            return new JsonSerializerSettings() {
                // handle inheritance correctly
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = converters,

                ContractResolver = new RequireOptInContractResolver(converters)

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
        /// <returns>An identical clone to the given instance.</returns>
        public static T DeepClone<T>(T instance, params JsonConverter[] converters) {
            JsonSerializerSettings settings = CreateSettings(converters);

            string json = JsonConvert.SerializeObject(instance, typeof(T), Formatting.Indented, settings);
            return (T)JsonConvert.DeserializeObject(json, typeof(T), settings);
        }

        /// <summary>
        /// Returns the serialized version of the given instance, optionally using the given
        /// converters during the serialization process.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="instance">The object instance itself to serialize.</param>
        /// <param name="converters">The converters to use during the serialization process.</param>
        /// <returns>A serialized version of the given object.</returns>
        public static string Serialize<T>(T instance, params JsonConverter[] converters) {
            JsonSerializerSettings settings = CreateSettings(converters);

            return JsonConvert.SerializeObject(instance, typeof(T), Formatting.Indented, settings);
        }

        /// <summary>
        /// Deserializes the given JSON data (hopefully created using Serialize for maximal
        /// compatibility) into an object instance of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The serialized state of the object.</param>
        /// <param name="converters">Converters to use during the deserialization process.</param>
        /// <returns>A deserialized object of type T (or a derived type) that was generated from the
        /// given JSON data.</returns>
        public static T Deserialize<T>(string json, params JsonConverter[] converters) {
            JsonSerializerSettings settings = CreateSettings(converters);

            return (T)JsonConvert.DeserializeObject(json, typeof(T), settings);
        }
    }
}