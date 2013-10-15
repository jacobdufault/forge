using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Utility {
    /// <summary>
    /// Container type that holds a reference to another object.
    /// </summary>
    /// <typeparam name="T">The type of object to store a reference to.</typeparam>
    public class Reference<T> {
        public T Value;

        public Reference() {
        }

        public Reference(T value) {
            Value = value;
        }
    }
}
