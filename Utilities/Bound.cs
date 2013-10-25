using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Utility {
    public struct Bound {
        public readonly Real X;
        public readonly Real Z;
        public readonly Real Radius;
        private readonly Real RadiusSq;

        public Bound(Real x, Real z, Real radius) {
            Contract.Requires(radius > 0);

            X = x;
            Z = z;
            Radius = radius;
            RadiusSq = radius * radius;
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
            return RadiusSq > distanceSq;
        }

        public override string ToString() {
            return string.Format("Bound [X={0}, Z={1}, Radius={2}]", X, Z, Radius);
        }
    }
}
