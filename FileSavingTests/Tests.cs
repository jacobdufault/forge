using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.FileSaving;
using Neon.Serialization;

namespace FileSavingTests {
    internal class FileItem0 : IFileItem {

        public Guid Identifier {
            get { return new Guid("86394AB6-C02D-45BB-9198-7C93F801AAA9"); }
        }

        public string PrettyIdentifier {
            get { return "FileItem0"; }
        }

        public SerializedData Export() {
            return new SerializedData(33);
        }

        public void Import(SerializedData data) {
            Assert.AreEqual(33, data.AsReal.AsInt);
        }
    }

    internal class FileItem1 : IFileItem {

        public Guid Identifier {
            get { return new Guid("8782D43C-839E-4BEE-8D08-520943584505"); }
        }

        public string PrettyIdentifier {
            get { return "FileItem1"; }
        }

        public SerializedData Export() {
            return new SerializedData(44);
        }

        public void Import(SerializedData data) {
            Assert.AreEqual(44, data.AsReal.AsInt);
        }
    }

    internal class NullItem : IFileItem {

        public Guid Identifier {
            get { return Guid.Empty; }
        }

        public string PrettyIdentifier {
            get { return null; }
        }

        public SerializedData Export() {
            return null;
        }

        public void Import(SerializedData data) {
        }
    }

    [TestClass]
    public class Tests {
        [TestMethod]
        public void EmptyFile() {
            FileWriter writer = new FileWriter();
            Assert.AreEqual(SerializedData.CreateList().PrettyPrinted, writer.FileContents);
        }

        [TestMethod]
        public void MultipleFileItems() {
            FileWriter writer = new FileWriter();
            writer.WriteFileItem(new FileItem0());
            writer.WriteFileItem(new FileItem1());

            FileReader reader = new FileReader(writer.FileContents);
            Assert.IsTrue(reader.GetFileItem<FileItem0>().Exists);
            Assert.IsTrue(reader.GetFileItem<FileItem1>().Exists);
        }

        [TestMethod]
        public void MissingFileItem() {
            FileWriter writer = new FileWriter();
            writer.WriteFileItem(new FileItem0());

            FileReader reader = new FileReader(writer.FileContents);
            Assert.IsTrue(reader.GetFileItem<FileItem0>().Exists);
            Assert.IsFalse(reader.GetFileItem<FileItem1>().Exists);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReaddFileItem() {
            FileWriter writer = new FileWriter();
            writer.WriteFileItem(new FileItem0());
            writer.WriteFileItem(new FileItem0());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteNullItem() {
            FileWriter writer = new FileWriter();
            writer.WriteFileItem(null);
        }

        [TestMethod]
        public void WriteInvalidItem() {
            FileWriter writer = new FileWriter();
            writer.WriteFileItem(new NullItem());
        }
    }
}