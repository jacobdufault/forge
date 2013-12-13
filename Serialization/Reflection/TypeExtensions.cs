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

using Neon.Utilities;
using System;

namespace Neon.Serialization {
    /// <summary>
    /// Extensions to the Type API.
    /// </summary>
    public static class TypeExtensions {
        /// <summary>
        /// Searches for a particular implementation of the given interface type inside of the type.
        /// This is particularly useful if the interface type is an open type, ie, typeof(IFace[]),
        /// because this method will then return IFace[] but with appropriate type parameters
        /// inserted.
        /// </summary>
        /// <param name="type">The base type to search for interface</param>
        /// <param name="interfaceType">The interface type to search for. Can be an open generic
        /// type.</param>
        /// <returns>The actual interface type that the type contains, or null if there is no
        /// implementation of the given interfaceType on type.</returns>
        public static Type GetInterface(this Type type, Type interfaceType) {
            if (interfaceType.IsGenericType && interfaceType.IsGenericTypeDefinition == false) {
                throw new ArgumentException("GetInterface requires that if the interface " +
                    "type is generic, then it must be the generic type definition, not a " +
                    "specific generic type instantiation");
            };

            while (type != null) {
                foreach (var iface in type.GetInterfaces()) {
                    if (iface.IsGenericType) {
                        if (interfaceType == iface.GetGenericTypeDefinition()) {
                            return iface;
                        }
                    }

                    else if (interfaceType == iface) {
                        return iface;
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the baseType is an implementation of the given interface type. The
        /// interface type can be generic.
        /// </summary>
        /// <param name="type">The type to search for an implementation of the given
        /// interface</param>
        /// <param name="interfaceType">The interface type to search for</param>
        /// <returns></returns>
        public static bool IsImplementationOf(this Type type, Type interfaceType) {
            if (interfaceType.IsGenericType && interfaceType.IsGenericTypeDefinition == false) {
                throw new ArgumentException("IsImplementationOf requires that if the interface " +
                    "type is generic, then it must be the generic type definition, not a " +
                    "specific generic type instantiation");
            };

            if (type.IsGenericType) {
                type = type.GetGenericTypeDefinition();
            }

            while (type != null) {
                foreach (var iface in type.GetInterfaces()) {
                    if (iface.IsGenericType) {
                        if (interfaceType == iface.GetGenericTypeDefinition()) {
                            return true;
                        }
                    }

                    else if (interfaceType == iface) {
                        return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}