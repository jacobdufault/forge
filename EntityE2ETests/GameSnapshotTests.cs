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
            return LevelManager.CreateSnapshot();
        }

        public static ITemplateGroup CreateEmptyTemplates() {
            return LevelManager.CreateTemplateGroup();
        }

        public static ITemplateGroup CreateDefaultTemplates() {
            ITemplateGroup templates = CreateEmptyTemplates();

            ITemplate template0 = templates.CreateTemplate();
            template0.AddDefaultData(new TestData0());

            ITemplate template1 = templates.CreateTemplate();
            template1.AddDefaultData(new TestData1() {
                A = 90
            });

            ITemplate template2 = templates.CreateTemplate();
            template2.AddDefaultData(new TestData0());
            template2.AddDefaultData(new TestData1() {
                A = 100
            });

            ITemplate template3 = templates.CreateTemplate();
            TestData3 data = new TestData3() {
                DataReference = new DataReference<TestData0>()
            };
            ((IDataReference)data.DataReference).Provider = template0;
            template3.AddDefaultData(data);

            return templates;
        }

        public static IGameSnapshot CreateDefaultDatabase() {
            IGameSnapshot database = CreateEmptyDatabase();

            IEntity entity;

            {
                IEntity entity0 = database.CreateEntity(EntityAddTarget.Added);
                entity0.AddData<TestData0>();

                IEntity entity1 = database.CreateEntity(EntityAddTarget.Added);
                entity1.AddData<TestData1>().A = 10;

                IEntity entity2 = database.CreateEntity(EntityAddTarget.Added);
                entity2.AddData<TestData0>();
                entity2.AddData<TestData1>().A = 20;

                IEntity entity3 = database.CreateEntity(EntityAddTarget.Added);
                entity3.AddData<TestData3>().DataReference = new DataReference<TestData0>();
                ((IDataReference)entity3.Current<TestData3>().DataReference).Provider = entity0;
            }

            {
                entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData1>().A = 30;

                entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 40;

                entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData1>().A = 50;

                entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 60;
            }

            {
                entity = database.CreateEntity(EntityAddTarget.Removed);
                entity.AddData<TestData0>();

                entity = database.CreateEntity(EntityAddTarget.Removed);
                entity.AddData<TestData1>().A = 70;

                entity = database.CreateEntity(EntityAddTarget.Removed);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 80;
            }

            return database;
        }

        [TestMethod]
        public void GotAddedEventsForInitialDatabase() {
            IGameSnapshot database = CreateDefaultDatabase();
            ITemplateGroup templates = CreateDefaultTemplates();

            IGameEngine engine = GameEngineFactory.CreateEngine(database, templates);
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
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            for (int i = 0; i < 10; ++i) {
                IEntity entity = database.CreateEntity(EntityAddTarget.Active);
                entity.AddData<TestData0>();
            }

            database.Systems.Add(new SystemCounter() {
                Filter = new[] { typeof(TestData0) }
            });

            IGameEngine engine = GameEngineFactory.CreateEngine(database, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            Assert.AreEqual(database.ActiveEntities.Count(), engine.GetSystem<SystemCounter>().UpdateCount);
        }

        [TestMethod]
        public void ToFromContentDatabase() {
            ITemplateGroup templates = CreateDefaultTemplates();

            IGameSnapshot database0 = CreateDefaultDatabase();
            IGameEngine engine0 = GameEngineFactory.CreateEngine(database0, templates);

            IGameSnapshot database1 = engine0.TakeSnapshot();
            IGameEngine engine1 = GameEngineFactory.CreateEngine(database1, templates);

            Assert.AreEqual(engine0.GetVerificationHash(), engine1.GetVerificationHash());
        }

        [TestMethod]
        public void SendRemoveFromContentDatabase() {
            IGameSnapshot database = CreateEmptyDatabase();
            ITemplateGroup templates = CreateDefaultTemplates();

            for (int i = 0; i < 10; ++i) {
                IEntity entity = database.CreateEntity(EntityAddTarget.Removed);
                entity.AddData<TestData0>();
            }

            database.Systems.Add(new SystemCounter() {
                Filter = new[] { typeof(TestData0) }
            });

            IGameEngine engine = GameEngineFactory.CreateEngine(database, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            Assert.AreEqual(database.RemovedEntities.Count(), engine.GetSystem<SystemCounter>().RemovedCount);
        }
    }
}