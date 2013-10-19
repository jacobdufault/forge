using Neon.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Entities {
    public abstract class Data {
        /// <summary>
        /// The Entity that contains this Data instance.
        /// </summary>
        public IEntity Entity;

        /// <summary>
        /// Creates a copy of this instance.
        /// </summary>
        public Data Duplicate() {
            return (Data)MemberwiseClone();
        }

        /// <summary>
        /// Optionally update any visualizations of this Data instance to the value currently stored in this instance.
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

        /// <summary>
        /// Does this type of Data support multiple modifications; ie, two systems can modify the same data instance in the same update loop.
        /// </summary>
        /// <remarks>
        /// If this is true, then it is *CRITICAL* that *ALL* systems which modify the data do so in a relative manner; ie, the system cannot
        /// set a particular attribute to zero, but it can reduce it by five. If this is not followed, then state desyncs will occur.
        /// </remarks>
        public virtual bool SupportsMultipleModifications {
            get {
                return false;
            }
        }

        /// <summary>
        /// If SupportsMultipleModifications is true, then this method is called after all modifications
        /// have been executed. The purpose is to allow for client code to resolve multiple modifications so that
        /// modA(modB(entity)) == modB(modA(entity))
        /// </summary>
        public virtual void ResolveModifications() {
            // The client code supports multiple modifications; it may also need to resolve them.
            throw new Exception("Type allows multiple modifications but does not support resolving them; override this method");
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
        public sealed override void CopyFrom(Data source) {
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
        /// Please note that a data instance can only be modified once; an exception is thrown
        /// if one instance is modified multiple times.
        /// </summary>
        /// <remarks>
        /// This is a shortcut method that simply forwards the call to Entity.Modify[T]()
        /// </remarks>
        public T Modify() {
            return Entity.Modify<T>();
        }
    }
}