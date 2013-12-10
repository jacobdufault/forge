using System;
using System.Diagnostics;

namespace Neon.Utilities {
    public static class Contract {
        public static void Requires(bool condition, string message = "") {
            if (condition == false) {
                throw new ArgumentException(message);
            }
        }

        private static void Check<T>(string name, T item) {
            if (item == null) {
                throw new ArgumentNullException(name);
            }
        }

        [Conditional("DEBUG")]
        public static void AssertArguments<T0>(
            T0 param0, string nparam0
        ) {
            Check(nparam0, param0);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1>(
            T0 param0, string nparam0,
            T1 param1, string nparam1
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3, T4>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3,
            T4 param4, string nparam4
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
            Check(nparam4, param4);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3, T4, T5>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3,
            T4 param4, string nparam4,
            T5 param5, string nparam5
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
            Check(nparam4, param4);
            Check(nparam5, param5);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3, T4, T5, T6>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3,
            T4 param4, string nparam4,
            T5 param5, string nparam5,
            T6 param6, string nparam6
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
            Check(nparam4, param4);
            Check(nparam5, param5);
            Check(nparam6, param6);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3, T4, T5, T6, T7>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3,
            T4 param4, string nparam4,
            T5 param5, string nparam5,
            T6 param6, string nparam6,
            T7 param7, string nparam7
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
            Check(nparam4, param4);
            Check(nparam5, param5);
            Check(nparam6, param6);
            Check(nparam7, param7);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3,
            T4 param4, string nparam4,
            T5 param5, string nparam5,
            T6 param6, string nparam6,
            T7 param7, string nparam7,
            T8 param8, string nparam8
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
            Check(nparam4, param4);
            Check(nparam5, param5);
            Check(nparam6, param6);
            Check(nparam7, param7);
            Check(nparam8, param8);
        }
        [Conditional("DEBUG")]
        public static void AssertArguments<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            T0 param0, string nparam0,
            T1 param1, string nparam1,
            T2 param2, string nparam2,
            T3 param3, string nparam3,
            T4 param4, string nparam4,
            T5 param5, string nparam5,
            T6 param6, string nparam6,
            T7 param7, string nparam7,
            T8 param8, string nparam8,
            T9 param9, string nparam9
        ) {
            Check(nparam0, param0);
            Check(nparam1, param1);
            Check(nparam2, param2);
            Check(nparam3, param3);
            Check(nparam4, param4);
            Check(nparam5, param5);
            Check(nparam6, param6);
            Check(nparam7, param7);
            Check(nparam8, param8);
            Check(nparam9, param9);
        }
    }
}