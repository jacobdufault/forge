
using Neon.Entities.Implementation.Shared;
using System;

namespace Neon.Entities {
    /// <summary>
    /// Contains game data.
    /// </summary>
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

    public abstract class BaseData<TData> : IData {
        public abstract bool SupportsConcurrentModifications {
            get;
        }

        public virtual void ResolveConcurrentModifications() {
            throw new InvalidOperationException("Type " + GetType() + " supports concurrent " +
                "modifications but did not override ResolveConcurrentModifications");
        }

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