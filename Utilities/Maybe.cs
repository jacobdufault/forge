using System;

namespace Neon.Utilities {
    /// <summary>
    /// Helper for Maybe[T] by providing local type inference at Just and Empty call sites.
    /// </summary>
    public static class Maybe {
        /// <summary>
        /// Returns a new Maybe instance containing the given value.
        /// </summary>
        public static Maybe<T> Just<T>(T instance) {
            return new Maybe<T>(instance);
        }
    }

    /// <summary>
    /// Maybe wraps another type and is used to signal to other code that it might not return a
    /// result. It performs the same function as null, but in a more type-safe manner that provides
    /// more clarity into the contract that function exhibits.
    /// </summary>
    /// <typeparam name="T">The type of value stored in the Maybe instance</typeparam>
    public struct Maybe<T> {
        /// <summary>
        /// The stored value in the maybe instance. Only contains interesting data if _hasValue is
        /// true (otherwise the data is garbage).
        /// </summary>
        private readonly T _value;

        /// <summary>
        /// True if the maybe instance is currently holding a value.
        /// </summary>
        private readonly bool _hasValue;

        /// <summary>
        /// Creates a new Maybe container that holds the given value.
        /// </summary>
        public Maybe(T value)
            : this(value, true) {
        }

        /// <summary>
        /// Internal constructor used to construct the maybe. Used primarily in construction of the
        /// Empty element.
        /// </summary>
        private Maybe(T item, bool hasValue) {
            _value = item;
            _hasValue = hasValue;
        }

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        /// <remarks>
        /// If IsEmpty returns true, then this method will throw an InvalidOperationException.
        /// </remarks>
        public T Value {
            get {
                if (!_hasValue) {
                    throw new InvalidOperationException("Can't access value when maybe is empty");
                }

                return _value;
            }
        }

        /// <summary>
        /// Returns true if this Maybe has a value stored in it.
        /// </summary>
        public bool Exists {
            get {
                return _hasValue;
            }
        }

        /// <summary>
        /// Returns true if this Maybe is empty, it, it does not have a value stored in it.
        /// </summary>
        public bool IsEmpty {
            get {
                return _hasValue == false;
            }
        }

        public override string ToString() {
            if (IsEmpty) {
                return "Maybe [Empty]";
            }

            // use implicit string conversion instead of .ToString because _value could technically
            // be null
            return "Maybe [Just " + _value + "]";
        }

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T), false);
    }
}