using System.Collections.Generic;

namespace Neon.Entities.Implementation.Content.Serialization {
    /// <summary>
    /// Serialization specification for an EntityTemplate.
    /// </summary>
    internal class TemplateSpecification {
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