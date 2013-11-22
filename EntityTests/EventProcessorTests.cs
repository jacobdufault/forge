using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neon.Entities.Tests {
    internal class TestEvent0 : IEvent { }
    internal class TestEvent1 : IEvent { }

    [TestClass]
    public class EventProcessorTests {
        [TestMethod]
        public void NewEntityCreatesEventProcessor() {
            IEntity e = EntityHelpers.CreateEntity();
            Assert.IsNotNull(e.EventProcessor);
        }

        [TestMethod]
        public void EntityManagerIntegration() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            IEntity e = EntityHelpers.CreateEntity();
            em.AddEntity(e);
            em.UpdateWorld();

            int called0 = 0;
            int called1 = 0;

            e.EventProcessor.OnEvent<TestEvent0>(evnt => {
                Assert.IsInstanceOfType(evnt, typeof(TestEvent0));
                called0++;
            });
            e.EventProcessor.OnEvent<TestEvent1>(evnt => {
                Assert.IsInstanceOfType(evnt, typeof(TestEvent1));
                called1++;
            });

            e.EventProcessor.Submit(new TestEvent0());

            Assert.AreEqual(0, called0);
            Assert.AreEqual(0, called1);

            em.RunEventProcessors();

            Assert.AreEqual(1, called0);
            Assert.AreEqual(0, called1);
        }
    }
}