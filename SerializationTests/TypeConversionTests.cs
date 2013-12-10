using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Collections;
using Neon.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Serialization.Tests {
    [SerializationSupportCyclicReferences]
    internal class CyclicReference {
        public CyclicReference Reference;
        public int A;
        public MyEnum B;
    }

    [SerializationSupportCyclicReferences]
    internal class CyclicReference1 {
        public CyclicReference1 Reference1;
        public CyclicReference2 Reference2;
        public int A;
    }

    [SerializationSupportCyclicReferences]
    internal class CyclicReference2 {
        public CyclicReference1 Reference1;
        public CyclicReference2 Reference2;
        public int B;
    }

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

    internal struct CustomConverterOverride {
        public IInterface InterfaceObject;
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
        /// Exports the value using a serialization converter, then reimports it using a new
        /// serialization converter. Returns the reimported value.
        /// </summary>
        private T GetImportedExportedValue<T>(T t0, SerializationConverter exporter = null,
            SerializationConverter importer = null) {
            SerializedData exported = (exporter ?? new SerializationConverter()).Export(t0);
            T imported = (importer ?? new SerializationConverter()).Import<T>(exported);
            return imported;
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// Assert.AreEqual.
        /// </summary>
        private void RunImportExportTest<T>(T t0, SerializationConverter exporter = null,
            SerializationConverter importer = null) {
            T imported = GetImportedExportedValue(t0, exporter, importer);
            Assert.AreEqual(t0, imported);
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// CollectionAssert.AreEqual.
        /// </summary>
        private void RunCollectionImportExportTest<TCollection>(TCollection collection,
            SerializationConverter exporter = null, SerializationConverter importer = null)
            where TCollection : ICollection {
            TCollection imported = GetImportedExportedValue(collection, exporter, importer);
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
            dict.Add(MyEnum.MyEnum0, "OK");
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

        [TestMethod]
        public void ImportExportCustomConverterOverride() {
            CustomConverterOverride c = new CustomConverterOverride() {
                InterfaceObject = new DerivedInterfaceB()
            };
            SerializationConverter exporter = new SerializationConverter();
            exporter.AddExporter<CustomConverterOverride>(i => new SerializedData("good data"));

            bool ranImporter = false;
            SerializationConverter importer = new SerializationConverter();
            importer.AddImporter<CustomConverterOverride>(data => {
                Assert.AreEqual("good data", data.AsString);
                ranImporter = true;
                return c;
            });

            RunImportExportTest(c, exporter, importer);

            Assert.IsTrue(ranImporter, "Failed to use custom importer");
        }

        [TestMethod]
        public void ImportExportOverrideInterfaceSupport() {
            IInterface iface = new DerivedInterfaceA();

            SerializationConverter exporter = new SerializationConverter();
            exporter.AddExporter<IInterface>(i => new SerializedData("good data"));

            bool ranImporter = false;
            SerializationConverter importer = new SerializationConverter();
            importer.AddImporter<IInterface>(data => {
                Assert.AreEqual("good data", data.AsString);
                ranImporter = true;
                return iface;
            });

            RunImportExportTest(iface, exporter, importer);

            Assert.IsTrue(ranImporter, "The generic inheritance importer was used instead of the custom one");
        }

        [TestMethod]
        public void ImportExportBag() {
            Bag<int> bag = new Bag<int>() {
                1, 2, 3, 4, 5
            };
            RunCollectionImportExportTest(bag);
        }

        [TestMethod]
        public void ImportExportSparseArray() {
            SparseArray<string> original = new SparseArray<string>();
            original[0] = "0";
            original[2] = "2";
            original[4] = "4";
            original[6] = "6";
            original[8] = "8";

            SparseArray<string> imported = GetImportedExportedValue(original);

            var originalEnumerator = original.GetEnumerator();
            var importedEnumerator = imported.GetEnumerator();

            while (originalEnumerator.MoveNext() && importedEnumerator.MoveNext()) {
                Assert.AreEqual(originalEnumerator.Current, importedEnumerator.Current);
            }

            Assert.IsFalse(originalEnumerator.MoveNext(), "Not enough elements in the imported collection");
            Assert.IsFalse(importedEnumerator.MoveNext(), "Too many elements in the imported collection");
        }

        [TestMethod]
        public void CyclicReferenceTestUsingSupportMethods() {
            CyclicReference a = new CyclicReference();
            CyclicReference b = new CyclicReference();

            a.Reference = b;
            a.A = 1;
            a.B = MyEnum.MyEnum0;

            b.Reference = a;
            b.A = 2;
            b.B = MyEnum.MyEnum1;

            SerializedData exportedData = (new SerializationConverter()).ExportGraph(typeof(CyclicReference), a);
            CyclicReference imported = (CyclicReference)(new SerializationConverter()).ImportGraph(typeof(CyclicReference), exportedData);

            Assert.AreEqual(a.A, imported.A);
            Assert.AreEqual(a.B, imported.B);
            Assert.AreEqual(b.A, imported.Reference.A);
            Assert.AreEqual(b.B, imported.Reference.B);
            Assert.AreEqual(imported, imported.Reference.Reference);
        }

        [TestMethod]
        public void ListOfCyclicReferences() {
            CyclicReference a = new CyclicReference() {
                A = 1,
                B = MyEnum.MyEnum0
            };
            CyclicReference b = new CyclicReference() {
                A = 2,
                B = MyEnum.MyEnum1
            };
            CyclicReference c = new CyclicReference() {
                A = 3,
                B = MyEnum.MyEnum2
            };
            CyclicReference d = new CyclicReference() {
                A = 4,
                B = MyEnum.MyEnum3
            };
            CyclicReference e = new CyclicReference() {
                A = 5,
                B = MyEnum.MyEnum4
            };

            a.Reference = b;
            b.Reference = c;
            c.Reference = d;
            d.Reference = e;
            e.Reference = a;

            List<CyclicReference> exportedReferences = new List<CyclicReference>() {
                a, b, c, d, e
            };

            SerializedData exportedData = (new SerializationConverter()).ExportGraph(exportedReferences.GetType(), exportedReferences);
            List<CyclicReference> importedReferences = (List<CyclicReference>)(new SerializationConverter()).ImportGraph(exportedReferences.GetType(), exportedData);

            Assert.AreEqual(exportedReferences[0].A, importedReferences[0].A);
            Assert.AreEqual(exportedReferences[0].B, importedReferences[0].B);
            Assert.AreEqual(exportedReferences[1].A, importedReferences[1].A);
            Assert.AreEqual(exportedReferences[1].B, importedReferences[1].B);
            Assert.AreEqual(exportedReferences[2].A, importedReferences[2].A);
            Assert.AreEqual(exportedReferences[2].B, importedReferences[2].B);
            Assert.AreEqual(exportedReferences[3].A, importedReferences[3].A);
            Assert.AreEqual(exportedReferences[3].B, importedReferences[3].B);
            Assert.AreEqual(exportedReferences[4].A, importedReferences[4].A);
            Assert.AreEqual(exportedReferences[4].B, importedReferences[4].B);

            Assert.IsTrue(importedReferences[0].Reference == importedReferences[1]);
            Assert.IsTrue(importedReferences[1].Reference == importedReferences[2]);
            Assert.IsTrue(importedReferences[2].Reference == importedReferences[3]);
            Assert.IsTrue(importedReferences[3].Reference == importedReferences[4]);
            Assert.IsTrue(importedReferences[4].Reference == importedReferences[0]);
        }

        [TestMethod]
        public void CyclicGraphComplex() {
            // significantly more complex object graph than the previous object graph serialization
            // test... this one involves multiple types and cycles within cycles

            CyclicReference1 ref1a = new CyclicReference1() {
                A = 1
            };
            CyclicReference1 ref1b = new CyclicReference1() {
                A = 2
            };
            CyclicReference2 ref2a = new CyclicReference2() {
                B = 3
            };
            CyclicReference2 ref2b = new CyclicReference2() {
                B = 4
            };

            ref1a.Reference1 = ref1b;
            ref1b.Reference1 = ref1a;

            ref2a.Reference2 = ref2b;
            ref2b.Reference2 = ref2b;

            ref1a.Reference2 = ref2a;
            ref1b.Reference2 = ref2b;
            ref2a.Reference1 = ref1a;
            ref2b.Reference1 = ref1b;

            SerializedData exportedData = (new SerializationConverter()).ExportGraph(typeof(CyclicReference1), ref1a);
            CyclicReference1 iref1a = (CyclicReference1)(new SerializationConverter()).ImportGraph(typeof(CyclicReference1), exportedData);

            Assert.AreEqual(ref1a.A, iref1a.A);
            Assert.AreEqual(ref1b.A, iref1a.Reference1.A);
            Assert.AreEqual(ref2a.B, iref1a.Reference2.B);
            Assert.AreEqual(ref2b.B, iref1a.Reference2.Reference2.B);

            Assert.AreEqual(iref1a, iref1a.Reference1.Reference1);
            Assert.AreEqual(iref1a, iref1a.Reference2.Reference1);
            Assert.AreEqual(iref1a, iref1a.Reference2.Reference2.Reference1.Reference1);
        }
    }
}