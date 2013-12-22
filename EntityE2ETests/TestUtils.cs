using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    internal static class TestUtils {
        public static ITemplateGroup CreateDefaultTemplates() {
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

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

        public static IGameSnapshot CreateDefaultDatabase(ITemplateGroup templates) {
            IGameSnapshot database = LevelManager.CreateSnapshot();

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

                IEntity entity4 = database.CreateEntity(EntityAddTarget.Added);
                entity4.AddData<TestData3>().DataReference = new DataReference<TestData0>();
                ((IDataReference)entity4.Current<TestData3>().DataReference).Provider = templates.Templates.First();
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
    }
}