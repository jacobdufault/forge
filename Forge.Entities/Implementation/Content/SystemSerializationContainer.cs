using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Forge.Entities.Implementation.Content {
    /// <summary>
    /// Stores a list of ISystems that are properly (de)serialized
    /// </summary>
    [JsonConverter(typeof(SystemSerializationContainer.Converter))]
    internal class SystemSerializationContainer {
        private class Converter : JsonConverter {
            public override bool CanConvert(Type objectType) {
                throw new NotSupportedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                var list = existingValue as SystemSerializationContainer ?? new SystemSerializationContainer();

                JArray array = serializer.Deserialize<JArray>(reader);
                foreach (JObject obj in array) {
                    Type systemType = obj["Type"].ToObject<Type>(serializer);
                    ISystem system = (ISystem)obj["System"].ToObject(systemType, serializer);
                    list.Systems.Add(system);
                }

                return list;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                var list = (SystemSerializationContainer)value;

                JArray array = new JArray();

                foreach (ISystem system in list.Systems) {
                    JObject obj = new JObject();
                    obj["Type"] = JToken.FromObject(system.GetType());
                    obj["System"] = JToken.FromObject(system, serializer);
                    array.Add(obj);
                }

                serializer.Serialize(writer, array);
            }
        }

        public List<ISystem> Systems = new List<ISystem>();
    }
}