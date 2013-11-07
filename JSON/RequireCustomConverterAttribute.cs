using System;

namespace Neon.Serialization {
    /// <summary>
    /// A type that is has a [RequireCustomConverter] attribute will cause the
    /// SerializationConverter to throw an exception if there is no custom converter registered for
    /// importing/exporting the type. Stated differently, the annotated with will not be imported or
    /// exported using reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
        Inherited = true, AllowMultiple = false)]
    public sealed class RequireCustomConverterAttribute : Attribute { }
}