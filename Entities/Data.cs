using Neon.Serialization;
using Neon.Utilities;
using System;

namespace Neon.Entities {
    [SerializationNoAutoInheritance]
    public abstract class Data : IImmutableData<Data> {
        /// <summary>
        /// The Entity that contains this Data instance.
        /// </summary>
        [NonSerialized]
        public IEntity Entity;

        /// <summary>
        /// Creates a copy of this instance.
        /// </summary>
        public virtual Data Duplicate() {
            return (Data)MemberwiseClone();
        }

        /// <summary>
        /// Optionally update any visualizations of this Data instance to the value currently stored
        /// in this instance.
        /// </summary>
        public void DoUpdateVisualization() {
            if (OnUpdateVisualization != null) {
                OnUpdateVisualization(this);
            }
        }

        /// <summary>
        /// Called when this data has new values that the attached Entity should visualize.
        /// </summary>
        public event Action<Data> OnUpdateVisualization;

        public virtual bool SupportsConcurrentModifications {
            get {
                return false;
            }
        }

        /// <summary>
        /// If SupportsMultipleModifications is true, then this method is called after all
        /// modifications have been executed. The purpose is to allow for client code to resolve
        /// multiple modifications so that modA(modB(entity)) == modB(modA(entity))
        /// </summary>
        public virtual void ResolveConcurrentModifications() {
            // The client code supports multiple modifications; it may also need to resolve them.
            throw new Exception("Type " + GetType() + " allows multiple modifications but does not support resolving them; override this method");
        }

        /// <summary>
        /// Moves all of the data from the specified source into this instance. After this call,
        /// this will be identical to source.
        /// </summary>
        /// <param name="source">The source to move from.</param>
        public abstract void CopyFrom(Data source);

        /// <summary>
        /// A stored hash code that is used for verification purposes
        /// </summary>
        [NonSerialized]
        public int VerificationHashCode;

        public override int GetHashCode() {
            return HashCode;
        }

        /// <summary>
        /// Compute the hash code of the Data element.
        /// </summary>
        /// <remarks>
        /// Use this.CalculateHashCode(...) for simple hash code computation
        /// </remarks>
        public abstract int HashCode {
            get;
        }
    }

    /// <summary>
    /// Typed data that derives from Data but is easier for clients to implement.
    /// </summary>
    /// <typeparam name="T">The type of data we are implementing</typeparam>
    public abstract class GameData<T> : Data where T : GameData<T> {
        public override sealed void CopyFrom(Data source) {
            if (ReferenceEquals(this, source)) {
                return;
            }

            DoCopyFrom((T)source);
        }

        /// <summary>
        /// Moves all of the data from the specified source into this instance. After this call,
        /// this will be identical to source.
        /// </summary>
        /// <param name="source">The source to move from.</param>
        public abstract void DoCopyFrom(T source);

        /// <summary>
        /// Modify the given data instance. The current and previous values are still accessible.
        /// Please note that a data instance can only be modified once; an exception is thrown if
        /// one instance is modified multiple times.
        /// </summary>
        /// <remarks>
        /// This is a shortcut method that simply forwards the call to Entity.Modify[T]()
        /// </remarks>
        public T Modify() {
            return Entity.Modify<T>();
        }
    }
}