using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    [JsonObject(MemberSerialization.OptIn)]
    internal class TestData0 : BaseData<TestData0> {
        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(TestData0 source) {
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class TestData1 : BaseData<TestData1> {
        [JsonProperty("A")]
        public int A;

        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(TestData1 source) {
            A = source.A;
        }
    }

}