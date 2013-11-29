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

            LambdaSystem addedTrigger = new LambdaSystem(new Type[] { });
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
        public void AddEntityAndModifyInUpdate() {
            IContentDatabase contentDatabase = CreateEmptyDatabase();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { typeof(TestData0) });
            contentDatabase.Systems.Add(trigger);

            LambdaSystem modifySystem = new LambdaSystem(new Type[] { });
            modifySystem.OnUpdate = entity => {
                entity.Modify<TestData0>();
            };
            contentDatabase.Systems.Add(modifySystem);

            {
                IEntity e = ContentDatabaseHelper.CreateEntity();
                e.AddData<TestData0>();
                contentDatabase.AddedEntities.Add(e);
            }

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

        [TestMethod]
        public void RemoveEntityWithData() {
            IContentDatabase contentDatabase = CreateEmptyDatabase();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            contentDatabase.Systems.Add(trigger);

            IEntity entity = ContentDatabaseHelper.CreateEntity();
            entity.AddData<TestData0>();
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

        /// <summary>
        /// An entity is being removed from the engine. When systems get the OnRemoved notification,
        /// they modify the entity.
        /// </summary>
        [TestMethod]
        public void RemoveEntityAndModifyInRemoveNotification() {
            IContentDatabase contentDatabase = CreateEmptyDatabase();

            TriggerEventLogger trigger = new TriggerEventLogger(new Type[] { });
            contentDatabase.Systems.Add(trigger);

            LambdaSystem lambdaSystem = new LambdaSystem(new Type[] { });
            lambdaSystem.OnRemoved = entity => {
                entity.Modify<TestData0>();
            };
            contentDatabase.Systems.Add(lambdaSystem);

            {
                IEntity e = ContentDatabaseHelper.CreateEntity();
                e.AddData<TestData0>();
                contentDatabase.RemovedEntities.Add(e);
            }

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
    }
}