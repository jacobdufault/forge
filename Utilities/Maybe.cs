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

    public static class MaybeExtensions {
        /// <summary>
        /// Lifts a maybe into another maybe using the given lifting function. If the given maybe is
        /// empty, then an empty maybe is returned.
        /// </summary>
        /// <typeparam name="T0">The type of the original maybe</typeparam> <typeparam name="T1">The
        /// type of the new maybe</typeparam>
        /// <param name="maybe">The maybe to transform</param>
        /// <param name="lifter">The lifting function that will transform the maybe</param>
        /// <returns>A new maybe created by the lifting function</returns>
        public static Maybe<T1> Lift<T0, T1>(this Maybe<T0> maybe, Func<T0, Maybe<T1>> lifter) {
            if (maybe.Exists) {
                return lifter(maybe.Value);
            }

            return Maybe<T1>.Empty;
        }

        /// <summary>
        /// Lifts a maybe into another maybe using the given lifting function. If the given maybe is
        /// empty, then an empty maybe is returned.The returned maybe is never empty.
        /// </summary>
        /// <typeparam name="T0">The type of the original maybe</typeparam> <typeparam name="T1">The
        /// type of the new maybe</typeparam>
        /// <param name="maybe">The maybe to transform</param>
        /// <param name="lifter">The lifting function that will transform the maybe</param>
        /// <returns>A new maybe created by the lifting function</returns>
        public static Maybe<T1> Lift<T0, T1>(this Maybe<T0> maybe, Func<T0, T1> lifter) {
            if (maybe.Exists) {
                return Maybe.Just(lifter(maybe.Value));
            }

            return Maybe<T1>.Empty;
        }

        /// <summary>
        /// C# has a limitation where non-reference generic types cannot be contravariant (the Maybe
        /// generic type should be contravariant). This function eases that limitation by providing
        /// automatic casting to a higher Maybe type.
        /// </summary>
        public static Maybe<TBase> Lift<TDerived, TBase>(this Maybe<TDerived> maybe)
            where TDerived : TBase {
            if (maybe.Exists) {
                return Maybe.Just((TBase)maybe.Value);
            }

            return Maybe<TBase>.Empty;
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