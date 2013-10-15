using System;
using System.Diagnostics;

namespace Neon.Utility {
    public class Contract {
        [Conditional("DEBUG")]
        public static void Requires(bool condition, string message = "") {
            if (condition == false) {
                throw new Exception(message);
            }
        }
    }
}