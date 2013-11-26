using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.FileSaving {
    /// <summary>
    /// Writes save files. Save files are just a set of file items. Each file item may only be
    /// written to a file once.
    /// </summary>
    public class FileWriter {
        /// <summary>
        /// The data that we will ultimately write.
        /// </summary>
        private SerializedData _data;

        /// <summary>
        /// The GUIDs of the IFileItems that we have already written. This is used to verify that we
        /// do not rewrite an item.
        /// </summary>
        private HashSet<Guid> _writtenFileItems;

        /// <summary>
        /// Creates a new file writer that is empty.
        /// </summary>
        public FileWriter() {
            _data = SerializedData.CreateList();
            _writtenFileItems = new HashSet<Guid>();
        }

        /// <summary>
        /// Write a new file item to the file. If the item has already been written to the file, an
        /// exception is thrown.
        /// </summary>
        /// <param name="item">The item to write.</param>
        public void WriteFileItem(IFileItem item) {
            Contract.AssertArguments(item, "item");

            if (_writtenFileItems.Add(item.Identifier) == false) {
                throw new InvalidOperationException("The item identified by guid=" +
                    item.Identifier + (" (aka " + item.PrettyIdentifier +
                    ") has already been written to the file"));
            }

            SerializedData data = SerializedData.CreateDictionary();

            data.AsDictionary["SectionGuid"] = new SerializedData(item.Identifier.ToString());
            data.AsDictionary["Name"] = new SerializedData(item.PrettyIdentifier ?? "");
            data.AsDictionary["Data"] = item.Export();

            _data.AsList.Add(data);
        }

        /// <summary>
        /// Returns a string representation of the file that can be written directly to disk.
        /// </summary>
        public string FileContents {
            get {
                return _data.PrettyPrinted;
            }
        }
    }
}