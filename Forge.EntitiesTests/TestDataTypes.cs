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

namespace Forge.Entities.Tests {
    [JsonObject(MemberSerialization.OptIn)]
    internal class DataEmpty : BaseData<DataEmpty> {
        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(DataEmpty source) {
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class DataInt : BaseData<DataInt> {
        [JsonProperty("A")]
        public int A;

        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(DataInt source) {
            A = source.A;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class DataTemplate : BaseData<DataTemplate> {
        [JsonProperty("Template")]
        public ITemplate Template;

        public override void CopyFrom(DataTemplate source) {
            Template = source.Template;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class DataEntity : BaseData<DataEntity> {
        [JsonProperty("Entity")]
        public IEntity Entity;

        public override void CopyFrom(DataEntity source) {
            Entity = source.Entity;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class DataQueryableEntity : BaseData<DataQueryableEntity> {
        [JsonProperty("QueryableEntity")]
        public IQueryableEntity QueryableEntity;

        public override void CopyFrom(DataQueryableEntity source) {
            QueryableEntity = source.QueryableEntity;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class DataDataReference : BaseData<DataDataReference> {
        [JsonProperty("DataReference")]
        public DataReference<DataEmpty> DataReference;

        public override void CopyFrom(DataDataReference source) {
            DataReference = source.DataReference;
        }
    }
}