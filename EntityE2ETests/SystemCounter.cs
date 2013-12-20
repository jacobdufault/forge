using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    [JsonObject(MemberSerialization.OptIn)]
    internal class SystemCounter : ITriggerUpdate, ITriggerRemoved {
        [JsonProperty("UpdateCount")]
        public int UpdateCount;
        [JsonProperty("RemovedCount")]
        public int RemovedCount;
        [JsonProperty("Filter")]
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