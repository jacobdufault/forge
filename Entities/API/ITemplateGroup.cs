using System.Collections.Generic;

namespace Neon.Entities {
    /// <summary>
    /// An ITemplateGroup is simply a collection of templates that IGameSnapshots use.
    /// </summary>
    public interface ITemplateGroup {
        /// <summary>
        /// All of the templates that are within the group.
        /// </summary>
        IEnumerable<ITemplate> Templates { get; }

        /// <summary>
        /// Creates a new ITemplate instance that is attached to this snapshot.
        /// </summary>
        ITemplate CreateTemplate();
    }
}