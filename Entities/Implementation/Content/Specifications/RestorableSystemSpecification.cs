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