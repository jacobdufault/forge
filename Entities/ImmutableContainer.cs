
using Neon.Utilities;
using System.Threading;
namespace Neon.Entities {
    /// <summary>
    /// Interface for immutable data types that exposes methods necessary for swapping between
    /// Previous, Current, and Modifiable instances.
    /// </summary>
    public interface IImmutableData<DerivedType> where DerivedType : IImmutableData<DerivedType> {
        /// <summary>
        /// Duplicates this instance of data.
        /// </summary>
        /// <returns>An exact copy of this data instance.</returns>
        DerivedType Duplicate();

        /// <summary>
        /// Make this data equivalent to the source data.
        /// </summary>
        /// <param name="source">The source data to copy from.</param>
        void CopyFrom(DerivedType source);

        /// <summary>
        /// Does this immutable data support concurrent, multithreaded changes to the data?
        /// </summary>
        /// <remarks>
        /// If this is true, then it is *CRITICAL* that *ALL* systems which modify the data do so in
        /// a relative manner; ie, the system cannot set a particular attribute to zero, but it can
        /// reduce it by five. If this is not followed, then state desyncs will occur.
        /// </remarks>
        bool SupportsConcurrentModifications { get; }

        /// <summary>
        /// If this immutable data supports concurrent modifications, then this method will be
        /// called to resolve any potential issues.
        /// </summary>
        void ResolveConcurrentModifications();
    }

    /// <summary>
    /// Contains a set of IImmutableData and allows swapping between those instances such that one
    /// of them is the previous state, one of them is the current state, and one of them is a
    /// modifiable state.
    /// </summary>
    internal class ImmutableContainer<DataType> where DataType : IImmutableData<DataType> {
        /// <summary>
        /// All stored immutable data items.
        /// </summary>
        public DataType[] Items;

        /// <summary>
        /// The index of the previous item.
        /// </summary>
        private int _previousIndex;

        /// <summary>
        /// Used by the Entity to determine if the data inside of this container has already been
        /// modified.
        /// </summary>
        public AtomicActivation MotificationActivation;

        /// <summary>
        /// Initializes a new instance of the ImmutableContainer class.
        /// </summary>
        /// <param name="data">The initial data instance.</param>
        public ImmutableContainer(DataType data) {
            Items = new DataType[] { data.Duplicate(), data.Duplicate(), data.Duplicate() };
            MotificationActivation = new AtomicActivation();
        }

        /// <summary>
        /// Return the data instance that contains the current values.
        /// </summary>
        public DataType Current {
            get {
                return Items[(_previousIndex + 1) % Items.Length];
            }
        }

        /// <summary>
        /// Return the data instance that can be modified.
        /// </summary>
        public DataType Modifying {
            get {
                return Items[(_previousIndex + 2) % Items.Length];
            }
        }

        /// <summary>
        /// Return the data instance that contains the previous values.
        /// </summary>
        public DataType Previous {
            get {
                return Items[(_previousIndex + 0) % Items.Length];
            }
        }

        /// <summary>
        /// Updates Modifying/Current/Previous so that they point to the next element
        /// </summary>
        public void Increment() {
            // If the object we modified supports multiple modifications, then we need to give it a
            // chance to resolve those modifications so that it is in a consistent state
            if (Modifying.SupportsConcurrentModifications) {
                Modifying.ResolveConcurrentModifications();
            }

            // this code is tricky because we change what data.{Previous,Current,Modifying} refer to
            // when we increment _swappableIndex below

            // when we increment _swappableIndex: modifying becomes current current becomes previous
            // previous becomes modifying

            // current refers to the current _swappableIndex current_modifying : data.Modifying
            // current_current : data.Current current_previous : data.Previous

            // next refers to when _swappableIndex += 1 next_modifying : data.Previous next_current
            // : data.Modifying next_previous : data.Current

            // we want next_modifying to become a copy of current_modifying
            Previous.CopyFrom(Modifying);
            // we want next_current to become a copy of current_modifying
            //Modifying.CopyFrom(data.Modifying);
            // we want next_previous to become a copy of current_current
            //Current.CopyFrom(data.Current);

            // update Modifying/Current/Previous references
            ++_previousIndex;
        }
    }
}