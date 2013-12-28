using Forge.Entities.Implementation.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Entities.Tests {
    [TestClass]
    public class EventNotifierTests {
        private class TestEvent : BaseEvent<TestEvent> {
            private TestEvent() {
            }

            public static TestEvent Create() {
                return GetInstance();
            }
        }

        [TestMethod]
        public void EventsAreNotRedispatched() {
            EventNotifier notifier = new EventNotifier();

            int callCount = 0;
            notifier.OnEvent<TestEvent>(evnt => ++callCount);

            notifier.DispatchEvents();
            Assert.AreEqual(0, callCount);

            notifier.Submit(TestEvent.Create());
            notifier.DispatchEvents();
            Assert.AreEqual(1, callCount);
            callCount = 0;

            notifier.DispatchEvents();
            Assert.AreEqual(0, callCount);

            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());
            notifier.DispatchEvents();
            Assert.AreEqual(3, callCount);
            callCount = 0;

            notifier.DispatchEvents();
            Assert.AreEqual(0, callCount);
        }

        [TestMethod]
        public void EventsAreNotRedispatchedWithConcurrentDispatch() {
            EventNotifier notifier = new EventNotifier();

            int callCount = 0;
            notifier.OnEvent<TestEvent>(evnt => {
                notifier.Submit(TestEvent.Create());
                ++callCount;
            });

            notifier.DispatchEvents();
            Assert.AreEqual(0, callCount);

            notifier.Submit(TestEvent.Create());
            notifier.DispatchEvents();
            Assert.AreEqual(1, callCount);

            for (int i = 0; i < 20; ++i) {
                callCount = 0;
                notifier.DispatchEvents();
                Assert.AreEqual(1, callCount);
            }

            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());

            for (int i = 0; i < 20; ++i) {
                callCount = 0;
                notifier.DispatchEvents();
                Assert.AreEqual(4, callCount);
            }
        }

    }
}