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
            IEntity entity = EntityFactory.Create();
        }

        [TestMethod]
        public void EntityUniqueId() {
            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < 200; ++i) {
                IEntity entity = EntityFactory.Create();
                Assert.IsTrue(ids.Add(entity.UniqueId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(AlreadyAddedDataException))]

        public void CannotAddMultipleDataInstances() {
            IEntity entity = EntityFactory.Create();
            entity.AddData<TestData0>();
            entity.AddData<TestData0>();
        }
    }
}
