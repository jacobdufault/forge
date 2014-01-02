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

using System.Collections.Generic;

namespace Forge.Entities.Implementation.Runtime {
    /// <summary>
    /// A system execution group is a collection of ISystems that have dependencies in the order
    /// which they execute. For example, one system may depend on another system for data; it could
    /// be important that the first system is *always* processed before the second system. The
    /// SystemExecutionGroup makes those guarantees.
    /// </summary>
    internal class SystemExecutionGroup {
        /// <summary>
        /// The systems in this execution group.
        /// </summary>
        public List<ISystem> Systems;

        /// <summary>
        /// Construct a new execution group from the given systems.
        /// </summary>
        private SystemExecutionGroup(IEnumerable<ISystem> systems) {
            Systems = new List<ISystem>(systems);
        }

        /// <summary>
        /// Returns true if the given system can be executed completely independently from the other
        /// systems.
        /// </summary>
        /// <param name="system">The system to check.</param>
        /// <param name="systems">The systems to check against.</param>
        /// <returns>True if the system can be concurrently executed with the other
        /// systems.</returns>
        private static bool IsFullyConcurrent(ISystem system, List<ISystem> systems) {
            foreach (ISystem other in systems) {
                if (system.GetExecutionOrdering(other) != SystemExecutionOrdering.Concurrent) {
                    return false;
                }

                if (other.GetExecutionOrdering(system) != SystemExecutionOrdering.Concurrent) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Corrects the execution order of the given systems.
        /// </summary>
        private static void CorrectExecutionOrder(List<ISystem> systems) {
            systems.Sort((a, b) => {
                switch (a.GetExecutionOrdering(b)) {
                    case SystemExecutionOrdering.AfterOther:
                        return 1;
                    case SystemExecutionOrdering.BeforeOther:
                        return -1;
                    case SystemExecutionOrdering.Concurrent:
                    default:
                        return 0;
                }
            });

            systems.Sort((a, b) => {
                switch (b.GetExecutionOrdering(a)) {
                    case SystemExecutionOrdering.AfterOther:
                        return -1;
                    case SystemExecutionOrdering.BeforeOther:
                        return 1;
                    case SystemExecutionOrdering.Concurrent:
                    default:
                        return 0;
                }
            });
        }

        /// <summary>
        /// Returns the execution groups for the given systems.
        /// </summary>
        /// <param name="allSystems">Every system which is going to be executed.</param>
        /// <returns>The execution groups for the systems.</returns>
        public static List<SystemExecutionGroup> GetExecutionGroups(IEnumerable<ISystem> allSystems) {
            List<ISystem> systems = new List<ISystem>(allSystems);
            List<SystemExecutionGroup> groups = new List<SystemExecutionGroup>();

            while (systems.Count > 0) {
                List<ISystem> currentGroup = new List<ISystem>();

                // add an arbitrary item to the group; doesn't matter which one
                currentGroup.Add(systems[systems.Count - 1]);
                systems.RemoveAt(systems.Count - 1);

                // now we need to find all systems which depend on this one
                int i = 0;
                while (i < systems.Count) {
                    if (IsFullyConcurrent(systems[i], currentGroup) == false) {
                        currentGroup.Add(systems[i]);
                        systems.RemoveAt(i);
                        i = 0; // previously checked systems may not be concurrent w.r.t the new one
                    }
                    else {
                        ++i;
                    }
                }

                // we found every group that depends on the current group; we need to ensure that
                // they execute in the correct order
                CorrectExecutionOrder(currentGroup);

                groups.Add(new SystemExecutionGroup(currentGroup));
            }

            return groups;
        }
    }
}