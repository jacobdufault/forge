using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Serialization {
    public class TemplateDataJson {
        /// <summary>
        /// The fully qualified type of the data instance.
        /// </summary>
        public string DataType;
        
        /// <summary>
        /// The JSON data for the state initial data state of the template.
        /// </summary>
        public JsonData State;


        [NonSerialized]
        private Data _dataInstance;

        /// <summary>
        /// Deserializes the TemplateDataJson into a Data instance.
        /// </summary>
        public Data GetDataInstance() {
            if (_dataInstance == null) {
                _dataInstance = (Data)JsonMapper.ReadValue(TypeCache.FindType(DataType), State);
            }

            return _dataInstance;
        }
    }

    public class TemplateJson {
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
        public List<TemplateDataJson> Data;

        // TODO: find a way to make TemplateJson not rely on static data
        public static void ClearCache() {
            _foundTemplates.Clear();
        }

        public static void LoadTemplates(IEnumerable<TemplateJson> templates) {
            foreach (var templateJson in templates) {
                EntityTemplate template = new EntityTemplate(templateJson.TemplateId);

                foreach (var dataJson in templateJson.Data) {
                    template.AddDefaultData(dataJson.GetDataInstance());
                }

                _foundTemplates[templateJson.TemplateId] = template;
            }
        }

        private static Dictionary<int, EntityTemplate> _foundTemplates = new Dictionary<int, EntityTemplate>();

        static TemplateJson() {
            JsonMapper.RegisterExporter<EntityTemplate>((template, writer) => {
                writer.Write(template.TemplateId);
            });

            JsonMapper.RegisterObjectImporter(typeof(EntityTemplate), jsonData => {
                EntityTemplate template;

                if (jsonData.IsInt) {
                    if (_foundTemplates.TryGetValue((int)jsonData, out template)) {
                        return template;
                    }

                    throw new Exception("No such template with id=" + (int)jsonData);
                }

                throw new Exception("Inline template definitions are not supported; must load template by referencing its TemplateId");
            });
        }
    }
}
