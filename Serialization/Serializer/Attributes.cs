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

namespace Neon.Serialization {
    /// <summary>
    /// Changes the serialization format of an object so that it will support cyclic references.
    /// This annotation has a large change on the serialization format (making it less clear) and
    /// has potential performance implications, and additionally requires the converter to
    /// export/import metadata, so try to minimize the number of types which need to support cyclic
    /// references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true,
        AllowMultiple = false)]
    public sealed class SerializationSupportCyclicReferencesAttribute : Attribute { }

    /// <summary>
    /// A type that has a [SerializationSupportInheritance] attribute will cause an automatic
    /// importer and exporter to be registered for the type which will allow for correct
    /// serialization and deserialization of derived types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
        Inherited = false, AllowMultiple = false)]
    public sealed class SerializationSupportInheritance : Attribute { }

    /// <summary>
    /// Annotate a type with this class to *not* automatically inject inheritance support if the
    /// type is abstract or an interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
        Inherited = false, AllowMultiple = false)]
    public sealed class SerializationNoAutoInheritance : Attribute { }

    /// <summary>
    /// Marks that a specified field/type/etc should not be serialized. This is equivalent to
    /// [NonSerialized] except it can be applied to a significantly larger number of types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct |
        AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event,
        Inherited = true, AllowMultiple = false)]
    public sealed class NotSerializableAttribute : Attribute { }
}