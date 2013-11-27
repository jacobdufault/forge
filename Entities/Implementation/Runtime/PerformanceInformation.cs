
namespace Neon.Entities.Implementation.Runtime {
    /// <summary>
    /// This stores performance information that can be used to debug slow running code. It is used
    /// in Systems when they are updated.
    /// </summary>
    internal class PerformanceInformation {
        /// <summary>
        /// Total number of ticks running the system required.
        /// </summary>
        public long RunSystemTicks;

        /// <summary>
        /// Total number of bookkeeping ticks required.
        /// </summary>
        public long BookkeepingTicks;

        /// <summary>
        /// Ticks required for adding entities when running the system.
        /// </summary>
        public long AddedTicks;

        /// <summary>
        /// Ticks required for removing entities when running the system.
        /// </summary>
        public long RemovedTicks;

        /// <summary>
        /// Ticks required for state change operations when running the system.
        /// </summary>
        public long StateChangeTicks;

        /// <summary>
        /// Ticks required for modification operations when running the system.
        /// </summary>
        public long ModificationTicks;

        /// <summary>
        /// Ticks required for updating the system.
        /// </summary>
        public long UpdateTicks;
    }
}