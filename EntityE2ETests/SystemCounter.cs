using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    internal class SystemCounter : ITriggerUpdate, ITriggerRemoved {
        public int UpdateCount;
        public int RemovedCount;

        public Type[] Filter;

        public void OnUpdate(IEntity entity) {
            ++UpdateCount;
        }

        public Type[] ComputeEntityFilter() {
            return Filter;
        }

        public void OnRemoved(IEntity entity) {
            ++RemovedCount;
        }
    }
}