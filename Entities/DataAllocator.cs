using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// Allocates instances of Data.
    /// </summary>
    // TODO: remove this class
    public sealed class DataAllocator {
        /// <summary>
        /// Used to allocate types of a given object
        /// </summary>
        private class Allocator {
            private Type _allocationType;
            private List<Action<Data, IEntity>> _auxiliaryAllocators;

            public Allocator(DataAccessor accessor) {
                _allocationType = DataAccessorFactory.GetTypeFromAccessor(accessor);
                _auxiliaryAllocators = new List<Action<Data, IEntity>>();
            }

            public Data Allocate() {
                // speed:
                // fastest: type erasure (aka (Data)new T())
                // medium: delegate
                // slowest: activator
                return (Data)Activator.CreateInstance(_allocationType);
            }

            public void NotifyAllocated(IEntity context, Data allocated) {
                if (RequireAuxiliaryAllocator && _auxiliaryAllocators.Count == 0) {
                    Log<DataAllocator>.Error("Auxiliary data allocator required, but one was not found for allocation type {0}. Did you forget to register it?", _allocationType);
                }

                for (int i = 0; i < _auxiliaryAllocators.Count; ++i) {
                    _auxiliaryAllocators[i](allocated, context);
                }
            }

            public void AddAuxiliaryAllocator(Action<Data, IEntity> allocator) {
                _auxiliaryAllocators.Add(allocator);
            }
        }

        // indexed by DataAccessor
        private static SparseArray<Allocator> _allocators = new SparseArray<Allocator>();

        /// <summary>
        /// If this is true, then if a Data instance is allocated but there are no allocators
        /// registered for that Data type then an exception is thrown.
        /// </summary>
        /// <remarks>
        /// This is extremely useful if all Data allocations must be paired with some other
        /// object, as this can catch bugs where a data type is not paired.
        /// </remarks>
        public static bool RequireAuxiliaryAllocator = false;

        /// <summary>
        /// Allocate data of a given type with a context of the given entity.
        /// </summary>
        /// <param name="accessor">The type of data to allocate</param>
        /// <returns></returns>
        public static Data Allocate(DataAccessor accessor) {
            return GetOrCreate(accessor).Allocate();
        }

        public static void NotifyAllocated(DataAccessor accessor, IEntity context, Data allocated) {
            GetOrCreate(accessor).NotifyAllocated(context, allocated);
        }

        /// <summary>
        /// When an instance of the given accessor type is allocated, also run the given
        /// allocator function on the IEntity target.
        /// </summary>
        /// <param name="accessor">The type of data that is allocated</param>
        /// <param name="allocator">The code to run when that data is allocated</param>
        public static void AddAuxiliaryAllocator(DataAccessor accessor, Action<Data, IEntity> allocator) {
            GetOrCreate(accessor).AddAuxiliaryAllocator(allocator);
        }

        /// <summary>
        /// Calls AllocateWith(DataMap[T].Accessor, allocator)
        /// </summary>
        public static void AddAuxiliaryAllocator<T>(Action<Data, IEntity> allocator) where T : Data {
            AddAuxiliaryAllocator(DataMap<T>.Accessor, allocator);
        }

        // looks up what data type to create from the accessor
        // or creates a new allocator if we don't have one
        private static Allocator GetOrCreate(DataAccessor accessor) {
            int id = accessor.Id;
            if (_allocators.Contains(id) == false) {
                _allocators[id] = new Allocator(accessor);
            }

            return _allocators[id];
        }
    }
}
