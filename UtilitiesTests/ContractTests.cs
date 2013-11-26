using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Utilities.Tests {
    [TestClass]
    public class ContractTests {
        private void TestParameters(object p) {
            Contract.AssertArguments(p, "p");
        }

        private void TestParameters(object p0, object p1) {
            Contract.AssertArguments(p0, "p0", p1, "p1");
        }
        private void TestParameters(object p0, object p1, object p2) {
            Contract.AssertArguments(p0, "p0", p1, "p1", p2, "p2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyParams() {
            TestParameters(null, null);
        }
    }
}