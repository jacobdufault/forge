using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Utility {
    public struct Bound {
        public readonly Real X;
        public readonly Real Z;
        public readonly Real Radius;

        public Bound(Real x, Real z, Real radius) {
            Contract.Requires(radius > 0);

            X = x;
            Z = z;
            Radius = radius;
        }

        /// <summary>
        /// Returns true if this bound intersects with the other bound.
        /// </summary>
        public bool Intersects(Bound other) {
            // shortcut aliases
            Real r0 = Radius, r1 = other.Radius;
            Real x0 = X, x1 = other.X;
            Real z0 = Z, z1 = other.Z;

            //                min               inner               max
            // equation is (R0-R1)^2 <= (x0-x1)^2 + (y0-y1)^2 <= (R0+R1)^2

            Real min = (r0 - r1);
            min *= min;

            Real dx = x0 - x1;
            Real dz = z0 - z1;
            Real inner = dx * dx + dz * dz;

            Real max = (r0 + r1);
            max *= max;

            return min <= inner && inner <= max;
        }

        public override string ToString() {
            return string.Format("Bound [X={0}, Z={1}, Radius={2}]", X, Z, Radius);
        }
    }
}
