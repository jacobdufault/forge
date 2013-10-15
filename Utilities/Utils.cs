using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Utility {
    public static class Utils {
        /// <summary>
        /// Swaps two ref objects.
        /// </summary>
        /// <typeparam name="T">The type of objects to swap.</typeparam>
        /// <param name="a">Object a</param>
        /// <param name="b">Object b</param>
        public static void Swap<T>(ref T a, ref T b) {
            T tmp;
            tmp = a;
            a = b;
            b = tmp;
        }
    }
}
