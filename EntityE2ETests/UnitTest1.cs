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
    public class UnitTest1 {
        public static IContentDatabase CreateDefaultDatabase() {
            IContentDatabase database = LevelManager.CreateLevel().CurrentState;

            IEntity entity;

            entity = database.AddEntity();
            entity.AddData<TestData0>();

            entity = database.AddEntity();
            entity.AddData<TestData1>().A = 10;

            entity = database.AddEntity();
            entity.AddData<TestData0>();
            entity.AddData<TestData1>().A = 20;

            return database;
        }

        [TestMethod]
        public void TestAddedNotifications() {
            IContentDatabase database = CreateDefaultDatabase();

            IGameEngine engine = GameEngineFactory.CreateEngine(database);
            engine.Update();

            int notifiedCount = 0;
            engine.EventNotifier.AddListener<EntityAddedEvent>(evnt => {
                ++notifiedCount;
            });

            engine.DispatchEvents();

            Assert.AreEqual(database.AddedEntities.Count, notifiedCount);
        }
    }
}