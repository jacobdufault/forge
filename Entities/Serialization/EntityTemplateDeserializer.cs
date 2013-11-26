
using Neon.Collections;
using Neon.Serialization;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Serialization {
    internal class EntityTemplateDeserializer : IEnumerable<EntityTemplate> {
        private SparseArray<EntityTemplate> _templates;
        private SerializationConverter _converter;

        public EntityTemplateDeserializer(List<SerializedTemplate> serializedTemplates, SerializationConverter converter) {
            _templates = new SparseArray<EntityTemplate>();
            _converter = converter;

            // create our initial list of template references
            foreach (var template in serializedTemplates) {
                _templates[template.TemplateId] = new EntityTemplate(template.TemplateId, template.PrettyName);
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
        public static void RestoreTemplate(EntityTemplate template,
            SerializedTemplate serializedTemplate,
            SerializationConverter converter) {
            foreach (var serializedData in serializedTemplate.Data) {
                IData data = serializedData.GetDataInstance(converter);
                template.AddDefaultData(data);
            }
        }

        public static void AddTemplateImporter(IEnumerable<EntityTemplate> templates, SerializationConverter converter) {
            var fastTemplateLookup = new SparseArray<EntityTemplate>();
            foreach (var template in templates) {
                fastTemplateLookup[template.TemplateId] = template;
            }

            converter.AddImporter(typeof(EntityTemplate), data => {
                if (data.IsReal == false) {
                    throw new InvalidOperationException("Unable to import EntityTemplate; expected TemplateId, found " + data);
                }

                int id = data.AsReal.AsInt;

                if (fastTemplateLookup.Contains(id) == false) {
                    throw new InvalidOperationException("Unable to import EntityTemplate; no template found with TemplateId=" + id);
                };

                return fastTemplateLookup[id];
            });
        }

        public void AddTemplateImporter(SerializationConverter converter) {
            converter.AddImporter(typeof(EntityTemplate), data => {
                if (data.IsReal == false) {
                    throw new InvalidOperationException("Unable to import EntityTemplate; expected TemplateId, found " + data);
                }

                int id = data.AsReal.AsInt;

                if (_templates.Contains(id) == false) {
                    throw new InvalidOperationException("Unable to import EntityTemplate; no template found with TemplateId=" + id);
                };

                return _templates[id];
            });
        }

        public static void AddTemplateExporter(SerializationConverter converter) {
            converter.AddExporter(typeof(EntityTemplate), instance => {
                return new SerializedData(((EntityTemplate)instance).TemplateId);
            });
        }

        public void RemoveTemplateConverter(SerializationConverter converter) {
            converter.RemoveImporter(typeof(EntityTemplate));
            converter.RemoveExporter(typeof(EntityTemplate));
        }

        public IEnumerator<EntityTemplate> GetEnumerator() {
            foreach (var tuple in _templates) {
                yield return tuple.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}