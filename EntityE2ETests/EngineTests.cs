using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    [TestClass]
    public class EngineTests {
        [TestMethod]
        public void EngineTest() {
            ITemplateGroup templates = TestUtils.CreateDefaultTemplates();
            IGameSnapshot snapshot = TestUtils.CreateDefaultDatabase(templates);

            GameEngineFactory.CreateEngine(snapshot, templates);
        }
    }
}