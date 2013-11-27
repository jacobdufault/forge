using Neon.Serialization;

namespace Neon.Entities.Implementation.Content.Serialization {
    /// <summary>
    /// Serialization specification for a data instance inside of an EntityTemplate.
    /// </summary>
    internal class TemplateDataSpecification {
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