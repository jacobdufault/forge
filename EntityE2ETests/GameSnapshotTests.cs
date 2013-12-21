using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Entities.E2ETests {
    public static class GameEngineExtensions {
        public static void Update(this IGameEngine engine) {
            engine.Update(new List<IGameInput>()).WaitOne();
        }
    }

    [TestClass]
    public class GameSnapshotTests {
        public static IGameSnapshot CreateEmptyDatabase() {
            return LevelManager.CreateLevel().CurrentState;
        }

        public static IGameSnapshot CreateDefaultDatabase() {
            IGameSnapshot database = CreateEmptyDatabase();

            IEntity entity;

            {
                entity = database.CreateEntity(EntityAddLocation.Added);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddLocation.Added);
                entity.AddData<TestData1>().A = 10;

                entity = database.CreateEntity(EntityAddLocation.Added);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 20;
            }

            {
                entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData1>().A = 30;

                entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 40;

                entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData1>().A = 50;

                entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 60;
            }

            {
                entity = database.CreateEntity(EntityAddLocation.Removed);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddLocation.Removed);
                entity.AddData<TestData1>().A = 70;

                entity = database.CreateEntity(EntityAddLocation.Removed);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 80;
            }

            {
                ITemplate template;
                template = database.CreateTemplate();

                template = database.CreateTemplate();
                template.AddDefaultData(new TestData0());

                template = database.CreateTemplate();
                template.AddDefaultData(new TestData1() {
                    A = 90
                });

                template = database.CreateTemplate();
                template.AddDefaultData(new TestData0());
                template.AddDefaultData(new TestData1() {
                    A = 100
                });

                entity = database.CreateEntity(EntityAddLocation.Removed);
                entity.AddData<TestData2>().Template = template;
            }

            return database;
        }

        [TestMethod]
        public void GotAddedEventsForInitialDatabase() {
            IGameSnapshot database = CreateDefaultDatabase();

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            int notifiedCount = 0;
            engine.EventNotifier.OnEvent<EntityAddedEvent>(evnt => {
                ++notifiedCount;
            });

            engine.DispatchEvents();

            Assert.AreEqual(database.AddedEntities.Count() + database.ActiveEntities.Count() +
                database.RemovedEntities.Count(), notifiedCount);
        }

        [TestMethod]
        public void CorrectUpdateCount() {
            IGameSnapshot database = CreateEmptyDatabase();
            for (int i = 0; i < 10; ++i) {
                IEntity entity = database.CreateEntity(EntityAddLocation.Active);
                entity.AddData<TestData0>();
            }

            database.Systems.Add(new SystemCounter() {
                Filter = new[] { typeof(TestData0) }
            });

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            Assert.AreEqual(database.ActiveEntities.Count(), engine.GetSystem<SystemCounter>().UpdateCount);
        }

        [TestMethod]
        public void ToFromContentDatabase() {
            IGameSnapshot database0 = CreateDefaultDatabase();
            IGameEngine engine0 = GameEngineFactory.CreateEngine(database0);

            IGameSnapshot database1 = engine0.TakeSnapshot();
            IGameEngine engine1 = GameEngineFactory.CreateEngine(database1);

            Assert.AreEqual(engine0.GetVerificationHash(), engine1.GetVerificationHash());
        }

        [TestMethod]
        public void SendRemoveFromContentDatabase() {
            IGameSnapshot database = CreateEmptyDatabase();
            for (int i = 0; i < 10; ++i) {
                IEntity entity = database.CreateEntity(EntityAddLocation.Removed);
                entity.AddData<TestData0>();
            }

            database.Systems.Add(new SystemCounter() {
                Filter = new[] { typeof(TestData0) }
            });

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            Assert.AreEqual(database.RemovedEntities.Count(), engine.GetSystem<SystemCounter>().RemovedCount);
        }
    }
}