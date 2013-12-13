// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Neon.Serialization;
using Neon.Utilities;
using System;

namespace Neon.FileSaving {
    /// <summary>
    /// Reads a saved file and allows for file items to be read from the file.
    /// </summary>
    public class SavedStateReader {
        /// <summary>
        /// The raw saved data.
        /// </summary>
        private SerializedData _data;

        /// <summary>
        /// Read a file from the given raw file contents (this is *not* a path).
        /// </summary>
        public SavedStateReader(string contents) {
            _data = Parser.Parse(contents);
        }

        /// <summary>
        /// Attempt to retrieve a file item from this reader. If the item is within the loaded data,
        /// then an instance of it is returned, otherwise an empty maybe is returned.
        /// </summary>
        /// <typeparam name="T">The type of file item to retrieve</typeparam>
        /// <returns>The file item, if found. Otherwise an empty Maybe.</returns>
        public Maybe<T> GetFileItem<T>() where T : ISaveFileItem, new() {
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