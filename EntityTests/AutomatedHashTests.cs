using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities.Implementation.Verification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Tests {
    internal class Complex0 {
        public int A;
    }
    internal class Complex1 {
        public int A;
        public Complex0 Complex;
    }

    [TestClass]
    public class AutomatedHashTests {
        [TestMethod]
        public void TestPrimitiveTypes() {
            Assert.AreEqual((1).GetHashCode(), AutomatedHashComputation.GetHash(1));
            Assert.AreEqual((2).GetHashCode(), AutomatedHashComputation.GetHash(2));
            Assert.AreEqual((true).GetHashCode(), AutomatedHashComputation.GetHash(true));
            Assert.AreEqual(("a").GetHashCode(), AutomatedHashComputation.GetHash("a"));
        }

        [TestMethod]
        public void TestComplexTypes() {
            Complex1 c0 = new Complex1();
            c0.A = 5;
            c0.Complex = new Complex0();

            Complex1 c1 = new Complex1();
            c1.A = 5;
            c1.Complex = new Complex0();

            Assert.AreEqual(AutomatedHashComputation.GetHash(c0), AutomatedHashComputation.GetHash(c1));

            Complex1 c2 = new Complex1();
            c2.A = 5;
            c2.Complex = new Complex0();
            c2.Complex.A = 1;

            Assert.AreNotEqual(AutomatedHashComputation.GetHash(c0), AutomatedHashComputation.GetHash(c2));
            Assert.AreNotEqual(AutomatedHashComputation.GetHash(c1), AutomatedHashComputation.GetHash(c2));
        }
    }
}