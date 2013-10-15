using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;

namespace EntityTests {
    [TestClass]
    public class EntityTest {
        [TestMethod]
        public void EntityCreation() {
            Entity entity = new Entity();
        }

        [TestMethod]
        public void EntityUniqueId() {
            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < 200; ++i) {
                Entity entity = new Entity();
                Assert.IsTrue(ids.Add(entity.UniqueId));
            }
        }
    }
}
