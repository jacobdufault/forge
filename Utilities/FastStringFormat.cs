using Neon.Utility;
using System;
using System.Text;

namespace Neon.Utility {
    /// <summary>
    /// Provides methods which format a string without garbage allocation.
    /// </summary>
    /// <remarks>
    /// The format strings only go from {0} to {9} and do not support any customizations. Backslashes
    /// are also not supported.
    /// </remarks>
    public static class FastStringFormat {
        private static StringBuilder _stringBuilder = new StringBuilder();

        public static string Format<T0>(string format, T0 arg0) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1>(string format, T0 arg0, T1 arg1) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else if (n == '4') _stringBuilder.Append(arg4);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else if (n == '4') _stringBuilder.Append(arg4);
                        else if (n == '5') _stringBuilder.Append(arg5);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else if (n == '4') _stringBuilder.Append(arg4);
                        else if (n == '5') _stringBuilder.Append(arg5);
                        else if (n == '6') _stringBuilder.Append(arg6);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else if (n == '4') _stringBuilder.Append(arg4);
                        else if (n == '5') _stringBuilder.Append(arg5);
                        else if (n == '6') _stringBuilder.Append(arg6);
                        else if (n == '7') _stringBuilder.Append(arg7);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else if (n == '4') _stringBuilder.Append(arg4);
                        else if (n == '5') _stringBuilder.Append(arg5);
                        else if (n == '6') _stringBuilder.Append(arg6);
                        else if (n == '7') _stringBuilder.Append(arg7);
                        else if (n == '8') _stringBuilder.Append(arg8);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }

        public static string Format<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            lock (_stringBuilder) {
                _stringBuilder.Length = 0;

                int length = format.Length;
                int start = 0, current = 0;

                while (current < length) {
                    char c = format[current];

                    if (c == '{') {
                        Contract.Requires(format[current + 2] == '}', "Character after replacement group must be a }");
                        if (start != current) _stringBuilder.Append(format, start, current - start);

                        char n = format[current + 1];
                        // variable code
                        if (n == '0') _stringBuilder.Append(arg0);
                        else if (n == '1') _stringBuilder.Append(arg1);
                        else if (n == '2') _stringBuilder.Append(arg2);
                        else if (n == '3') _stringBuilder.Append(arg3);
                        else if (n == '4') _stringBuilder.Append(arg4);
                        else if (n == '5') _stringBuilder.Append(arg5);
                        else if (n == '6') _stringBuilder.Append(arg6);
                        else if (n == '7') _stringBuilder.Append(arg7);
                        else if (n == '8') _stringBuilder.Append(arg8);
                        else if (n == '9') _stringBuilder.Append(arg9);
                        else throw new ArgumentException("Invalid replacement group in format string " + format);

                        current += 2;
                        start = current + 1;
                    }

                    ++current;
                }

                if (start != current) _stringBuilder.Append(format, start, length - start);
                return _stringBuilder.ToString();
            }
        }
    }
}
