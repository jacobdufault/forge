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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Collections;
using Neon.Utilities;
using System;

namespace Neon.Serialization.Tests {
    internal class ClassWithPrivateMethods {
        private int A;

        public int GetA() {
            return A;
        }
        public void SetA(int a) {
            A = a;
        }

        private void Method0() {
        }
        protected virtual void Method1() {
        }
        protected internal virtual void Method2() {
        }
        public virtual void Method3() {
        }
    }

    internal class ClassWithPrivateField {
        private int A;

        public int GetA() {
            return A;
        }

        public void SetA(int value) {
            A = value;
        }
    }

    internal class PrivateBase {
        private int A;

        public int GetA() {
            return A;
        }

        public void SetA(int a) {
            A = a;
        }
    }

    internal class PrivateDerived : PrivateBase {
        private int B;

        public int GetB() {
            return B;
        }

        public void SetB(int b) {
            B = b;
        }
    }

    internal class DelegateType {
        public event Action EventDelegate;
        public Action FieldDelegate;
        public Action PropertyDelegate {
            get;
            set;
        }
    }

    internal class TypeWithNonSerialized {
        [NotSerializable]
        public int A;
    }

    [TestClass]
    public class TypeConversionPrivateTests {
        [TestMethod]
        public void ImportPrivateFields() {
            ClassWithPrivateField original = new ClassWithPrivateField();
            original.SetA(3);

            ClassWithPrivateField imported = ObjectSerializer.Import<ClassWithPrivateField>(ObjectSerializer.Export(original));

            Assert.AreEqual(original.GetA(), imported.GetA());
        }

        [TestMethod]
        public void InheritedPrivateFields() {
            PrivateDerived instance = new PrivateDerived();
            instance.SetA(33);
            instance.SetB(44);

            SerializedData exported = ObjectSerializer.Export(instance);
            PrivateDerived imported = ObjectSerializer.Import<PrivateDerived>(exported);

            Assert.AreEqual(instance.GetA(), imported.GetA());
            Assert.AreEqual(instance.GetB(), imported.GetB());
        }

        [TestMethod]
        public void PrivateSerializationWithInstanceMethods() {
            ClassWithPrivateMethods instance = new ClassWithPrivateMethods();
            instance.SetA(33);

            SerializedData exported = ObjectSerializer.Export(instance);
            ClassWithPrivateMethods imported = ObjectSerializer.Import<ClassWithPrivateMethods>(exported);

            Assert.AreEqual(instance.GetA(), imported.GetA());
        }

        [TestMethod]
        public void DontSerializeDelegate() {
            DelegateType instance = new DelegateType();
            instance.EventDelegate += () => { };
            instance.FieldDelegate = () => { };
            instance.PropertyDelegate = () => { };

            // if we're serializing a delegate, then this method will throw an exception
            SerializationHelpers.ImportExport(instance);
        }

        [TestMethod]
        public void TypeWithNonSerializedAttribute() {
            TypeWithNonSerialized t = new TypeWithNonSerialized() {
                A = 5
            };

            TypeWithNonSerialized imported = ObjectSerializer.Import<TypeWithNonSerialized>(ObjectSerializer.Export(t));
            Assert.AreEqual(default(int), imported.A);
        }

        [TestMethod]
        public void BagTest() {
            SerializedData data = ObjectSerializer.Export(new Bag<int>());
            ObjectSerializer.Import<Bag<int>>(data);
        }
    }
}