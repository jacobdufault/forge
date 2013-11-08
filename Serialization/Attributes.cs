using System;

namespace Neon.Serialization {
    /// <summary>
    /// A type that is has a [SerializationRequireCustomConverter] attribute will cause the
    /// SerializationConverter to throw an exception if there is no custom converter registered for
    /// importing/exporting the type. Stated differently, the annotated with will not be imported or
    /// exported using reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
        Inherited = true, AllowMultiple = false)]
    public sealed class SerializationRequireCustomConverterAttribute : Attribute { }

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
    public sealed class SerializationNoAutoInheritance : Attribute { }
}