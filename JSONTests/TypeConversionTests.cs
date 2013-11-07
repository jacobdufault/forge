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
            SerializedValue a = (Real)3.0;
            SerializedValue b = true;
            SerializedValue c = false;
            SerializedValue d = "hello";

            TypeConverter converter = new TypeConverter();
            Assert.AreEqual((Real)3, converter.Import<Real>(a));
            Assert.AreEqual(true, converter.Import<bool>(b));
            Assert.AreEqual(false, converter.Import<bool>(c));
            Assert.AreEqual("hello", converter.Import<string>(d));
        }

        [TestMethod]
        public void ImportDeserializingStructs() {
            SerializedValue serialized = SerializedValue.CreateDictionary();
            serialized["A"] = (Real)3;
            serialized["B"] = true;
            serialized["C"] = "hello";

            TypeConverter converter = new TypeConverter();
            SimpleStruct simpleStruct = converter.Import<SimpleStruct>(serialized);

            Assert.AreEqual(3, simpleStruct.A);
            Assert.AreEqual(true, simpleStruct.B);
            Assert.AreEqual("hello", simpleStruct.C);
        }

        [TestMethod]
        public void ImportPrivateFields() {
            SerializedValue serialized = SerializedValue.CreateDictionary();
            serialized["A"] = (Real)3;

            TypeConverter converter = new TypeConverter();
            ClassWithPrivateField obj = converter.Import<ClassWithPrivateField>(serialized);

            Assert.AreEqual((Real)3, obj.GetA());
        }

        [TestMethod]
        public void ImportComplexArray() {
            SerializedValue serialized = Parser.Parse(@"
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
            TypeConverter converter = new TypeConverter();
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
            TypeConverter converter = new TypeConverter();
            Assert.AreEqual(new SerializedValue(3), converter.Export(3));
            Assert.AreEqual((SerializedValue)true, converter.Export(true));
            Assert.AreEqual((SerializedValue)false, converter.Export(false));
            Assert.AreEqual((SerializedValue)"hello", converter.Export("hello"));
        }

        [TestMethod]
        public void ExportPrimitivesAsString() {
            TypeConverter converter = new TypeConverter();
            Assert.AreEqual("3", converter.Export(3).AsString);
            Assert.AreEqual("true", converter.Export(true).AsString);
            Assert.AreEqual("false", converter.Export(false).AsString);
            Assert.AreEqual("\"hello\"", converter.Export("hello").AsString);
        }

        [TestMethod]
        public void ExportStructs() {
            SimpleStruct s = new SimpleStruct() {
                A = 3,
                B = true,
                C = "hi"
            };

            TypeConverter converter = new TypeConverter();
            SerializedValue exported = converter.Export(s);

            Assert.AreEqual(@"{
    A: 3
    B: true
    C: ""hi""
}", exported.AsString);
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
            SerializedValue serializedValue = Parser.Parse(serializedString);

            TypeConverter converter = new TypeConverter();
            ClassWithArray deserialized = converter.Import<ClassWithArray>(serializedValue);

            SerializedValue reserializedString = converter.Export(deserialized);
            Assert.AreEqual(serializedString, reserializedString.AsString);
        }
    }
}