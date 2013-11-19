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

        private void Method0() {}
        protected virtual void Method1() {}
        internal protected virtual void Method2() { }
        public virtual void Method3() { }
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
        public int this[int index] {
            get {
                return 0;
            }
            set {
            }
        }
    }

    [TestClass]
    public class TypeConversionPrivateTests {
        [TestMethod]
        public void ImportPrivateFields() {
            SerializedData serialized = SerializedData.CreateDictionary();
            serialized.AsDictionary["A"] = new SerializedData(Real.CreateDecimal(3));

            SerializationConverter converter = new SerializationConverter();
            ClassWithPrivateField obj = converter.Import<ClassWithPrivateField>(serialized);

            Assert.AreEqual((Real)3, obj.GetA());
        }

        [TestMethod]
        public void InheritedPrivateFields() {
            PrivateDerived instance = new PrivateDerived();
            instance.SetA(33);
            instance.SetB(44);

            SerializedData exported = (new SerializationConverter()).Export(instance);
            PrivateDerived imported = (new SerializationConverter()).Import<PrivateDerived>(exported);

            Assert.AreEqual(instance.GetA(), imported.GetA());
            Assert.AreEqual(instance.GetB(), imported.GetB());
        }

        [TestMethod]
        public void PrivateSerializationWithInstanceMethods() {
            ClassWithPrivateMethods instance = new ClassWithPrivateMethods();
            instance.SetA(33);

            SerializedData exported = (new SerializationConverter()).Export(instance);
            ClassWithPrivateMethods imported = (new SerializationConverter()).Import<ClassWithPrivateMethods>(exported);

            Assert.AreEqual(instance.GetA(), imported.GetA());
        }

        [TestMethod]
        public void DontSerializeDelegate() {
            DelegateType instance = new DelegateType();
            instance.EventDelegate += () => { };
            instance.FieldDelegate = () => { };
            instance.PropertyDelegate = () => { };

            SerializedData exported = (new SerializationConverter()).Export(instance);

            Assert.IsTrue(exported.IsDictionary);
            Assert.AreEqual(0, exported.AsDictionary.Count);
        }

        [TestMethod]
        public void TypeWithNonSerializedAttribute() {
            (new SerializationConverter()).Export(new TypeWithNonSerialized());
            (new SerializationConverter()).Import<TypeWithNonSerialized>(SerializedData.CreateDictionary());
        }

        [TestMethod]
        public void BagTest() {
            SerializedData data = (new SerializationConverter()).Export(new Bag<int>());
            (new SerializationConverter()).Import<Bag<int>>(data);
        }
    }
}