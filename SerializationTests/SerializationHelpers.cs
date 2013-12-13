using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Tests {
    internal static class SerializationHelpers {
        public static T ImportExport<T>(T value) {
            SerializedData exported = ObjectSerializer.Export(value);
            T imported = ObjectSerializer.Import<T>(exported);
            return imported;
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// Assert.AreEqual.
        /// </summary>
        public static void RunImportExportTest<T>(T t0) {
            T imported = SerializationHelpers.ImportExport(t0);
            Assert.AreEqual(t0, imported);
        }

        /// <summary>
        /// Helper method that exports the given instance, imports the exported data, and then
        /// asserts that the imported instance is equal to the original instance using
        /// CollectionAssert.AreEqual.
        /// </summary>
        public static void RunCollectionImportExportTest<TCollection>(TCollection collection)
            where TCollection : ICollection {
            TCollection imported = ImportExport(collection);
            CollectionAssert.AreEqual(collection, imported);
        }

    }
}