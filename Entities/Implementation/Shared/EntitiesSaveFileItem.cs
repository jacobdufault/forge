using Neon.Entities.Implementation.Content;
using Neon.FileSaving;
using Neon.Serialization;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Shared {
    internal class EntitiesSaveFileItem : ISaveFileItem {
        private bool _runtimeImport;

        /// <summary>
        /// Constructor. If runtimeImport is true, then the imported runtime system will be used
        /// when doing imports, as opposed to the content system.
        /// </summary>
        /// <param name="runtimeImport">If true, then the runtime system will be used to satisfy
        /// IEntity, ITemplate, etc.</param>
        public EntitiesSaveFileItem(bool runtimeImport) {
            _runtimeImport = runtimeImport;
        }

        public Guid Identifier {
            get { return new Guid("C8C0303D-97C9-4876-91C0-F6259CCE6760"); }
        }

        public string PrettyIdentifier {
            get { return "Neon.Entities Saved State (format v1.0)"; }
        }

        public SerializedData Export() {
            throw new NotImplementedException();
        }

        public void Import(SerializedData data) {
            if (_runtimeImport) {
                throw new NotImplementedException();
            }

            else {
                SerializationConverter converter = new SerializationConverter();

                List<ITemplate> templates = new List<ITemplate>();

                List<ISystem> systems = new List<ISystem>();

                IContentDatabase current = ContentDatabase.Read(data.AsDictionary["Current"], converter, templates, systems);
                IContentDatabase original = ContentDatabase.Read(data.AsDictionary["Original"], converter, templates, systems);

                throw new NotImplementedException();
            }
        }
    }
}