using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    /// <summary>
    /// Parses serialized data into instances of SerializedData.
    /// </summary>
    public class Parser {
        internal string _input;
        internal int _start;

        private char CurrentCharacter(int offset = 0) {
            return _input[_start + offset];
        }

        private void MoveNext() {
            ++_start;

            if (_start > _input.Length) {
                throw new ParseException("Unexpected end of input", this);
            }
        }

        private bool HasNext() {
            return _start < _input.Length - 1;
        }

        private bool HasValue(int offset = 0) {
            return (_start + offset) >= 0 && (_start + offset) < _input.Length;
        }

        private void SkipSpace() {
            while (HasValue()) {
                char c = CurrentCharacter();

                if (char.IsWhiteSpace(c)) {
                    MoveNext();
                }
                else if (c == '#') {
                    while (HasValue() && Environment.NewLine.Contains(CurrentCharacter()) == false) {
                        MoveNext();
                    }
                }
                else {
                    break;
                }
            }
        }

        private bool IsHex(char c) {
            return ((c >= '0' && c <= '9') ||
                     (c >= 'a' && c <= 'f') ||
                     (c >= 'A' && c <= 'F'));
        }

        private uint ParseSingleChar(char c1, uint multipliyer) {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            return p1;
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4) {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private char UnescapeChar() {
            /* skip leading backslash '\' */
            switch (CurrentCharacter()) {
                case '\\': MoveNext(); return '\\';
                case '/': MoveNext(); return '/';
                case '\'': MoveNext(); return '\'';
                case '"': MoveNext(); return '\"';
                case 'a': MoveNext(); return '\a';
                case 'b': MoveNext(); return '\b';
                case 'f': MoveNext(); return '\f';
                case 'n': MoveNext(); return '\n';
                case 'r': MoveNext(); return '\r';
                case 't': MoveNext(); return '\t';
                case '0': MoveNext(); return '\0';
                case 'u':
                    MoveNext();
                    if (IsHex(CurrentCharacter(0))
                     && IsHex(CurrentCharacter(1))
                     && IsHex(CurrentCharacter(2))
                     && IsHex(CurrentCharacter(3))) {
                        MoveNext();
                        MoveNext();
                        MoveNext();
                        MoveNext();
                        uint codePoint = ParseUnicode(CurrentCharacter(0), CurrentCharacter(1), CurrentCharacter(2), CurrentCharacter(3));
                        return (char)codePoint;
                    }

                    /* invalid hex escape sequence */
                    throw new ParseException(string.Format("invalid escape sequence '\\u{0}{1}{2}{3}'\n",
                            CurrentCharacter(0),
                            CurrentCharacter(1),
                            CurrentCharacter(2),
                            CurrentCharacter(3)), this);
                default:
                    throw new ParseException(string.Format("Invalid escape sequence \\{0}", CurrentCharacter()), this);
            }
        }

        private SerializedData ParseTrue() {
            if (CurrentCharacter() != 't') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'r') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'u') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'e') throw new ParseException("expected true", this);
            MoveNext();

            return new SerializedData(true);
        }

        private SerializedData ParseFalse() {
            if (CurrentCharacter() != 'f') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 'a') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 'l') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 's') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 'e') throw new ParseException("expected false", this);
            MoveNext();

            return new SerializedData(false);
        }

        private SerializedData ParseNull() {
            if (CurrentCharacter() != 'n') throw new ParseException("expected null", this);
            MoveNext();
            if (CurrentCharacter() != 'u') throw new ParseException("expected null", this);
            MoveNext();
            if (CurrentCharacter() != 'l') throw new ParseException("expected null", this);
            MoveNext();
            if (CurrentCharacter() != 'l') throw new ParseException("expected null", this);
            MoveNext();

            return new SerializedData();
        }

        private long ParseSubstring(string baseString, int start, int end) {
            if (start == end) {
                return 0;
            }

            return long.Parse(baseString.Substring(start, end - start));
        }

        /// <summary>
        /// Parses numbers that follow the regular expression [-+](\d+|\d*\.\d*)
        /// </summary>
        /// <returns></returns>
        private SerializedData ParseNumber() {
            // determine if the result should be negative
            bool negative = false;
            if (CurrentCharacter() == '-' || CurrentCharacter() == '+') {
                negative = CurrentCharacter() == '-';
                MoveNext();
            }

            // parse the before decimal portion of the number
            int start = _start;
            while (HasValue() && char.IsNumber(CurrentCharacter())) {
                MoveNext();
            }
            int end = _start;

            long leftValue = ParseSubstring(_input, start, end);
            if (negative) {
                leftValue *= -1;
            }

            // if there is no period, then we don't have a decimal, number is of format [-]\d*
            if ((HasValue() && CurrentCharacter() == '.') == false) {
                return new SerializedData(Real.CreateDecimal(leftValue));
            }

            // we have a period, so the number is of the format [-]\d*.\d*
            MoveNext();
            start = _start;
            while (HasValue() && char.IsNumber(CurrentCharacter())) {
                MoveNext();
            }
            end = _start;

            int rightValue = (int)ParseSubstring(_input, start, end);
            return new SerializedData(Real.CreateDecimal(leftValue, rightValue, end - start));
        }

        private int ParsePositiveInt() {
            int start = _start;

            if (HasValue() == false || char.IsNumber(CurrentCharacter()) == false) {
                throw new ParseException("Attempt to parse positive int failed; no integer", this);
            }

            while (HasValue() && char.IsNumber(CurrentCharacter())) {
                MoveNext();
            }

            return Int32.Parse(_input.Substring(start, _start - start));
        }

        private string ParseKey() {
            StringBuilder result = new StringBuilder();

            while (CurrentCharacter() != ':' && CurrentCharacter() != '`' &&
                char.IsWhiteSpace(CurrentCharacter()) == false) {
                char c = CurrentCharacter();

                if (c == '\\') {
                    char unescaped = UnescapeChar();
                    result.Append(unescaped);
                }
                else {
                    result.Append(c);
                }

                MoveNext();
            }

            SkipSpace();

            return result.ToString();
        }

        private SerializedData ParseString() {
            if (CurrentCharacter() != '"') {
                throw new ParseException("Attempt to parse string without leading \"", this);
            }

            // skip '"'
            MoveNext();

            StringBuilder result = new StringBuilder();

            while (CurrentCharacter() != '"') {
                char c = CurrentCharacter();

                if (c == '\\') {
                    char unescaped = UnescapeChar();
                    result.Append(unescaped);
                }
                else {
                    result.Append(c);
                }

                MoveNext();
            }

            // skip '"'
            MoveNext();

            return new SerializedData(result.ToString());
        }

        private SerializedData ParseArray() {
            // skip '['
            MoveNext();
            SkipSpace();

            List<SerializedData> result = new List<SerializedData>();

            while (CurrentCharacter() != ']') {
                SerializedData element = RunParse();
                result.Add(element);

                SkipSpace();
            }

            // skip ']'
            MoveNext();

            return new SerializedData(result);
        }

        private SerializedData ParseObject() {
            // skip '{'
            SkipSpace();
            MoveNext();
            SkipSpace();

            Dictionary<string, SerializedData> result = new Dictionary<string, SerializedData>();

            while (CurrentCharacter() != '}') {
                SkipSpace();
                string key = ParseKey();
                SkipSpace();

                if (CurrentCharacter() != ':') {
                    throw new ParseException("Expected : after object key " + key, this);
                }

                // skip ':'
                MoveNext();
                SkipSpace();

                SerializedData value = RunParse();
                result.Add(key, value);

                SkipSpace();
            }

            /* skip '}' */
            MoveNext();
            return new SerializedData(result);
        }

        private SerializedData ParseObjectDefinition() {
            // skip the `
            MoveNext();

            int objectId = ParsePositiveInt();

            SerializedData obj = RunParse();
            obj.SetObjectDefinition(objectId);
            return obj;
        }

        private SerializedData ParseObjectReference() {
            if (CurrentCharacter() != '~') {
                throw new ParseException("Expected object reference; failed", this);
            }

            // skip the ~
            MoveNext();

            int objectref = ParsePositiveInt();

            return SerializedData.CreateObjectReference(objectref);
        }

        /// <summary>
        /// Parses the specified input. Throws a ParseException if parsing failed.
        /// </summary>
        /// <param name="input">The input to parse.</param>
        /// <returns>The parsed input.</returns>
        public static SerializedData Parse(string input) {
            Parser context = new Parser(input);
            return context.RunParse();
        }

        private Parser(string input) {
            _input = input;
            _start = 0;
        }

        private SerializedData RunParse() {
            SkipSpace();

            switch (CurrentCharacter()) {
                case '.':
                case '+':
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return ParseNumber();
                case '~': return ParseObjectReference();
                case '`': return ParseObjectDefinition();
                case '"': return ParseString();
                case '[': return ParseArray();
                case '{': return ParseObject();
                case 't': return ParseTrue();
                case 'f': return ParseFalse();
                case 'n': return ParseNull();
                default: throw new ParseException("unable to parse", this);
            }
        }

    }

}