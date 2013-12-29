using Forge.Entities.Implementation.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Forge.Entities.Tests {
    public class EventNotifierTests {
        private class TestEvent : BaseEvent<TestEvent> {
            private TestEvent() {
            }

            public static TestEvent Create() {
                return GetInstance();
            }
        }

        [Fact]
        public void EventsAreNotRedispatched() {
            EventNotifier notifier = new EventNotifier();

            int callCount = 0;
            notifier.OnEvent<TestEvent>(evnt => ++callCount);

            notifier.DispatchEvents();
            Assert.Equal(0, callCount);

            notifier.Submit(TestEvent.Create());
            notifier.DispatchEvents();
            Assert.Equal(1, callCount);
            callCount = 0;

            notifier.DispatchEvents();
            Assert.Equal(0, callCount);

            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());
            notifier.DispatchEvents();
            Assert.Equal(3, callCount);
            callCount = 0;

            notifier.DispatchEvents();
            Assert.Equal(0, callCount);
        }

        [Fact]
        public void EventsAreNotRedispatchedWithConcurrentDispatch() {
            EventNotifier notifier = new EventNotifier();

            int callCount = 0;
            notifier.OnEvent<TestEvent>(evnt => {
                notifier.Submit(TestEvent.Create());
                ++callCount;
            });

            notifier.DispatchEvents();
            Assert.Equal(0, callCount);

            notifier.Submit(TestEvent.Create());
            notifier.DispatchEvents();
            Assert.Equal(1, callCount);

            for (int i = 0; i < 20; ++i) {
                callCount = 0;
                notifier.DispatchEvents();
                Assert.Equal(1, callCount);
            }

            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());
            notifier.Submit(TestEvent.Create());

            for (int i = 0; i < 20; ++i) {
                callCount = 0;
                notifier.DispatchEvents();
                Assert.Equal(4, callCount);
            }
        }

    }
}