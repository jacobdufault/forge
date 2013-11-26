
using Neon.Utilities;
using System.Threading;

namespace Neon.Entities {
    /// <summary>
    /// Contains a set of three IData instances and allows swapping between those instances such
    /// that one of them is the previous state, one of them is the current state, and one of them is
    /// a modifiable state.
    /// </summary>
    internal class DataContainer {
        /// <summary>
        /// All stored immutable data items.
        /// </summary>
        public IData[] Items;

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
        public DataContainer(IData previous, IData current, IData modified) {
            Items = new IData[] { previous, current, modified };
            MotificationActivation = new AtomicActivation();
        }

        /// <summary>
        /// Return the data instance that contains the current values.
        /// </summary>
        public IData Current {
            get {
                return Items[(_previousIndex + 1) % Items.Length];
            }
        }

        /// <summary>
        /// Return the data instance that can be modified.
        /// </summary>
        public IData Modifying {
            get {
                return Items[(_previousIndex + 2) % Items.Length];
            }
        }

        /// <summary>
        /// Return the data instance that contains the previous values.
        /// </summary>
        public IData Previous {
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