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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Entities.E2ETests {
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