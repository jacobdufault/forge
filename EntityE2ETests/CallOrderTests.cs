using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Neon.Entities.E2ETests {
    internal class LambdaTrigger : ITriggerAdded {
        public Type[] ComputeEntityFilter() {
            return EntityFilter;
        }

        public Type[] EntityFilter;

        public LambdaTrigger(Type[] entityFilter) {
            EntityFilter = entityFilter;
        }

        public Action<IEntity> OnAdded;

        void ITriggerAdded.OnAdded(IEntity entity) {
            if (OnAdded != null) OnAdded(entity);
        }
    }

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
        public Type[] ComputeEntityFilter() {
            return EntityFilter;
        }

        public Type[] EntityFilter;

        public TriggerEventLogger(Type[] entityFilter) {
            EntityFilter = entityFilter;
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

    [TestClass]
    public class CallOrderTests {
        public static IContentDatabase CreateEmptyDatabase() {
            return LevelManager.CreateLevel().CurrentState;
        }

        [TestMethod]
        public void AddAndUpdateEntity() {
            IContentDatabase contentDatabase = CreateEmptyDatabase();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            contentDatabase.Systems.Add(trigger);

            IEntity entity = ContentDatabaseHelper.CreateEntity();
            contentDatabase.AddedEntities.Add(entity);

            IGameEngine engine = GameEngineFactory.CreateEngine(contentDatabase);

            engine.SynchronizeState().WaitOne();
            engine.Update();

            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate
            }, trigger.Events);
            trigger.ClearEvents();

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                CollectionAssert.AreEqual(new TriggerEvent[] {
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate,
                }, trigger.Events);
                trigger.ClearEvents();
            }
        }

        [TestMethod]
        public void AddEntityAndModifyInAdd() {
            IContentDatabase contentDatabase = CreateEmptyDatabase();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            contentDatabase.Systems.Add(trigger);

            LambdaTrigger addedTrigger = new LambdaTrigger(new Type[] { });
            addedTrigger.OnAdded = entity => {
                entity.AddData<TestData0>();
                entity.Modify<TestData0>();
            };
            contentDatabase.Systems.Add(addedTrigger);

            contentDatabase.AddedEntities.Add(ContentDatabaseHelper.CreateEntity());

            IGameEngine engine = GameEngineFactory.CreateEngine(contentDatabase);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate,
            }, trigger.Events);
            trigger.ClearEvents();

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                CollectionAssert.AreEqual(new TriggerEvent[] {
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate,
                }, trigger.Events);
                trigger.ClearEvents();
            }
        }

        [TestMethod]
        public void RemoveEntity() {
            IContentDatabase contentDatabase = CreateEmptyDatabase();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            contentDatabase.Systems.Add(trigger);

            IEntity entity = ContentDatabaseHelper.CreateEntity();
            contentDatabase.RemovedEntities.Add(entity);

            IGameEngine engine = GameEngineFactory.CreateEngine(contentDatabase);

            engine.SynchronizeState().WaitOne();
            engine.Update();

            CollectionAssert.AreEqual(new TriggerEvent[] {
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate
            }, trigger.Events);
            trigger.ClearEvents();

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                CollectionAssert.AreEqual(new TriggerEvent[] {
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnGlobalPostUpdate,
                }, trigger.Events);
                trigger.ClearEvents();
            }
        }

        /*

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
        */
    }
}