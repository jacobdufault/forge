using Neon.Serialization;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// Serialization specification for a saved ISystem state.
    /// </summary>
    internal class SerializedSystem {
        /// <summary>
        /// The ISystem GUID used for identifying the system to restore.
        /// </summary>
        public string RestorationGUID;

        /// <summary>
        /// The ISystem's saved state.
        /// </summary>
        public SerializedData SavedState;
    }
}