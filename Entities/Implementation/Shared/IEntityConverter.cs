using Neon.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Supports the serialization of IEntity references. Only the unique id of the entity is
    /// serialized.
    /// </summary>
    internal class IEntityConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(IEntity);
        }

        private class RefResolver : IReferenceResolver {
            public void AddReference(object context, string reference, object value) {
                throw new NotImplementedException();
            }

            public string GetReference(object context, object value) {
                throw new NotImplementedException();
            }

            public bool IsReferenced(object context, object value) {
                throw new NotImplementedException();
            }

            public object ResolveReference(object context, string reference) {
                throw new NotImplementedException();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            StreamingContext context;

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}