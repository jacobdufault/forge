namespace Neon.Entities.Serialization {
    /// <summary>
    /// Provides a set of instantiated systems that will be used when the engine is executing.
    /// </summary>
    internal interface ISystemProvider {
        /// <summary>
        /// Return all systems that should be processed while the engine is executing.
        /// </summary>
        ISystem[] GetSystems();
    }
}