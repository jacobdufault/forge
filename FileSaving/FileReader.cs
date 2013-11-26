using Neon.Serialization;
using Neon.Utilities;
using System;

namespace Neon.FileSaving {
    /// <summary>
    /// Reads a saved file and allows for file items to be read from the file.
    /// </summary>
    public class FileReader {
        /// <summary>
        /// The raw saved data.
        /// </summary>
        private SerializedData _data;

        /// <summary>
        /// Read a file from the given raw file contents (this is *not* a path).
        /// </summary>
        public FileReader(string contents) {
            _data = Parser.Parse(contents);
        }

        /// <summary>
        /// Attempt to retrieve a file item from this reader. If the item is within the loaded data,
        /// then an instance of it is returned, otherwise an empty maybe is returned.
        /// </summary>
        /// <typeparam name="T">The type of file item to retrieve</typeparam>
        /// <returns>The file item, if found. Otherwise an empty Maybe.</returns>
        public Maybe<T> GetFileItem<T>() where T : IFileItem, new() {
            T item = new T();

            foreach (var dataItem in _data.AsList) {
                Guid dataItemGuid = new Guid(dataItem.AsDictionary["SectionGuid"].AsString);

                if (dataItemGuid == item.Identifier) {
                    item.Import(dataItem.AsDictionary["Data"]);
                    return Maybe.Just(item);
                }
            }

            return Maybe<T>.Empty;
        }
    }
}