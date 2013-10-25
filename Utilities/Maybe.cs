using System;

namespace Neon.Utilities {
    public struct MaybeU {
        public static Maybe<T> Just<T>(T instance) {
            return new Maybe<T>(instance);
        }

        public static Maybe<T> Empty<T>() {
            return Maybe<T>.Empty;
        }
    }

    public struct Maybe<T> {
        private readonly T value;

        private readonly bool hasValue;

        private Maybe(T item, bool hasValue) {
            value = item;
            this.hasValue = hasValue;
        }

        /// <summary> Gets the underlying value, if it is available </summary>
        /// <value>The value.</value>
        public T Value {
            get {
                if (!hasValue) {
                    throw new InvalidOperationException("Can't access value when maybe is empty");
                }

                return value;
            }
        }

        public Maybe(T value)
            : this(value, true) {
        }

        public bool Exists {
            get {
                return hasValue;
            }
        }

        public bool IsEmpty {
            get {
                return hasValue == false;
            }
        }

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T), false);

        /// <summary>
        /// Convenience method to return just the given instance.
        /// </summary>
        public static Maybe<T> Just(T instance) {
            return new Maybe<T>(instance);
        }
    }
}