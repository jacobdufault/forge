
using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Content.Serialization;
using Neon.Serialization;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Serialization {
    internal class TemplateDeserializer : IEnumerable<ITemplate> {
        private SparseArray<Template> _templates;
        private SerializationConverter _converter;

        public TemplateDeserializer(List<TemplateSpecification> serializedTemplates, SerializationConverter converter) {
            _templates = new SparseArray<Template>();
            _converter = converter;

            // create our initial list of template references
            foreach (var template in serializedTemplates) {
                _templates[template.TemplateId] = new Template(template.TemplateId, template.PrettyName);
            }

            // setup the importer so that it returns the proper reference to the template (that may
            // not have been deserialized yet)
            AddTemplateImporter(_converter);

            // actually deserialize all of the templates
            foreach (var template in serializedTemplates) {
                RestoreTemplate(_templates[template.TemplateId], template, _converter);
            }

            // remove the importer
            RemoveTemplateConverter(_converter);
        }

        /// <summary>
        /// Populates the given template with data from the serializedTemplate.
        /// </summary>
        /// <param name="template">The template instance to populate.</param>
        /// <param name="serializedTemplate">The serialized template instance to get data
        /// from</param>
        /// <param name="converter">The serialization converter to use when deserializing data
        /// instances</param>
        private static void RestoreTemplate(Template template,
            TemplateSpecification serializedTemplate,
            SerializationConverter converter) {

            foreach (var serializedData in serializedTemplate.Data) {
                Type dataType = TypeCache.FindType(serializedData.DataType);
                IData data = (IData)converter.Import(dataType, serializedData.State);

                template.AddDefaultData(data);
            }
        }

        public void AddTemplateImporter(SerializationConverter converter) {
            converter.AddImporter(typeof(ITemplate), data => {
                if (data.IsReal == false) {
                    throw new InvalidOperationException("Unable to import ITemplate; expected " +
                        "TemplateId, found " + data);
                }

                int id = data.AsReal.AsInt;

                if (_templates.Contains(id) == false) {
                    throw new InvalidOperationException("Unable to import ITemplate; no template " +
                        "found with TemplateId=" + id);
                };

                return _templates[id];
            });
        }

        public static void AddTemplateExporter(SerializationConverter converter) {
            converter.AddExporter(typeof(ITemplate), instance => {
                return new SerializedData(((ITemplate)instance).TemplateId);
            });
        }

        public void RemoveTemplateConverter(SerializationConverter converter) {
            converter.RemoveImporter(typeof(ITemplate));
            converter.RemoveExporter(typeof(ITemplate));
        }

        public IEnumerator<ITemplate> GetEnumerator() {
            foreach (var tuple in _templates) {
                yield return tuple.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}