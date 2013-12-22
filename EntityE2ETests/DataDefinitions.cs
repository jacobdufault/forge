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

    [JsonObject(MemberSerialization.OptIn)]
    internal class TestData2 : BaseData<TestData2> {
        [JsonProperty("Template")]
        public ITemplate Template;

        public override void CopyFrom(TestData2 source) {
            Template = source.Template;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class TestData3 : BaseData<TestData3> {
        [JsonProperty("DataReference")]
        public DataReference<TestData0> DataReference;

        public override void CopyFrom(TestData3 source) {
            DataReference = source.DataReference;
        }
    }
}