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

using Neon.Entities.Implementation.Shared;
using Newtonsoft.Json;
using System;

namespace Neon.Entities {
    /// <summary>
    /// Contains game data.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public interface IData {
        /// <summary>
        /// Does this data type support concurrent modifications, such as from two threads at the
        /// same time, or just multiple modifications in the same frame?
        /// </summary>
        bool SupportsConcurrentModifications {
            get;
        }

        /// <summary>
        /// If SupportsConcurrentModifications is true *and* multiple modifications have been made
        /// to the data instance, then this method is called after all modifications have been made.
        /// The purpose is to allow for client code to resolve multiple modifications so that
        /// modA(modB(entity)) == modB(modA(entity)).
        /// </summary>
        /// <remarks>
        /// No other calls will be made to the data instance while this function is being executed.
        /// </remarks>
        void ResolveConcurrentModifications();

        /// <summary>
        /// Moves all of the data from the specified source into this instance. After this call,
        /// this data instance must be identical to source, such that this instance could completely
        /// replace source in other code and the other code would be unable to tell the difference.
        /// </summary>
        /// <param name="source">The source to move from.</param>
        void CopyFrom(IData source);

        /// <summary>
        /// Return an exact copy of this data instance.
        /// </summary>
        IData Duplicate();
    }

    /// <summary>
    /// Base data type that makes implementing IData easier.
    /// </summary>
    /// <typeparam name="TData">The type that is extending BaseData.</typeparam>
    public abstract class BaseData<TData> : IData {
        /// <summary>
        /// Does this data type support concurrent modifications (two or more modifications made in
        /// the same update loop, potentially in different threads at the same time).
        /// </summary>
        /// <remarks>
        /// If this returns true, then ResolveConcurrentModifications will be called at the end of
        /// the update loop (which by default throws an exception, so make sure to override it).
        /// </remarks>
        public virtual bool SupportsConcurrentModifications {
            get {
                return false;
            }
        }

        /// <summary>
        /// Resolve any modifications that have occurred to the data instance. This method will only
        /// be called if SupportsConcurrentModifications returns true. By default, this method
        /// throws an exception.
        /// </summary>
        public virtual void ResolveConcurrentModifications() {
            throw new InvalidOperationException("Type " + GetType() + " supports concurrent " +
                "modifications but did not override ResolveConcurrentModifications");
        }

        /// <summary>
        /// Copy the data from source into this object instance. After this method is done being
        /// called, source should be identical in content to this (such that this.Equals(source) ==
        /// true) .
        /// </summary>
        /// <param name="source">The source data object to copy from.</param>
        public abstract void CopyFrom(TData source);

        void IData.CopyFrom(IData source) {
            CopyFrom((TData)source);
        }

        IData IData.Duplicate() {
            return (IData)MemberwiseClone();
        }
    }

    /// <summary>
    /// Provides a convenient and efficient way to access a type of Data.
    /// </summary>
    public struct DataAccessor {
        /// <summary>
        /// Helper constructor for DataAccessor(Type). This merely forwards the call with the type
        /// parameter being data.GetType().
        /// </summary>
        public DataAccessor(IData data)
            : this(data.GetType()) {
        }

        /// <summary>
        /// Creates a new DataAccessor that accesses the specified Data type.
        /// </summary>
        /// <param name="dataType">The type of Data to retrieve; note that this parameter must be a
        /// subtype of Data</param>
        public DataAccessor(Type dataType)
            : this() {
            if (dataType == typeof(IData) || typeof(IData).IsAssignableFrom(dataType) == false) {
                throw new ArgumentException("Type " + dataType + " is not a subtype of " +
                    typeof(IData));
            }

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
    }

    /// <summary>
    /// Map a data type to its respective accessor, at compile time.
    /// </summary>
    /// <typeparam name="T">The type of Data to map.</typeparam>
    public class DataMap<T> where T : IData {
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