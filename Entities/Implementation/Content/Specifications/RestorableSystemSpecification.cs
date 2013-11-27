using Neon.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Content.Specifications {
    internal class RestorableSystemSpecification {
        public RestorableSystemSpecification(SerializedData data) {
            RestorationGuid = new Guid(data.AsDictionary["RestorationGuid"].AsString);
            SavedState = data.AsDictionary["SavedState"];
        }

        public RestorableSystemSpecification(IRestoredSystem system, SerializationConverter converter) {
            RestorationGuid = system.RestorationGuid;
            SavedState = system.ExportState(converter);
        }

        public SerializedData Export() {
            Dictionary<string, SerializedData> dict = new Dictionary<string, SerializedData>();

            dict["RestorationGuid"] = new SerializedData(RestorationGuid.ToString());
            dict["SavedState"] = SavedState;

            return new SerializedData(dict);
        }

        /// <summary>
        /// The ISystem GUID used for identifying the system to restore.
        /// </summary>
        public Guid RestorationGuid;

        /// <summary>
        /// The ISystem's saved state.
        /// </summary>
        public SerializedData SavedState;
    }
}