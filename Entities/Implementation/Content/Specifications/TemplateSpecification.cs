using Neon.Serialization;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Content.Serialization {
    /// <summary>
    /// Serialization specification for an EntityTemplate.
    /// </summary>
    internal class TemplateSpecification {
        public TemplateSpecification(SerializedData data) {
            PrettyName = data.AsDictionary["PrettyName"].AsString;
            TemplateId = data.AsDictionary["TemplateId"].AsReal.AsInt;

            Data = new List<TemplateDataSpecification>();
            foreach (var dataSpec in data.AsDictionary["Data"].AsList) {
                TemplateDataSpecification spec = new TemplateDataSpecification(dataSpec);
                Data.Add(spec);
            }
        }

        public TemplateSpecification(Template template, SerializationConverter converter) {
            PrettyName = template.PrettyName;
            TemplateId = template.TemplateId;

            Data = new List<TemplateDataSpecification>();
            foreach (var data in template.SelectCurrentData()) {
                TemplateDataSpecification spec = new TemplateDataSpecification(data, converter);
                Data.Add(spec);
            }
        }

        public SerializedData Export() {
            Dictionary<string, SerializedData> dict = new Dictionary<string, SerializedData>();

            dict["PrettyName"] = new SerializedData(PrettyName);
            dict["TemplateId"] = new SerializedData(TemplateId);

            List<SerializedData> data = new List<SerializedData>();
            foreach (var dataSpec in Data) {
                data.Add(dataSpec.Export());
            }
            dict["Data"] = new SerializedData(data);

            return new SerializedData(dict);
        }

        /// <summary>
        /// The pretty name of the template (for debugging).
        /// </summary>
        public string PrettyName;

        /// <summary>
        /// The template's unique id.
        /// </summary>
        public int TemplateId;

        /// <summary>
        /// All data instances inside of the template.
        /// </summary>
        public List<TemplateDataSpecification> Data;
    }
}