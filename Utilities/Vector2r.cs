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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Utilities {
    [Serializable]
    public struct Vector2r {
        public Real X;
        public Real Z;

        public Vector2r(Real x, Real z) {
            X = x;
            Z = z;
        }

        public override string ToString() {
            return string.Format("Vector2r [x={0}, z={1}]", X, Z);
        }

        public Real Length() {
            return Real.Sqrt(X * X + Z * Z);
        }

        public void Normalize() {
            Real length = Length();
            if (length == 0) {
                return;
            }

            X /= length;
            Z /= length;
        }

        public static Real Distance(Vector2r a, Vector2r b) {
            return Distance(a.X, a.Z, b.X, b.Z);
        }

        public static Real Distance(Real x0, Real z0, Real x1, Real z1) {
            var dx = x0 - x1;
            var dz = z0 - z1;

            return Real.Sqrt(dx * dx + dz * dz);
        }

        public static Real DistanceSq(Vector2r a, Vector2r b) {
            return DistanceSq(a.X, a.Z, b.X, b.Z);
        }

        public static Real DistanceSq(Real x0, Real z0, Real x1, Real z1) {
            var dx = x0 - x1;
            var dz = z0 - z1;

            return dx * dx + dz * dz;
        }

        public static Vector2r operator -(Vector2r a, Vector2r b) {
            return new Vector2r() {
                X = a.X - b.X,
                Z = a.Z - b.Z
            };
        }

        public static Vector2r operator +(Vector2r a, Vector2r b) {
            return new Vector2r() {
                X = a.X + b.X,
                Z = a.Z + b.Z
            };
        }

        public static Vector2r operator *(Vector2r a, Real v) {
            return new Vector2r() {
                X = a.X * v,
                Z = a.Z * v
            };
        }

        public override bool Equals(System.Object obj) {
            // If parameter is null return false.
            if (obj == null) {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Vector2r p = (Vector2r)obj;
            if ((System.Object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return X == p.X && Z == p.Z;
        }

        public bool Equals(Vector2r p) {
            // If parameter is null return false:
            if ((object)p == null) {
                return false;
            }

            // Return true if the fields match:
            return (X == p.X) && (Z == p.Z);
        }

        public override int GetHashCode() {
            int hash = 5;
            hash *= (29 + X.GetHashCode());
            hash *= (29 + Z.GetHashCode());
            return hash;
        }

        public static bool operator ==(Vector2r a, Vector2r b) {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.X == b.X && a.Z == b.Z;
        }

        public static bool operator !=(Vector2r a, Vector2r b) {
            return !(a == b);
        }
    }
}