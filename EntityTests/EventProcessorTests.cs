﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;

namespace EntityTests {
    class TestEvent0 : IEvent { }
    class TestEvent1 : IEvent { }

    [TestClass]
    public class EventProcessorTests {
        [TestMethod]
        public void CreateProcessor() {
            EventProcessor processor = new EventProcessor();
        }

        [TestMethod]
        public void NewEntityCreatesEventProcessor() {
            IEntity e = EntityFactory.Create();
            Assert.IsNotNull(e.EventProcessor);
        }

        [TestMethod]
        public void EntityManagerIntegration() {
            EntityManager em = new EntityManager(EntityFactory.Create());
            IEntity e = EntityFactory.Create();
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

            e.EventProcessor.Dispatch(new TestEvent0());

            Assert.AreEqual(0, called0);
            Assert.AreEqual(0, called1);

            em.UpdateWorld();

            Assert.AreEqual(1, called0);
            Assert.AreEqual(0, called1);
        }
    }
}
