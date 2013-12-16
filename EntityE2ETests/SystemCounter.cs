using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    [ProtoContract]
    internal class SystemCounter : ITriggerUpdate, ITriggerRemoved {
        [ProtoMember(1)]
        public int UpdateCount;
        [ProtoMember(2)]
        public int RemovedCount;
        [ProtoMember(3)]
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