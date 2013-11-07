using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Neon.Serialization {
    public class SerializedData {
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

            else if (_value is Dictionary<string, SerializedData>) {
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

            else if (_value is List<SerializedData>) {
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

        public SerializedData(bool boolean) {
            _value = boolean;
        }

        public SerializedData(Real real) {
            _value = real;
        }

        public SerializedData(string str) {
            _value = str;
        }

        public SerializedData(Dictionary<string, SerializedData> dict) {
            _value = dict;
        }

        public SerializedData(List<SerializedData> list) {
            _value = list;
        }

        public static SerializedData CreateDictionary() {
            return new Dictionary<string, SerializedData>();
        }

        public static SerializedData CreateList() {
            return new List<SerializedData>();
        }

        public static implicit operator SerializedData(bool boolean) {
            return new SerializedData(boolean);
        }

        public static implicit operator SerializedData(Real real) {
            return new SerializedData(real);
        }

        public static implicit operator SerializedData(string str) {
            return new SerializedData(str);
        }

        public static implicit operator SerializedData(List<SerializedData> list) {
            return new SerializedData(list);
        }

        public static implicit operator SerializedData(Dictionary<string, SerializedData> dict) {
            return new SerializedData(dict);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IDictionary<string, SerializedData> AsDictionary {
            get {
                return Cast<IDictionary<string, SerializedData>>();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IList<SerializedData> AsList {
            get {
                return Cast<IList<SerializedData>>();
            }
        }

        public SerializedData this[int index] {
            get {
                return AsList[index];
            }
            set {
                AsList[index] = value;
            }
        }

        public SerializedData this[string key] {
            get {
                return AsDictionary[key];
            }
            set {
                AsDictionary[key] = value;
            }
        }

        public static implicit operator Real(SerializedData value) {
            return value.Cast<Real>();
        }

        public static implicit operator string(SerializedData value) {
            return value.Cast<string>();
        }

        public static implicit operator bool(SerializedData value) {
            return value.Cast<bool>();
        }

        private T Cast<T>() {
            if (_value is T) {
                return (T)_value;
            }

            throw new Exception("Unable to cast <" + AsString + "> to type " + typeof(T));
        }

        public override bool Equals(Object obj) {
            if (obj == null) {
                return false;
            }

            SerializedData v = obj as SerializedData;
            if (v == null) {
                return false;
            }

            return _value.Equals(v._value);
        }

        public bool Equals(SerializedData v) {
            if (v == null) {
                return false;
            }

            return _value.Equals(v._value);
        }

        public static bool operator ==(SerializedData a, SerializedData b) {
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

        public static bool operator !=(SerializedData a, SerializedData b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            return _value.GetHashCode();
        }
    }

}