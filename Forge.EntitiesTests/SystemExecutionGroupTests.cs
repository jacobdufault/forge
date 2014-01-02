using Forge.Entities.Implementation.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Forge.Entities.Tests {
    public class SystemExecutionGroupTests {
        private class SystemConcurrent : BaseSystem {
            protected override SystemExecutionOrdering GetExecutionOrdering(ISystem system) {
                return SystemExecutionOrdering.Concurrent;
            }
        }

        private class SystemBeforeA : BaseSystem {
            protected override SystemExecutionOrdering GetExecutionOrdering(ISystem system) {
                if (system is SystemA) {
                    return SystemExecutionOrdering.BeforeOther;
                }

                return SystemExecutionOrdering.Concurrent;
            }
        }

        private class SystemA : BaseSystem {
            protected override SystemExecutionOrdering GetExecutionOrdering(ISystem system) {
                if (system is SystemBeforeA) {
                    return SystemExecutionOrdering.AfterOther;
                }

                if (system is SystemAfterA) {
                    return SystemExecutionOrdering.BeforeOther;
                }

                return SystemExecutionOrdering.Concurrent;
            }
        }

        private class SystemAfterA : BaseSystem {
            protected override SystemExecutionOrdering GetExecutionOrdering(ISystem system) {
                if (system is SystemA) {
                    return SystemExecutionOrdering.AfterOther;
                }

                return SystemExecutionOrdering.Concurrent;
            }
        }

        public static IEnumerable<object[]> AllConcurrentSystemsData {
            get {
                List<object[]> items = new List<object[]>() {
                    new object[] { new SystemConcurrent(), new SystemConcurrent(), new SystemConcurrent() },
                    new object[] { new SystemA(), new SystemA(), new SystemA() },
                    new object[] { new SystemConcurrent(), new SystemA(), new SystemA() },
                    new object[] { new SystemA(), new SystemConcurrent(), new SystemA() },
                    new object[] { new SystemA(), new SystemA(), new SystemConcurrent() }
                };
                return items;
            }
        }

        [Theory]
        [PropertyData("AllConcurrentSystemsData")]
        public void AllConcurrentSystems(ISystem systemA, ISystem systemB, ISystem systemC) {
            List<SystemExecutionGroup> groups = SystemExecutionGroup.GetExecutionGroups(new List<ISystem>() {
                systemA, systemB, systemC
            });
            Assert.Equal(3, groups.Count);
            Assert.Equal(1, groups[0].Systems.Count);
            Assert.Equal(1, groups[1].Systems.Count);
            Assert.Equal(1, groups[2].Systems.Count);
        }

        public static IEnumerable<object[]> SystemOrderingData {
            get {
                List<object[]> items = new List<object[]>() {
                    // permute SystemA
                    new object[] { new SystemA(), new SystemBeforeA(), new SystemAfterA(), new SystemConcurrent() },
                    new object[] { new SystemBeforeA(), new SystemA(), new SystemAfterA(), new SystemConcurrent() },
                    new object[] { new SystemBeforeA(), new SystemAfterA(), new SystemA(), new SystemConcurrent() },
                    new object[] { new SystemBeforeA(), new SystemAfterA(), new SystemConcurrent(), new SystemA() },

                    // permute SystemBeforeA
                    new object[] { new SystemBeforeA(), new SystemA(), new SystemAfterA(), new SystemConcurrent() },
                    new object[] { new SystemA(), new SystemBeforeA(), new SystemAfterA(), new SystemConcurrent() },
                    new object[] { new SystemA(), new SystemAfterA(), new SystemBeforeA(), new SystemConcurrent() },
                    new object[] { new SystemA(), new SystemAfterA(), new SystemConcurrent(), new SystemBeforeA() },

                    // permute SystemAfterA
                    new object[] { new SystemAfterA(), new SystemBeforeA(), new SystemA(), new SystemConcurrent() },
                    new object[] { new SystemBeforeA(), new SystemAfterA(), new SystemA(), new SystemConcurrent() },
                    new object[] { new SystemBeforeA(), new SystemA(), new SystemAfterA(), new SystemConcurrent() },
                    new object[] { new SystemBeforeA(), new SystemA(), new SystemConcurrent(), new SystemAfterA() },

                    // permute SystemConcurrent
                    new object[] { new SystemConcurrent(), new SystemBeforeA(), new SystemA(), new SystemAfterA() },
                    new object[] { new SystemBeforeA(), new SystemConcurrent(), new SystemA(), new SystemAfterA() },
                    new object[] { new SystemBeforeA(), new SystemA(), new SystemConcurrent(), new SystemAfterA() },
                    new object[] { new SystemBeforeA(), new SystemA(), new SystemAfterA(), new SystemConcurrent() }
                };
                return items;
            }
        }

        [Theory]
        [PropertyData("SystemOrderingData")]
        public void SystemOrdering(ISystem system0, ISystem system1, ISystem system2, ISystem system3) {
            List<SystemExecutionGroup> groups = SystemExecutionGroup.GetExecutionGroups(new List<ISystem>() {
                system0, system1, system2, system3
            });
            Assert.Equal(2, groups.Count);

            SystemExecutionGroup singleGroup;
            SystemExecutionGroup bigGroup;

            if (groups[0].Systems.Count == 1) {
                singleGroup = groups[0];
                bigGroup = groups[1];
            }
            else {
                singleGroup = groups[1];
                bigGroup = groups[0];
            }

            Assert.Equal(1, singleGroup.Systems.Count);
            Assert.IsType<SystemConcurrent>(singleGroup.Systems[0]);

            Assert.Equal(3, bigGroup.Systems.Count);
            Assert.IsType<SystemBeforeA>(bigGroup.Systems[0]);
            Assert.IsType<SystemA>(bigGroup.Systems[1]);
            Assert.IsType<SystemAfterA>(bigGroup.Systems[2]);
        }
    }
}