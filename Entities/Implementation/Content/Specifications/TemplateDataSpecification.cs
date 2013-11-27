using Neon.Serialization;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Content.Specifications {
    /// <summary>
    /// Serialization specification for a data instance inside of an EntityTemplate.
    /// </summary>
    internal class TemplateDataSpecification {
        public TemplateDataSpecification(SerializedData data) {
            DataType = data.AsDictionary["DataType"].AsString;
            State = data.AsDictionary["State"];
        }

        public TemplateDataSpecification(IData data, SerializationConverter converter) {
            DataType = data.GetType().FullName;
            State = converter.Export(data.GetType(), data);
        }

        public SerializedData Export() {
            Dictionary<string, SerializedData> dict = new Dictionary<string, SerializedData>();

            dict["DataType"] = new SerializedData(DataType);
            dict["State"] = State;

            return new SerializedData(dict);
        }

        /// <summary>
        /// The fully qualified type of the data instance.
        /// </summary>
        public string DataType;

        /// <summary>
        /// The serialization data for the state initial data state of the template.
        /// </summary>
        public SerializedData State;
    }
}