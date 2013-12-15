using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Tests {
    internal class AfterImportPrivate {
        public int RunCount;

        [SerializationAfterImport]
        private void Run() {
            ++RunCount;
        }
    }

    internal class AfterImportProtected {
        public int RunCount;

        [SerializationAfterImport]
        protected void Run() {
            ++RunCount;
        }
    }

    internal class AfterImportInternal {
        public int RunCount;

        [SerializationAfterImport]
        internal void Run() {
            ++RunCount;
        }
    }

    internal class AfterImportPublic {
        public int RunCount;

        [SerializationAfterImport]
        public void Run() {
            ++RunCount;
        }
    }

    internal class AfterImportMultiple {
        public int RunCount;

        [SerializationAfterImport]
        private void Run0() {
            ++RunCount;
        }

        [SerializationAfterImport]
        private void Run1() {
            ++RunCount;
        }
    }

    internal class AfterImportInherited : AfterImportPrivate {
    }

    [TestClass]
    public class AfterImportTests {
        [TestMethod]
        public void AfterImport() {
            Assert.AreEqual(1, SerializationHelpers.ImportExport(new AfterImportPrivate()).RunCount);
            Assert.AreEqual(1, SerializationHelpers.ImportExport(new AfterImportProtected()).RunCount);
            Assert.AreEqual(1, SerializationHelpers.ImportExport(new AfterImportInternal()).RunCount);
            Assert.AreEqual(1, SerializationHelpers.ImportExport(new AfterImportPublic()).RunCount);
            Assert.AreEqual(2, SerializationHelpers.ImportExport(new AfterImportMultiple()).RunCount);

            Assert.AreEqual(1, SerializationHelpers.ImportExport(new AfterImportInherited()).RunCount);
        }
    }
}