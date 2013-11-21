using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Serialization.Tests {
    internal enum MyEnum {
        MyEnum0,
        MyEnum1,
        MyEnum2,
        MyEnum3,
        MyEnum4
    }

    internal class EnumContainer {
        public MyEnum EnumA;
        public MyEnum EnumB;

        public override bool Equals(object obj) {
            if (obj is EnumContainer == false) return false;
            EnumContainer ec = (EnumContainer)obj;
            return EnumA == ec.EnumA && EnumB == ec.EnumB;
        }
    }

    internal interface IInterface { }
    internal class DerivedInterfaceA : IInterface {
        public override bool Equals(object obj) {
            return obj is DerivedInterfaceA;
        }
    }
    internal class DerivedInterfaceB : IInterface {
        public override bool Equals(object obj) {
            return obj is DerivedInterfaceB;
        }
    }

    internal abstract class AbstractClass {
        public string A;
    }
    internal class DerivedAbstractClassA : AbstractClass {
        public string B;
        public override bool Equals(object obj) {
            return obj is DerivedAbstractClassA &&
                ((DerivedAbstractClassA)obj).A == A && ((DerivedAbstractClassA)obj).B == B;
        }
    }
    internal class DerivedAbstractClassB : AbstractClass {
        public string C;
        public override bool Equals(object obj) {
            return obj is DerivedAbstractClassB &&
                ((DerivedAbstractClassB)obj).A == A && ((DerivedAbstractClassB)obj).C == C;
        }
    }

    [SerializationSupportInheritance]
    internal class BaseClassWithInheritance {
        public string A;
    }
    internal class DerivedBaseClassA : BaseClassWithInheritance {
        public string B;
        public override bool Equals(object obj) {
            return obj is DerivedBaseClassA &&
                ((DerivedBaseClassA)obj).A == A && ((DerivedBaseClassA)obj).B == B;
        }
    }
    internal class DerivedBaseClassB : BaseClassWithInheritance {
        public string C;
        public override bool Equals(object obj) {
            return obj is DerivedBaseClassB &&
                ((DerivedBaseClassB)obj).A == A && ((DerivedBaseClassB)obj).C == C;
        }
    }

    [SerializationRequireCustomConverter]
    internal class RequireCustomConverter {
    }

    internal struct SimpleStruct {
        public int A;
        public bool B;
        public string C;
    }

    internal class ClassWithArray {
        public SimpleStruct[] Items;
    }

    [TestClass]
    public class TypeConversionTests {
        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// Assert.AreEqual.
        /// </summary>
        private void RunImportExportTest<T>(T t0) {
            SerializedData exported = (new SerializationConverter()).Export(t0);
            T imported = (new SerializationConverter()).Import<T>(exported);
            Assert.AreEqual(t0, imported);
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// CollectionAssert.AreEqual.
        /// </summary>
        private void RunCollectionImportExportTest<TCollection>(TCollection collection) where TCollection : ICollection {
            SerializedData exported = (new SerializationConverter()).Export(collection);
            TCollection imported = (new SerializationConverter()).Import<TCollection>(exported);
            CollectionAssert.AreEqual(collection, imported);
        }

        [TestMethod]
        [ExpectedException(typeof(RequiresCustomConverterException))]
        public void RequireCustomConverterImport() {
            SerializationConverter converter = new SerializationConverter();
            converter.Import<RequireCustomConverter>(SerializedData.CreateDictionary());
        }

        [TestMethod]
        [ExpectedException(typeof(RequiresCustomConverterException))]
        public void RequireCustomConverterExport() {
            SerializationConverter converter = new SerializationConverter();
            converter.Export(new RequireCustomConverter());
        }

        [TestMethod]
        public void ImportDeserializingPrimitives() {
            SerializedData a = new SerializedData(Real.CreateDecimal(3));
            SerializedData b = new SerializedData(true);
            SerializedData c = new SerializedData(false);
            SerializedData d = new SerializedData("hello");

            SerializationConverter converter = new SerializationConverter();
            Assert.AreEqual((Real)3, converter.Import<Real>(a));
            Assert.AreEqual(true, converter.Import<bool>(b));
            Assert.AreEqual(false, converter.Import<bool>(c));
            Assert.AreEqual("hello", converter.Import<string>(d));
        }

        [TestMethod]
        public void ImportDeserializingStructs() {
            SerializedData serialized = SerializedData.CreateDictionary();
            serialized.AsDictionary["A"] = new SerializedData(Real.CreateDecimal(3));
            serialized.AsDictionary["B"] = new SerializedData(true);
            serialized.AsDictionary["C"] = new SerializedData("hello");

            SerializationConverter converter = new SerializationConverter();
            SimpleStruct simpleStruct = converter.Import<SimpleStruct>(serialized);

            Assert.AreEqual(3, simpleStruct.A);
            Assert.AreEqual(true, simpleStruct.B);
            Assert.AreEqual("hello", simpleStruct.C);
        }

        [TestMethod]
        public void ImportComplexArray() {
            SerializedData serialized = Parser.Parse(@"
{
    Items: [
        {
            A: 1
            B: true
            C: ""1""
        }
        {
            A: 2
            B: false
            C: ""2""
        }
        {
            A: 3
            B: true
            C: ""3""
        }
    ]
}
");
            SerializationConverter converter = new SerializationConverter();
            ClassWithArray deserialized = converter.Import<ClassWithArray>(serialized);

            Assert.AreEqual(1, deserialized.Items[0].A);
            Assert.AreEqual(true, deserialized.Items[0].B);
            Assert.AreEqual("1", deserialized.Items[0].C);
            Assert.AreEqual(2, deserialized.Items[1].A);
            Assert.AreEqual(false, deserialized.Items[1].B);
            Assert.AreEqual("2", deserialized.Items[1].C);
            Assert.AreEqual(3, deserialized.Items[2].A);
            Assert.AreEqual(true, deserialized.Items[2].B);
            Assert.AreEqual("3", deserialized.Items[2].C);
        }

        [TestMethod]
        public void ExportPrimitives() {
            SerializationConverter converter = new SerializationConverter();
            Assert.AreEqual(new SerializedData(3), converter.Export(3));
            Assert.AreEqual(new SerializedData(true), converter.Export(true));
            Assert.AreEqual(new SerializedData(false), converter.Export(false));
            Assert.AreEqual(new SerializedData("hello"), converter.Export("hello"));
        }

        [TestMethod]
        public void ExportPrimitivesAsString() {
            SerializationConverter converter = new SerializationConverter();
            Assert.AreEqual("3", converter.Export(3).PrettyPrinted);
            Assert.AreEqual("true", converter.Export(true).PrettyPrinted);
            Assert.AreEqual("false", converter.Export(false).PrettyPrinted);
            Assert.AreEqual("\"hello\"", converter.Export("hello").PrettyPrinted);
        }

        [TestMethod]
        public void ExportStructs() {
            SimpleStruct s = new SimpleStruct() {
                A = 3,
                B = true,
                C = "hi"
            };

            SerializationConverter converter = new SerializationConverter();
            SerializedData exported = converter.Export(s);

            Assert.AreEqual(@"{
    A: 3
    B: true
    C: ""hi""
}", exported.PrettyPrinted);
        }

        [TestMethod]
        public void ExportComplexArray() {
            string serializedString = @"{
    Items: [
        {
            A: 1
            B: true
            C: ""1""
        }
        {
            A: 2
            B: false
            C: ""2""
        }
        {
            A: 3
            B: true
            C: ""3""
        }
    ]
}";
            SerializedData serializedData = Parser.Parse(serializedString);

            SerializationConverter converter = new SerializationConverter();
            ClassWithArray deserialized = converter.Import<ClassWithArray>(serializedData);

            SerializedData reserializedString = converter.Export(deserialized);
            Assert.AreEqual(serializedString, reserializedString.PrettyPrinted);
        }

        [TestMethod]
        public void ImportExportEnums() {
            RunImportExportTest(MyEnum.MyEnum0);
            RunImportExportTest(MyEnum.MyEnum1);
            RunImportExportTest(MyEnum.MyEnum2);
            RunImportExportTest(MyEnum.MyEnum3);
            RunImportExportTest(MyEnum.MyEnum4);

            RunImportExportTest(new EnumContainer() {
                EnumA = MyEnum.MyEnum0,
                EnumB = MyEnum.MyEnum1
            });
            RunImportExportTest(new EnumContainer() {
                EnumA = MyEnum.MyEnum1,
                EnumB = MyEnum.MyEnum1
            });
            RunImportExportTest(new EnumContainer() {
                EnumA = MyEnum.MyEnum4,
                EnumB = MyEnum.MyEnum2
            });
            RunImportExportTest(new EnumContainer() {
                EnumA = MyEnum.MyEnum0,
                EnumB = MyEnum.MyEnum0
            });

            Dictionary<MyEnum, string> dict = new Dictionary<MyEnum, string>();
            dict.Add(MyEnum.MyEnum0, "ok");
            RunCollectionImportExportTest(dict);
        }

        /// <summary>
        /// Runs an inheritance test. The generic parameter is the interface type (the base type in
        /// the inheritance tree), instanceA is a derived class of the interface type, and instanceB
        /// is another derived type of the interface type.
        /// </summary>
        private void RunInheritanceTest<InterfaceType>(InterfaceType instanceA, InterfaceType instanceB) {
            SerializedData exported = (new SerializationConverter()).Export(instanceA);
            InterfaceType imported = (new SerializationConverter()).Import<InterfaceType>(exported);
            Assert.IsInstanceOfType(imported, instanceA.GetType());
            Assert.AreEqual(instanceA, imported);

            exported = (new SerializationConverter()).Export(instanceB);
            imported = (new SerializationConverter()).Import<InterfaceType>(exported);
            Assert.IsInstanceOfType(imported, instanceB.GetType());
            Assert.AreEqual(instanceB, imported);
        }

        [TestMethod]
        public void InterfaceInheritance() {
            RunInheritanceTest<IInterface>(
                new DerivedInterfaceA(),
                new DerivedInterfaceB());
        }

        [TestMethod]
        public void AbstractClassInheritance() {
            RunInheritanceTest<AbstractClass>(
                new DerivedAbstractClassA() {
                    A = "aA",
                    B = "aB"
                }, new DerivedAbstractClassB() {
                    A = "bA",
                    C = "bC"
                });
        }

        [TestMethod]
        public void BaseClassInheritance() {
            RunInheritanceTest<BaseClassWithInheritance>(
                new DerivedBaseClassA() {
                    A = "aA",
                    B = "aB"
                }, new DerivedBaseClassB() {
                    A = "bA",
                    C = "bC"
                });
        }

        [TestMethod]
        public void ImportExportDictionary() {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict["1"] = 1;
            dict["2"] = 2;
            dict["3"] = 3;
            dict["4"] = 4;
            dict["5"] = 5;
            RunCollectionImportExportTest(dict);

            Dictionary<MyEnum, string> dict2 = new Dictionary<MyEnum, string>();
            dict2[MyEnum.MyEnum0] = "0";
            dict2[MyEnum.MyEnum1] = "1";
            dict2[MyEnum.MyEnum2] = "2";
            dict2[MyEnum.MyEnum3] = "3";
            dict2[MyEnum.MyEnum4] = "4";
            RunCollectionImportExportTest(dict2);
        }

        [TestMethod]
        public void ImportExportSortedDictionary() {
            SortedDictionary<int, string> dict = new SortedDictionary<int, string>();
            dict[1] = "1";
            dict[2] = "2";
            dict[3] = "3";
            dict[4] = "4";
            dict[5] = "5";
            RunCollectionImportExportTest(dict);

            SortedDictionary<MyEnum, string> dict2 = new SortedDictionary<MyEnum, string>();
            dict2[MyEnum.MyEnum0] = "0";
            dict2[MyEnum.MyEnum1] = "1";
            dict2[MyEnum.MyEnum2] = "2";
            dict2[MyEnum.MyEnum3] = "3";
            dict2[MyEnum.MyEnum4] = "4";
            RunCollectionImportExportTest(dict2);
        }
    }
}