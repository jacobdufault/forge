using Neon.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Converters instances of SparseArray[IData] to JSON. We don't use the default converter,
    /// because by default SparseArray converts using the dictionary serializer. However, the keys
    /// in the sparse array are specific to the run of the program, so we just regenerate them when
    /// we deserialize.
    /// </summary>
    internal class DataSparseArrayConverter : JsonConverter {
        public static DataSparseArrayConverter Instance = new DataSparseArrayConverter();

        private DataSparseArrayConverter() {
        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(SparseArray<IData>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            List<IData> items = serializer.Deserialize<List<IData>>(reader);

            SparseArray<IData> result = existingValue as SparseArray<IData> ?? new SparseArray<IData>();
            for (int i = 0; i < items.Count; ++i) {
                IData data = items[i];
                result[DataAccessorFactory.GetId(data)] = data;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            SparseArray<IData> array = (SparseArray<IData>)value;
            IEnumerable<IData> enumerator = from pair in array
                                            select pair.Value;
            serializer.Serialize(writer, enumerator);
        }
    }
}