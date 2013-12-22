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