using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;
using System;
using System.Collections.Generic;

namespace Neon.Entities.E2ETests {
    public static class GameEngineExtensions {
        public static void Update(this IGameEngine engine) {
            engine.Update(new List<IGameInput>()).WaitOne();
        }
    }

    [TestClass]
    public class ContentDatabaseTests {
        public static IContentDatabase CreateEmptyDatabase() {
            return LevelManager.CreateLevel().CurrentState;
        }

        public static IContentDatabase CreateDefaultDatabase() {
            IContentDatabase database = CreateEmptyDatabase();

            IEntity entity;

            {
                entity = ContentDatabaseHelper.CreateEntity();
                database.AddedEntities.Add(entity);
                entity.AddData<TestData0>();

                entity = ContentDatabaseHelper.CreateEntity();
                database.AddedEntities.Add(entity);
                entity.AddData<TestData1>().A = 10;

                entity = ContentDatabaseHelper.CreateEntity();
                database.AddedEntities.Add(entity);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 20;
            }

            {
                entity = ContentDatabaseHelper.CreateEntity();
                database.ActiveEntities.Add(entity);
                entity.AddData<TestData0>();

                entity = ContentDatabaseHelper.CreateEntity();
                database.ActiveEntities.Add(entity);
                entity.AddData<TestData1>().A = 30;

                entity = ContentDatabaseHelper.CreateEntity();
                database.ActiveEntities.Add(entity);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 40;

                entity = ContentDatabaseHelper.CreateEntity();
                database.ActiveEntities.Add(entity);
                entity.AddData<TestData0>();

                entity = ContentDatabaseHelper.CreateEntity();
                database.ActiveEntities.Add(entity);
                entity.AddData<TestData1>().A = 50;

                entity = ContentDatabaseHelper.CreateEntity();
                database.ActiveEntities.Add(entity);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 60;
            }

            {
                entity = ContentDatabaseHelper.CreateEntity();
                database.RemovedEntities.Add(entity);
                entity.AddData<TestData0>();

                entity = ContentDatabaseHelper.CreateEntity();
                database.RemovedEntities.Add(entity);
                entity.AddData<TestData1>().A = 70;

                entity = ContentDatabaseHelper.CreateEntity();
                database.RemovedEntities.Add(entity);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 80;
            }

            return database;
        }

        [TestMethod]
        public void GotAddedEventsForInitialDatabase() {
            IContentDatabase database = CreateDefaultDatabase();

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            int notifiedCount = 0;
            engine.EventNotifier.OnEvent<EntityAddedEvent>(evnt => {
                ++notifiedCount;
            });

            engine.DispatchEvents();

            Assert.AreEqual(database.AddedEntities.Count + database.ActiveEntities.Count +
                database.RemovedEntities.Count, notifiedCount);
        }

        [TestMethod]
        public void CorrectUpdateCount() {
            IContentDatabase database = CreateEmptyDatabase();
            for (int i = 0; i < 10; ++i) {
                IEntity entity = ContentDatabaseHelper.CreateEntity();
                entity.AddData<TestData0>();
                database.ActiveEntities.Add(entity);
            }

            SystemCounter system = new SystemCounter() {
                Filter = new[] { typeof(TestData0) }
            };
            database.Systems.Add(system);

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            Assert.AreEqual(database.ActiveEntities.Count, system.UpdateCount);
        }

        [TestMethod]
        public void ToFromContentDatabase() {
            IContentDatabase database0 = CreateDefaultDatabase();
            IGameEngine engine0 = GameEngineFactory.CreateEngine(database0);

            IContentDatabase database1 = engine0.GetContentDatabase();
            IGameEngine engine1 = GameEngineFactory.CreateEngine(database1);

            Assert.AreEqual(engine0.GetVerificationHash(), engine1.GetVerificationHash());
        }

        [TestMethod]
        public void SendRemoveFromContentDatabase() {
            IContentDatabase database = CreateEmptyDatabase();
            for (int i = 0; i < 10; ++i) {
                IEntity entity = ContentDatabaseHelper.CreateEntity();
                entity.AddData<TestData0>();
                database.RemovedEntities.Add(entity);
            }

            SystemCounter system = new SystemCounter() {
                Filter = new[] { typeof(TestData0) }
            };
            database.Systems.Add(system);

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            Assert.AreEqual(database.RemovedEntities.Count, system.RemovedCount);
        }
    }
}