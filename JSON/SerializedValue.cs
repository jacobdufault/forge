using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Neon.Serialization {
    public class SerializedValue {
        private void InsertSpacing(StringBuilder builder, int count) {
            for (int i = 0; i < count; ++i) {
                builder.Append("    ");
            }
        }

        public void BuildString(StringBuilder builder, int depth) {
            if (_value is bool) {
                bool b = (bool)_value;
                if (b) {
                    builder.Append("true");
                }
                else {
                    builder.Append("false");
                }
            }

            else if (_value is Real) {
                // We can convert the real to a float and export it that way, because upon import
                // all computers will parse the same string the same way.
                builder.Append(((Real)_value).AsFloat);
            }

            else if (_value is string) {
                // we don't support escaping
                builder.Append('"');
                builder.Append((string)_value);
                builder.Append('"');
            }

            else if (_value is Dictionary<string, SerializedValue>) {
                builder.Append('{');
                builder.AppendLine();
                foreach (var entry in AsDictionary) {
                    InsertSpacing(builder, depth + 1);
                    builder.Append(entry.Key);
                    builder.Append(": ");
                    entry.Value.BuildString(builder, depth + 1);
                    builder.AppendLine();
                }
                InsertSpacing(builder, depth);
                builder.Append('}');
            }

            else if (_value is List<SerializedValue>) {
                builder.Append('[');
                builder.AppendLine();
                foreach (var entry in AsList) {
                    InsertSpacing(builder, depth + 1);
                    entry.BuildString(builder, depth + 1);
                    builder.AppendLine();
                }
                InsertSpacing(builder, depth);
                builder.Append(']');
            }

            else {
                throw new NotImplementedException("Unknown stored value type of " + _value);
            }
        }

        public string AsString {
            get {
                StringBuilder builder = new StringBuilder();
                BuildString(builder, 0);
                return builder.ToString();
            }
        }

        private object _value;

        public SerializedValue(bool boolean) {
            _value = boolean;
        }

        public SerializedValue(Real real) {
            _value = real;
        }

        public SerializedValue(string str) {
            _value = str;
        }

        public SerializedValue(Dictionary<string, SerializedValue> dict) {
            _value = dict;
        }

        public SerializedValue(List<SerializedValue> list) {
            _value = list;
        }

        public static SerializedValue CreateDictionary() {
            return new Dictionary<string, SerializedValue>();
        }

        public static SerializedValue CreateList() {
            return new List<SerializedValue>();
        }

        public static implicit operator SerializedValue(bool boolean) {
            return new SerializedValue(boolean);
        }

        public static implicit operator SerializedValue(Real real) {
            return new SerializedValue(real);
        }

        public static implicit operator SerializedValue(string str) {
            return new SerializedValue(str);
        }

        public static implicit operator SerializedValue(List<SerializedValue> list) {
            return new SerializedValue(list);
        }

        public static implicit operator SerializedValue(Dictionary<string, SerializedValue> dict) {
            return new SerializedValue(dict);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IDictionary<string, SerializedValue> AsDictionary {
            get {
                return Cast<IDictionary<string, SerializedValue>>();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IList<SerializedValue> AsList {
            get {
                return Cast<IList<SerializedValue>>();
            }
        }

        public SerializedValue this[int index] {
            get {
                return AsList[index];
            }
            set {
                AsList[index] = value;
            }
        }

        public SerializedValue this[string key] {
            get {
                return AsDictionary[key];
            }
            set {
                AsDictionary[key] = value;
            }
        }

        public static implicit operator Real(SerializedValue value) {
            return value.Cast<Real>();
        }

        public static implicit operator string(SerializedValue value) {
            return value.Cast<string>();
        }

        public static implicit operator bool(SerializedValue value) {
            return value.Cast<bool>();
        }

        private T Cast<T>() {
            try {
                return (T)_value;
            }
            catch (InvalidCastException) {
                throw new Exception("Unable to cast <" + this + "> to type " + typeof(T));
            }
        }

        public override bool Equals(Object obj) {
            if (obj == null) {
                return false;
            }

            SerializedValue v = obj as SerializedValue;
            if (v == null) {
                return false;
            }

            return _value.Equals(v._value);
        }

        public bool Equals(SerializedValue v) {
            if (v == null) {
                return false;
            }

            return _value.Equals(v._value);
        }

        public static bool operator ==(SerializedValue a, SerializedValue b) {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(SerializedValue a, SerializedValue b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            return _value.GetHashCode();
        }
    }

}