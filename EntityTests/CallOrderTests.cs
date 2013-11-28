using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Tests {

    internal class TestData0 : BaseData<TestData0> {
        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(TestData0 source) {
        }
    }

    internal class TestData1 : BaseData<TestData1> {
        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(TestData1 source) {
        }
    }

    /*
    internal enum TriggerEvent {
        OnAdded,
        OnRemoved,
        OnModified,
        OnUpdate,
        OnGlobalPreUpdate,
        OnGlobalPostUpdate,
        OnInput,
        OnGlobalInput
    }

    internal class TriggerEventLogger : ITriggerAdded, ITriggerRemoved, ITriggerModified, ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate, ITriggerInput, ITriggerGlobalInput {
        public virtual Type[] ComputeEntityFilter() {
            return new Type[] { };
        }

        public List<TriggerEvent> _events = new List<TriggerEvent>();

        public TriggerEvent[] Events {
            get {
                return _events.ToArray();
            }
        }

        public void ClearEvents() {
            _events.Clear();
        }

        public void OnAdded(IEntity entity) {
            _events.Add(TriggerEvent.OnAdded);
        }

        public void OnRemoved(IEntity entity) {
            _events.Add(TriggerEvent.OnRemoved);
        }

        public void OnModified(IEntity entity) {
            _events.Add(TriggerEvent.OnModified);
        }

        public void OnUpdate(IEntity entity) {
            _events.Add(TriggerEvent.OnUpdate);
        }

        public void OnGlobalPreUpdate(IEntity singletonEntity) {
            _events.Add(TriggerEvent.OnGlobalPreUpdate);
        }

        public void OnGlobalPostUpdate(IEntity singletonEntity) {
            _events.Add(TriggerEvent.OnGlobalPostUpdate);
        }

        public Type IStructuredInputType {
            get { return typeof(int); }
        }

        public void OnInput(IGameInput input, IEntity entity) {
            _events.Add(TriggerEvent.OnInput);
        }

        public void OnGlobalInput(IGameInput input, IEntity singletonEntity) {
            _events.Add(TriggerEvent.OnGlobalInput);
        }
    }

    internal class TriggerEventLoggerFilterRequiresData0 : TriggerEventLogger {
        public override Type[] ComputeEntityFilter() {
            return new[] { typeof(TestData0) };
        }
    }

    [TestClass]
    public class CallOrderTests {
        [TestMethod]
        public void Basic() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLogger();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            em.AddEntity(entity);

            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate
            }, trigger.Events);
            trigger.ClearEvents();

            for (int i = 0; i < 20; ++i) {
                em.UpdateWorld();
                CollectionAssert.AreEqual(new TriggerEvent[] {
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate,
                }, trigger.Events);
                trigger.ClearEvents();
            }

            em.RemoveEntity(entity);
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
        }

        [TestMethod]
        public void InitializeWithAddingData() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLogger();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            em.AddEntity(entity);

            entity.AddData<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // adding new data shouldn't impact the filter, so it should be like a regular update
            entity.AddData<TestData1>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }

        [TestMethod]
        public void EntityModifyBeforeUpdate() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLogger();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            TestData0 data = entity.AddData<TestData0>();
            entity.Modify<TestData0>();
            em.AddEntity(entity);

            // even though we modified we shouldn't care -- it should be considered part of the
            // initialization
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }

        [TestMethod]
        public void EntityRemoveNothing() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            TestData0 data = entity.AddData<TestData0>();
            em.AddEntity(entity);

            // do the add
            em.UpdateWorld();
            trigger.ClearEvents();

            // do an update
            entity.Modify<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnModified,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // remove the entity
            entity.Modify<TestData0>();
            entity.Destroy();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            try {
                entity.Modify<TestData0>();
                Assert.Fail();
            }
            catch (NoSuchDataException) { }
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }

        [TestMethod]
        public void EntityModifyAfterUpdate() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            TestData0 data = entity.AddData<TestData0>();
            em.AddEntity(entity);

            // do the add
            em.UpdateWorld();
            trigger.ClearEvents();

            // modify the data
            entity.Modify<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnModified,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // add a Data1 instance
            entity.AddData<TestData1>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // modify the Data1 instance
            entity.Modify<TestData1>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();
        }

        [TestMethod]
        public void InitializeBeforeAddingDataFilter() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            TestData0 data = entity.AddData<TestData0>();
            em.AddEntity(entity);

            // entity now has the data
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // adding random data should not trigger a modification notification
            entity.AddData<TestData1>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // entity no longer has the data, it should get removed
            entity.RemoveData<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
        }

        [TestMethod]
        public void InitializeAfterAddingDataFilter() {
            EntityManager em = new EntityManager(EntityHelpers.CreateEntity());
            TriggerEventLogger trigger = new TriggerEventLoggerFilterRequiresData0();
            em.AddSystem(trigger);
            IEntity entity = EntityHelpers.CreateEntity();
            em.AddEntity(entity);

            // entity doesn't have required data
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // entity now has the data
            TestData0 data = entity.AddData<TestData0>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // adding random data should not trigger a modification notification
            entity.AddData<TestData1>();
            em.UpdateWorld();
            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            // entity no longer has the data, it should get removed
            entity.RemoveData<TestData0>();
            em.UpdateWorld();

            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
        }
    }
    */
}