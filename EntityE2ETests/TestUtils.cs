using Forge.Entities.Implementation.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Entities.E2ETests {
    internal class TestTemplateGroup {
        public ITemplateGroup TemplateGroup = LevelManager.CreateTemplateGroup();
        public TestTemplate NewTemplate {
            get {
                return new TestTemplate(TemplateGroup.CreateTemplate());
            }
        }
    }

    internal class TestTemplate {
        public ITemplate Template;

        public TestTemplate(ITemplate template) {
            Template = template;
        }

        public TestTemplate AddData<TData>(TData data) where TData : IData {
            Template.AddDefaultData(data);
            return this;
        }
    }

    internal static class TestUtils {
        public static DataReference<TData> CreateDataReference<TData>(IQueryableEntity entity)
            where TData : IData {
            var dataReference = new DataReference<TData>();
            ((IDataReference)dataReference).Provider = entity;
            return dataReference;
        }

        /// <summary>
        /// Creates the default templates.
        /// </summary>
        /// <returns></returns>
        public static ITemplateGroup CreateDefaultTemplates() {
            TestTemplateGroup group = new TestTemplateGroup();

            ITemplate template0 = group.NewTemplate
                .AddData(new TestData0())
                .Template;

            group.NewTemplate
                .AddData(new TestData0())
                .AddData(new TestData1() { A = 90 });

            group.NewTemplate
                .AddData(new TestData0())
                .AddData(new TestData1() { A = 100 });

            group.NewTemplate
                .AddData(new TestData3() {
                    DataReference = CreateDataReference<TestData0>(template0)
                });

            return group.TemplateGroup;
        }

        public static IGameSnapshot CreateDefaultDatabase(ITemplateGroup templates) {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();

            IEntity entity;

            {
                IEntity entity0 = snapshot.CreateEntity();
                entity0.AddData<TestData0>();

                IEntity entity1 = snapshot.CreateEntity();
                entity1.AddData<TestData1>().A = 10;

                IEntity entity2 = snapshot.CreateEntity();
                entity2.AddData<TestData0>();
                entity2.AddData<TestData1>().A = 20;

                IEntity entity3 = snapshot.CreateEntity();
                entity3.AddData<TestData3>().DataReference = new DataReference<TestData0>();
                ((IDataReference)entity3.Current<TestData3>().DataReference).Provider = entity0;

                IEntity entity4 = snapshot.CreateEntity();
                entity4.AddData<TestData3>().DataReference = new DataReference<TestData0>();
                ((IDataReference)entity4.Current<TestData3>().DataReference).Provider = templates.Templates.First();
            }

            {
                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<TestData0>();

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<TestData1>().A = 30;

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 40;

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<TestData0>();

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<TestData1>().A = 50;

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 60;
            }

            {
                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
                entity.AddData<TestData0>();

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
                entity.AddData<TestData1>().A = 70;

                entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
                entity.AddData<TestData0>();
                entity.AddData<TestData1>().A = 80;
            }

            return snapshot;
        }
    }
}