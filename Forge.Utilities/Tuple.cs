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

namespace Forge.Utilities {
    [JsonObject(MemberSerialization.OptIn)]
    public class Tuple<T1> {
        public Tuple(T1 item1) {
            Item1 = item1;
        }

        [JsonProperty("Item1")]
        public T1 Item1 { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Tuple<T1, T2> : Tuple<T1> {
        public Tuple(T1 item1, T2 item2)
            : base(item1) {
            Item2 = item2;
        }

        [JsonProperty("Item2")]
        public T2 Item2 { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Tuple<T1, T2, T3> : Tuple<T1, T2> {
        public Tuple(T1 item1, T2 item2, T3 item3)
            : base(item1, item2) {
            Item3 = item3;
        }

        [JsonProperty("Item3")]
        public T3 Item3 { get; set; }
    }

    public static class Tuple {
        public static Tuple<T1> Create<T1>(T1 item1) {
            return new Tuple<T1>(item1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
    }
}