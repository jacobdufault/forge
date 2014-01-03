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

using Forge.Entities.Implementation.Shared;
using System;

namespace Forge.Entities {

    /// <summary>
    /// Provides a convenient and efficient way to access a type of Data.
    /// </summary>
    public struct DataAccessor {
        /// <summary>
        /// Helper constructor for DataAccessor(Type). This merely forwards the call with the type
        /// parameter being data.GetType().
        /// </summary>
        public DataAccessor(Data.IData data)
            : this(data.GetType()) {
        }

        /// <summary>
        /// Creates a new DataAccessor that accesses the specified Data type.
        /// </summary>
        /// <param name="dataType">The type of Data to retrieve; note that this parameter must be a
        /// subtype of Data</param>
        public DataAccessor(Type dataType)
            : this() {
            Id = DataAccessorFactory.GetId(dataType);
        }

        /// <summary>
        /// Directly construct a DataAccessor with the given id.
        /// </summary>
        /// <param name="id">The id of the DataAccessor</param>
        internal DataAccessor(int id)
            : this() {
            Id = id;
        }

        /// <summary>
        /// Returns the mapped id for this accessor.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public override bool Equals(Object obj) {
            return obj is DataAccessor && this == (DataAccessor)obj;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode() {
            return Id.GetHashCode();
        }
        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public static bool operator ==(DataAccessor x, DataAccessor y) {
            return x.Id == y.Id;
        }
        /// <summary>
        /// Indicates whether this instance and a specified object are not equal.
        /// </summary>
        public static bool operator !=(DataAccessor x, DataAccessor y) {
            return !(x == y);
        }

        /// <summary>
        /// Helper method to get the type of data the given DataAccessor maps back to.
        /// </summary>
        public static Type GetDataType(DataAccessor accessor) {
            return DataAccessorFactory.GetTypeFromAccessor(accessor);
        }
    }

    /// <summary>
    /// Map a data type to its respective accessor, at compile time.
    /// </summary>
    /// <typeparam name="T">The type of Data to map.</typeparam>
    public class DataMap<T> where T : Data.IData {
        static DataMap() {
            Accessor = new DataAccessor(typeof(T));
        }

        /// <summary>
        /// Gets the accessor for the specified data type.
        /// </summary>
        public static DataAccessor Accessor {
            get;
            private set;
        }
    }
}