using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Entities.E2ETests {
    [TestClass]
    public class LevelManagementTests {
        [TestMethod]
        public void LoadTemplateContext() {
            ITemplateGroup templates = TestUtils.CreateDefaultTemplates();
            string saved = LevelManager.SaveTemplates(templates);
            ITemplateGroup restored = LevelManager.LoadTemplates(saved);

            CollectionAssert.AreEqual(templates.Templates.ToList(), restored.Templates.ToList());
        }

        [TestMethod]
        public void RoundTripEmptyTemplateGroup() {
            LevelManager.LoadTemplates(LevelManager.SaveTemplates(LevelManager.CreateTemplateGroup()));
        }
    }
}