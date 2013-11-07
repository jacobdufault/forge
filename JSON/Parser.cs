using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    public class Parser {
        internal string _json;
        internal int _start;

        private char CurrentCharacter(int offset = 0) {
            return _json[_start + offset];
        }

        private void MoveNext() {
            ++_start;

            if (_start > _json.Length) {
                throw new ParseException("Unexpected end of input", this);
            }
        }

        private bool HasNext() {
            return _start < _json.Length - 1;
        }

        private bool HasValue(int offset = 0) {
            return (_start + offset) >= 0 && (_start + offset) < _json.Length;
        }

        private void SkipSpace() {
            while (HasValue() && char.IsWhiteSpace(CurrentCharacter())) {
                MoveNext();
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

        SerializedValue ParseTrue() {
            if (CurrentCharacter() != 't') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'r') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'u') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'e') throw new ParseException("expected true", this);
            MoveNext();

            return true;
        }

        SerializedValue ParseFalse() {
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

            return false;
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
        SerializedValue ParseNumber() {
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

            long leftValue = ParseSubstring(_json, start, end);
            if (negative) {
                leftValue *= -1;
            }

            // if there is no period, then we don't have a decimal, number is of format [-]\d*
            if ((HasValue() && CurrentCharacter() == '.') == false) {
                return new SerializedValue(Real.CreateDecimal(leftValue));
            }

            // we have a period, so the number is of the format [-]\d*.\d*
            MoveNext();
            start = _start;
            while (HasValue() && char.IsNumber(CurrentCharacter())) {
                MoveNext();
            }
            end = _start;

            int rightValue = (int)ParseSubstring(_json, start, end);
            return new SerializedValue(Real.CreateDecimal(leftValue, rightValue));
        }

        string ParseKey() {
            StringBuilder result = new StringBuilder();

            while (CurrentCharacter() != ':') {
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

            return result.ToString();
        }

        SerializedValue ParseString() {
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

            return result.ToString();
        }


        SerializedValue ParseArray() {
            // skip '['
            MoveNext();
            SkipSpace();

            List<SerializedValue> result = new List<SerializedValue>();

            while (CurrentCharacter() != ']') {
                SerializedValue element = RunParse();
                result.Add(element);

                SkipSpace();
            }

            // skip ']'
            MoveNext();

            return result;
        }



        SerializedValue ParseObject() {
            // skip '{'
            SkipSpace();
            MoveNext();
            SkipSpace();

            Dictionary<string, SerializedValue> result = new Dictionary<string, SerializedValue>();

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

                SerializedValue value = RunParse();
                result.Add(key, value);

                SkipSpace();
            }

            /* skip '}' */
            MoveNext();
            return result;
        }


        public static SerializedValue Parse(string json) {
            Parser context = new Parser(json);
            return context.RunParse();
        }

        private Parser(string json) {
            _json = json;
            _start = 0;
        }

        SerializedValue RunParse() {
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
                case '"': return ParseString();
                case '[': return ParseArray();
                case '{': return ParseObject();
                case 't': return ParseTrue();
                case 'f': return ParseFalse();
                default: throw new ParseException("unable to parse", this);
            }
        }

    }


}
