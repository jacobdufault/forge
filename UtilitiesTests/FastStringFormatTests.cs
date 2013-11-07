using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neon.Utilities.Tests {
    [TestClass]
    public class FastStringFormatTests {
        [TestMethod]
        public void OneArgument() {
            Assert.AreEqual("1", FastStringFormat.Format("{0}", "1"));
            Assert.AreEqual(" 1", FastStringFormat.Format(" {0}", "1"));
            Assert.AreEqual(" 1 ", FastStringFormat.Format(" {0} ", "1"));

            Assert.AreEqual("11", FastStringFormat.Format("{0}{0}", "1"));
            Assert.AreEqual("1 1", FastStringFormat.Format("{0} {0}", "1"));
        }

        [TestMethod]
        public void NArguments() {
            Assert.AreEqual("0", FastStringFormat.Format("{0}", "0"));
            Assert.AreEqual("01", FastStringFormat.Format("{0}{1}", "0", "1"));
            Assert.AreEqual("012", FastStringFormat.Format("{0}{1}{2}", "0", "1", "2"));
            Assert.AreEqual("0123", FastStringFormat.Format("{0}{1}{2}{3}", "0", "1", "2", "3"));
            Assert.AreEqual("01234", FastStringFormat.Format("{0}{1}{2}{3}{4}", "0", "1", "2", "3", "4"));
            Assert.AreEqual("012345", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}", "0", "1", "2", "3", "4", "5"));
            Assert.AreEqual("0123456", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}", "0", "1", "2", "3", "4", "5", "6"));
            Assert.AreEqual("01234567", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}", "0", "1", "2", "3", "4", "5", "6", "7"));
            Assert.AreEqual("012345678", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "0", "1", "2", "3", "4", "5", "6", "7", "8"));
            Assert.AreEqual("0123456789", FastStringFormat.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"));
        }
    }
}