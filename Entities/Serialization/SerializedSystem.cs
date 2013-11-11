using Neon.Serialization;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// Serialization specification for a saved ISystem state.
    /// </summary>
    public class SerializedSystem {
        /// <summary>
        /// The ISystem GUID used for identifying the system to restore.
        /// </summary>
        public string RestorationGUID;

        /// <summary>
        /// The ISystem's saved JSON.
        /// </summary>
        public SerializedData SavedState;
    }
}