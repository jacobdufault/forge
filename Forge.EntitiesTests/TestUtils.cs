using Forge.Entities.Implementation.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Entities.Tests {
    internal class TestSnapshotBuilder {
        public IGameSnapshot Snapshot = LevelManager.CreateSnapshot();

        public TestEntityBuilder NewEntity() {
            return new TestEntityBuilder(Snapshot.CreateEntity());
        }

        public TestEntityBuilder NewEntity(GameSnapshot.EntityAddTarget addTarget) {
            return new TestEntityBuilder(((GameSnapshot)Snapshot).CreateEntity(addTarget));
        }
    }

    internal class TestEntityBuilder {
        public IEntity Entity;

        public TestEntityBuilder(IEntity entity) {
            Entity = entity;
        }

        public TestEntityBuilder AddData<TData>(TData data) where TData : IData {
            Entity.AddData(new DataAccessor(data)).CopyFrom(data);
            return this;
        }
    }

    internal class TestTemplateGroupBuilder {
        public ITemplateGroup TemplateGroup = LevelManager.CreateTemplateGroup();
        public TestTemplateBuilder NewTemplate {
            get {
                return new TestTemplateBuilder(TemplateGroup.CreateTemplate());
            }
        }
    }

    internal class TestTemplateBuilder {
        public ITemplate Template;

        public TestTemplateBuilder(ITemplate template) {
            Template = template;
        }

        public TestTemplateBuilder AddData<TData>(TData data) where TData : IData {
            Template.AddDefaultData(data);
            return this;
        }
    }

    internal class SnapshotTemplateData : IEnumerable<object[]> {
        private static readonly List<object[]> _data;

        static SnapshotTemplateData() {
            _data = new List<object[]>();
            foreach (ITemplateGroup templateGroup in new[]  {
                // list all template group providers here
                TemplateGroup1(),
                TemplateGroup2()
            }) {
                // list all snapshot providers here
                AddData(Snapshot1, templateGroup);
                AddData(Snapshot2, templateGroup);
                AddData(Snapshot3, templateGroup);
            }
        }

        private static void AddData(Func<ITemplateGroup, IGameSnapshot> snapshot, ITemplateGroup templateGroup) {
            _data.Add(new object[] { snapshot(templateGroup), templateGroup });
        }

        public IEnumerator<object[]> GetEnumerator() {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public static DataReference<TData> CreateDataReference<TData>(IQueryableEntity entity)
            where TData : IData {
            var dataReference = new DataReference<TData>();
            ((IDataReference)dataReference).Provider = entity;
            return dataReference;
        }

        public static ITemplateGroup TemplateGroup1() {
            return LevelManager.CreateTemplateGroup();
        }

        public static ITemplateGroup TemplateGroup2() {
            var builder = new TestTemplateGroupBuilder();

            builder.NewTemplate
                .AddData(new DataEmpty());

            ITemplate template1 = builder.NewTemplate
                .AddData(new DataEmpty())
                .AddData(new DataInt() { A = 90 })
                .Template;

            builder.NewTemplate
                .AddData(new DataEmpty())
                .AddData(new DataInt() { A = 100 });

            builder.NewTemplate
                .AddData(new DataTemplate() { Template = template1 });

            builder.NewTemplate
                .AddData(new DataDataReference() {
                    DataReference = CreateDataReference<DataEmpty>(template1)
                });

            return builder.TemplateGroup;
        }

        public static IGameSnapshot Snapshot1(ITemplateGroup templates) {
            return LevelManager.CreateSnapshot();
        }

        public static IGameSnapshot Snapshot2(ITemplateGroup templates) {
            TestSnapshotBuilder builder = new TestSnapshotBuilder();

            {
                builder.NewEntity();

                builder.NewEntity()
                    .AddData(new DataEmpty());

                IEntity entity1 = builder.NewEntity()
                    .AddData(new DataInt() { A = 10 })
                    .Entity;

                builder.NewEntity()
                    .AddData(new DataEmpty())
                    .AddData(new DataInt() { A = 20 });

                IEntity entity3 = builder.NewEntity()
                    .AddData(new DataDataReference() { DataReference = CreateDataReference<DataEmpty>(entity1) })
                    .Entity;
            }

            {
                builder.NewEntity(GameSnapshot.EntityAddTarget.Active);

                for (int i = 0; i < 5; ++i) {
                    builder.NewEntity(GameSnapshot.EntityAddTarget.Active)
                        .AddData(new DataEmpty());

                    builder.NewEntity(GameSnapshot.EntityAddTarget.Active)
                        .AddData(new DataInt { A = 30 });

                    builder.NewEntity(GameSnapshot.EntityAddTarget.Active)
                        .AddData(new DataEmpty())
                        .AddData(new DataInt { A = 40 });
                }
            }

            {
                builder.NewEntity(GameSnapshot.EntityAddTarget.Removed)
                    .AddData(new DataEmpty());

                builder.NewEntity(GameSnapshot.EntityAddTarget.Removed)
                    .AddData(new DataInt { A = 70 });

                builder.NewEntity(GameSnapshot.EntityAddTarget.Removed)
                    .AddData(new DataEmpty())
                    .AddData(new DataInt() { A = 80 });
            }

            return builder.Snapshot;
        }

        public static IGameSnapshot Snapshot3(ITemplateGroup templates) {
            var builder = new TestSnapshotBuilder();

            {
                builder.NewEntity()
                    .AddData(new DataEmpty());

                IEntity entity1 = builder.NewEntity()
                    .AddData(new DataInt() { A = 10 })
                    .Entity;

                builder.NewEntity()
                    .AddData(new DataEmpty())
                    .AddData(new DataInt() { A = 20 });

                IEntity entity3 = builder.NewEntity()
                    .AddData(new DataDataReference() { DataReference = CreateDataReference<DataEmpty>(entity1) })
                    .Entity;

                builder.NewEntity()
                    .AddData(new DataEntity() { Entity = entity3 });

                IEntity entity5 = builder.Snapshot.CreateEntity();
                entity5.AddData<DataEntity>().Entity = entity5;

                builder.NewEntity()
                    .AddData(new DataQueryableEntity() { QueryableEntity = entity5 });

                IEntity entity7 = builder.Snapshot.CreateEntity();
                entity7.AddData<DataQueryableEntity>().QueryableEntity = entity7;
            }

            if (templates.Templates.Count() > 0) {
                ITemplate template = templates.Templates.First();

                builder.NewEntity()
                    .AddData(new DataDataReference() { DataReference = CreateDataReference<DataEmpty>(template) });

                builder.NewEntity()
                    .AddData(new DataQueryableEntity() { QueryableEntity = template });
            }

            return builder.Snapshot;
        }
    }
}