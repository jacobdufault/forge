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

// This file contains some exception definitions that Neon.Serialization uses.

namespace Neon.Serialization {
    /// <summary>
    /// Exception thrown when a parsing error has occurred.
    /// </summary>
    public sealed class ParseException : Exception {
        /// <summary>
        /// Helper method to create a parsing exception message
        /// </summary>
        private static string CreateMessage(string message, Parser context) {
            int start = Math.Max(0, context._start - 10);
            int length = Math.Min(20, context._input.Length - start);

            return "Error while parsing: " + message + "; context = \"" +
                context._input.Substring(start, length) + "\"";
        }

        /// <summary>
        /// Initializes a new instance of the ParseException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="context">The context that the error occurred</param>
        public ParseException(string message, Parser context)
            : base(CreateMessage(message, context)) {
        }
    }
}