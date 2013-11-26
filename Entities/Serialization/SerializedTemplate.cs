using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// Serialization specification for a data instance inside of an EntityTemplate.
    /// </summary>
    internal class SerializedTemplateData {
        /// <summary>
        /// The fully qualified type of the data instance.
        /// </summary>
        public string DataType;

        /// <summary>
        /// The serialization data for the state initial data state of the template.
        /// </summary>
        public SerializedData State;

        [NonSerialized]
        private IData _dataInstance;

        /// <summary>
        /// Deserializes the SerializedTemplateData into a Data instance. The result is cached.
        /// </summary>
        public IData GetDataInstance(SerializationConverter converter) {
            if (_dataInstance == null) {
                _dataInstance = (IData)converter.Import(TypeCache.FindType(DataType), State);
            }

            return _dataInstance;
        }
    }

    /// <summary>
    /// Serialization specification for an EntityTemplate.
    /// </summary>
    internal class SerializedTemplate {
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
        public List<SerializedTemplateData> Data;

        /// <summary>
        /// Default constructor for Neon.Serialization. This method assigns an empty list instance
        /// to Data, but otherwise does nothing.
        /// </summary>
        public SerializedTemplate() {
            Data = new List<SerializedTemplateData>();
        }

        /// <summary>
        /// Construct a SerializedTemplate from an EntityTemplate.
        /// </summary>
        public SerializedTemplate(EntityTemplate template, SerializationConverter converter) {
            PrettyName = template.PrettyName;
            TemplateId = template.TemplateId;

            Data = new List<SerializedTemplateData>();
            foreach (IData data in template.SelectData()) {
                SerializedTemplateData serializedData = new SerializedTemplateData() {
                    DataType = data.GetType().FullName,
                    State = converter.Export(data.GetType(), data)
                };
                Data.Add(serializedData);
            }
        }
    }
}