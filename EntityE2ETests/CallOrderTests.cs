using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Neon.Entities.E2ETests {
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
        public static IGameSnapshot CreateEmptySnapshot() {
            return LevelManager.CreateLevel().CurrentState;
        }

        [TestMethod]
        public void AddAndUpdateEntity() {
            IGameSnapshot snapshot = CreateEmptySnapshot();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            snapshot.Systems.Add(trigger);

            IEntity entity = ContentDatabaseHelper.CreateEntity();
            snapshot.AddedEntities.Add(entity);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, new List<ITemplate>());

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
            IGameSnapshot snapshot = CreateEmptySnapshot();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            snapshot.Systems.Add(trigger);

            LambdaSystem addedTrigger = new LambdaSystem(new Type[] { });
            addedTrigger.OnAdded = entity => {
                entity.AddData<TestData0>();
                entity.Modify<TestData0>();
            };
            snapshot.Systems.Add(addedTrigger);

            snapshot.AddedEntities.Add(ContentDatabaseHelper.CreateEntity());

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, new List<ITemplate>());
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
        public void AddEntityAndModifyInUpdate() {
            IGameSnapshot snapshot = CreateEmptySnapshot();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { typeof(TestData0) });
            snapshot.Systems.Add(trigger);

            LambdaSystem modifySystem = new LambdaSystem(new Type[] { });
            modifySystem.OnUpdate = entity => {
                entity.Modify<TestData0>();
            };
            snapshot.Systems.Add(modifySystem);

            {
                IEntity e = ContentDatabaseHelper.CreateEntity();
                e.AddData<TestData0>();
                snapshot.AddedEntities.Add(e);
            }

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, new List<ITemplate>());
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
                    TriggerEvent.OnModified,
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate,
                }, trigger.Events);
                trigger.ClearEvents();
            }
        }

        [TestMethod]
        public void RemoveEntityWithNoData() {
            IGameSnapshot snapshot = CreateEmptySnapshot();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            snapshot.Systems.Add(trigger);

            IEntity entity = ContentDatabaseHelper.CreateEntity();
            snapshot.RemovedEntities.Add(entity);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, new List<ITemplate>());

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

        [TestMethod]
        public void RemoveEntityWithData() {
            IGameSnapshot snapshot = CreateEmptySnapshot();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            snapshot.Systems.Add(trigger);

            IEntity entity = ContentDatabaseHelper.CreateEntity();
            entity.AddData<TestData0>();
            snapshot.RemovedEntities.Add(entity);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, new List<ITemplate>());

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

        /// <summary>
        /// An entity is being removed from the engine. When systems get the OnRemoved notification,
        /// they modify the entity.
        /// </summary>
        [TestMethod]
        public void RemoveEntityAndModifyInRemoveNotification() {
            IGameSnapshot snapshot = CreateEmptySnapshot();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            snapshot.Systems.Add(trigger);

            LambdaSystem lambdaSystem = new LambdaSystem(new Type[] { });
            lambdaSystem.OnRemoved = entity => {
                entity.Modify<TestData0>();
            };
            snapshot.Systems.Add(lambdaSystem);

            {
                IEntity e = ContentDatabaseHelper.CreateEntity();
                e.AddData<TestData0>();
                snapshot.RemovedEntities.Add(e);
            }

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, new List<ITemplate>());

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
    }
}