using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Serialization;
using Neon.Utilities;

namespace JSONTests {
    internal struct SimpleStruct {
        public int A;
        public bool B;
        public string C;
    }

    internal class ClassWithArray {
        public SimpleStruct[] Items;
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

    [TestClass]
    public class TypeConversionTests {
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
        public void ImportPrivateFields() {
            SerializedData serialized = SerializedData.CreateDictionary();
            serialized.AsDictionary["A"] = new SerializedData(Real.CreateDecimal(3));

            SerializationConverter converter = new SerializationConverter();
            ClassWithPrivateField obj = converter.Import<ClassWithPrivateField>(serialized);

            Assert.AreEqual((Real)3, obj.GetA());
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
    }
}