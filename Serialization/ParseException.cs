using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    public class ParseException : Exception {
        private static string CreateMessage(string message, Parser context) {
            int start = Math.Max(0, context._start - 10);
            int length = Math.Min(20, context._json.Length - start);

            return "Error while parsing : " + message + "; context = \"" +
                context._json.Substring(start, length) + "\"";
        }

        public ParseException(string message, Parser context)
            : base(CreateMessage(message, context)) {
        }
    }
}
