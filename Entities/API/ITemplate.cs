
namespace Neon.Entities {
    /// <summary>
    /// An IEntityTemplate is used for creating IEntity instances that have a set of data values
    /// already initialized.
    /// </summary>
    /// <remarks>
    /// For example, a generic Orc type will have an IEntityTemplate for it. Spawning code will then
    /// receive the Orc IEntityTemplate, and when it comes time to spawn it will instantiate an
    /// entity from the template, and that entity will be a derivate instance of the original Orc.
    /// </remarks>
    public interface ITemplate : IQueryableEntity {
        /// <summary>
        /// Each IEntityTemplate can be uniquely identified by its TemplateId.
        /// </summary>
        int TemplateId {
            get;
        }

        /// <summary>
        /// Creates a new IEntity instance.
        /// </summary>
        IEntity InstantiateEntity();

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <param name="data">The data instance to copy from.</param>
        void AddDefaultData(IData data);
    }
}