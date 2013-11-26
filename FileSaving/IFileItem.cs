using Neon.Serialization;
using System;

namespace Neon.FileSaving {
    /// <summary>
    /// An item that is within a save file.
    /// </summary>
    public interface IFileItem {
        /// <summary>
        /// The globally unique identifier that identifies this file item.
        /// </summary>
        Guid Identifier {
            get;
        }

        /// <summary>
        /// The pretty name of this file item; this is useful merely for debugging purposes.
        /// </summary>
        string PrettyIdentifier {
            get;
        }

        /// <summary>
        /// Export the serialized state of this item.
        /// </summary>
        SerializedData Export();

        /// <summary>
        /// Import the given data into this item.
        /// </summary>
        void Import(SerializedData data);
    }
}