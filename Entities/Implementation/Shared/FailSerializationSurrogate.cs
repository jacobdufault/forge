using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// A type that is meant to act as a surrogate for other types when serializing them using
    /// ProtoBuf. When the given type is serialized, then an exception is thrown, stating that the
    /// type cannot be serialized. This can be used to mark a type temporarily not serializable.
    /// </summary>
    /// <remarks>
    /// Usage is simple;
    ///
    /// <![CDATA[
    ///
    /// RuntimeTypeModel metadata = RuntimeTypeModel.Create();
    ///
    /// metadata.Add(typeof(SimpleType), false).SetSurrogate(typeof(FailSurrogate<SimpleType>));
    ///
    /// ]]>
    /// </remarks>
    /// <typeparam name="T">The object type to mark as not serializable.</typeparam>
    [ProtoContract]
    internal class FailSerializationSurrogate<T> {
        public static implicit operator T(FailSerializationSurrogate<T> t) {
            throw new InvalidOperationException("Attempt to serialize type " + typeof(T) +
                " which is not applicable in this context");
        }

        public static implicit operator FailSerializationSurrogate<T>(T t) {
            throw new InvalidOperationException("Attempt to serialize type " + typeof(T) +
                " which is not applicable in this context");
        }
    }
}