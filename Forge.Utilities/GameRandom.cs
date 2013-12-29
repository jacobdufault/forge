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

namespace Forge.Utilities {
    public static class GameRandom {
        private static System.Random _random = new System.Random(32352);

        public static Real Range(Real min, Real max) {
            return Real.CreateFromRaw(_random.NextLong(min.RawValue, max.RawValue));
        }

        public static int Range(int min, int max) {
            return _random.Next(min, max);
        }
    }

    // from http://stackoverflow.com/a/13095144
    public static class RandomExtensionMethods {
        /// <summary>
        /// Returns a random long from min (inclusive) to max (exclusive)
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="min">The inclusive minimum bound</param>
        /// <param name="max">The exclusive maximum bound. Must be greater than min</param>
        public static long NextLong(this System.Random random, long min, long max) {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            // Prevent a modulo bias; see http://stackoverflow.com/a/10984975/238419 for more
            // information. In the worst case, the expected number of calls is 2 (though usually
            // it's much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        /// <summary>
        /// Returns a random long from 0 (inclusive) to max (exclusive)
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="max">The exclusive maximum bound. Must be greater than 0</param>
        public static long NextLong(this System.Random random, long max) {
            return random.NextLong(0, max);
        }

        /// <summary>
        /// Returns a random long over all possible values of long (except long.MaxValue, similar to
        /// random.Next())
        /// </summary>
        /// <param name="random">The given random instance</param>
        public static long NextLong(this System.Random random) {
            return random.NextLong(long.MinValue, long.MaxValue);
        }
    }
}