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
using System.Collections;
using System.Collections.Generic;

namespace Forge.Utilities {
    [JsonObject(MemberSerialization.OptIn)]
    public struct Bound {
        [JsonProperty("X")]
        public readonly Real X;
        [JsonProperty("Z")]
        public readonly Real Z;
        [JsonProperty("Radius")]
        public readonly Real Radius;

        public Bound(Real x, Real z, Real radius) {
            Contract.Requires(radius > 0, "Radius must be > 0");

            X = x;
            Z = z;
            Radius = radius;
        }

        /// <summary>
        /// Returns true if this bound is either intersecting or colliding with the other bound.
        /// </summary>
        public bool Intersects(Bound other) {
            // TODO: optimize this function to avoid the sqrt
            Real centerDistances = Vector2r.Distance(X, Z, other.X, other.Z);
            Real range = Radius + other.Radius;

            return range >= centerDistances;
        }

        /// <summary>
        /// Returns true if the given point is contained within this bound.
        /// </summary>
        public bool Contains(Vector2r point) {
            return Contains(point.X, point.Z);
        }

        /// <summary>
        /// Returns true if the given point is contained within this bound.
        /// </summary>
        public bool Contains(Real x, Real z) {
            Real distanceSq = Vector2r.DistanceSq(X, Z, x, z);
            return (Radius * Radius) > distanceSq;
        }

        public override string ToString() {
            return string.Format("Bound [X={0}, Z={1}, Radius={2}]", X, Z, Radius);
        }
    }
}