using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;
using System.IO;

namespace EntityTests {
    class TestData2 : GameData<TestData2> {
        public int Value;

        public override void DoCopyFrom(TestData2 source) {
            Value = source.Value;
        }

        public override bool SupportsMultipleModifications {
            get { return false; }
        }

        public override int HashCode {
            get { return Value; }
        }
    }

    class TestData3 : GameData<TestData3> {
        public int Value;

        public override void DoCopyFrom(TestData3 source) {
            Value = source.Value;
        }

        public override bool SupportsMultipleModifications {
            get { return false; }
        }

        public override int HashCode {
            get { return Value; }
        }
    }

    class AllTriggers : ITriggerAdded, ITriggerRemoved, ITriggerModified, ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate, ITriggerInput, ITriggerGlobalInput {
        public Type[] ComputeEntityFilter() {
            return new Type[] { };
        }

        public void OnAdded(IEntity entity) {
        }

        public void OnRemoved(IEntity entity) {
        }

        public void OnPassedFilter(IEntity entity) {
        }

        public void OnModified(IEntity entity) {
        }

        public void OnUpdate(IEntity entity) {
        }

        public void OnGlobalPreUpdate(IEntity singletonEntity) {
        }

        public void OnGlobalPostUpdate(IEntity singletonEntity) {
        }

        public Type IStructuredInputType {
            get { return typeof(int); }
        }

        public void OnInput(IStructuredInput input, IEntity entity) {
        }

        public void OnGlobalInput(IStructuredInput input, IEntity singletonEntity) {
        }
    }

    class CountUpdatesTrigger : ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate {
        public int Count = 0;
        public int PreCount = 0;
        public int PostCount = 0;

        public void OnUpdate(IEntity entity) {
            ++Count;
        }

        public Type[] ComputeEntityFilter() {
            return new Type[] { };
        }

        public void OnGlobalPreUpdate(IEntity singletonEntity) {
            ++PreCount;
        }

        public void OnGlobalPostUpdate(IEntity singletonEntity) {
            ++PostCount;
        }
    }

    class CountModifiesTrigger : ITriggerModified {
        public int ModifiedCount = 0;

        private Type[] _filter;

        public CountModifiesTrigger(Type[] filter) {
            _filter = filter;
        }

        public Type[] ComputeEntityFilter() {
            return _filter;
        }

        public void OnModified(IEntity entity) {
            ++ModifiedCount;
        }
    }

    public static class EntityManagerExtensions {
        public static void UpdateWorld(this EntityManager em) {
            IStructuredInput[] commands = new IStructuredInput[0];
            em.UpdateWorld(commands);
        }
    }

    [TestClass]
    public class EntityManagerTests {
        [TestMethod]
        public void Creation() {
            EntityManager em = new EntityManager(EntityFactory.Create());

            Assert.IsNotNull(em.SingletonEntity);
        }

        [TestMethod]
        public void UpdateNumber() {
            EntityManager em = new EntityManager(EntityFactory.Create());

            for (int i = 0; i < 50; ++i) {
                Assert.AreEqual(em.UpdateNumber, i);
                em.UpdateWorld();
            }
        }

        [TestMethod]
        public void AddTrigger() {
            EntityManager em = new EntityManager(EntityFactory.Create());

            CountUpdatesTrigger updates = new CountUpdatesTrigger();
            em.AddSystem(updates);
            em.AddSystem(new AllTriggers());

            em.UpdateWorld();
            Assert.AreEqual(1, updates.PreCount);
            Assert.AreEqual(1, updates.PostCount);
            Assert.AreEqual(0, updates.Count);
            updates.PreCount = 0;
            updates.PostCount = 0;

            int numEntities = 25;
            for (int i = 0; i < numEntities; ++i) {
                IEntity e = EntityFactory.Create(); 
                em.AddEntity(e);
            }

            for (int i = 1; i < 10; ++i) {
                em.UpdateWorld();
                Assert.AreEqual(i, updates.PreCount);
                Assert.AreEqual(i, updates.PostCount);
                Assert.AreEqual(i * numEntities, updates.Count);
            }
        }

        [TestMethod]
        public void AddEntity() {
            EntityManager em = new EntityManager(EntityFactory.Create());
            IEntity e = EntityFactory.Create();
            em.AddEntity(e);
        }

        [TestMethod]
        public void InitializeData() {
            EntityManager em = new EntityManager(EntityFactory.Create());

            IEntity e = EntityFactory.Create();
            em.AddEntity(e);

            {
                TestData2 data = e.AddData<TestData2>();
                data.Value = 33;
            }

            em.UpdateWorld();
            Assert.AreEqual(33, e.Current<TestData2>().Value);

            // by using ++ we ensure that the modify value is currently 33
            e.Modify<TestData2>().Value++;
            em.UpdateWorld();
            Assert.AreEqual(33, e.Previous<TestData2>().Value);
            Assert.AreEqual(34, e.Current<TestData2>().Value);
        }

        [TestMethod]
        public void InitializeDataViaModification() {
            EntityManager em = new EntityManager(EntityFactory.Create());

            IEntity e = EntityFactory.Create();
            em.AddEntity(e);

            // use modify, not the object we get from AddData, to initialize the data
            {
                TestData2 added = e.AddData<TestData2>();
                TestData2 data = e.Modify<TestData2>();
                added.Value = 32;
                data.Value = 33;
            }

            em.UpdateWorld();
            Assert.AreEqual(32, e.Previous<TestData2>().Value);
            Assert.AreEqual(33, e.Current<TestData2>().Value);

            // by using ++ we ensure that the modify value is currently 33
            e.Modify<TestData2>().Value++;
            em.UpdateWorld();
            Assert.AreEqual(33, e.Previous<TestData2>().Value);
            Assert.AreEqual(34, e.Current<TestData2>().Value);
        }

        [TestMethod]
        public void SystemModificationNotificationWithoutPassingFilter() {
            EntityManager em = new EntityManager(EntityFactory.Create());

            CountModifiesTrigger system = new CountModifiesTrigger(new Type[] {
                typeof(TestData2)
            });
            em.AddSystem(system);

            IEntity e0 = EntityFactory.Create();
            em.AddEntity(e0);
            IEntity e1 = EntityFactory.Create();
            em.AddEntity(e1);

            e0.AddData<TestData2>();
            e0.AddData<TestData3>();
            e1.AddData<TestData3>();

            em.UpdateWorld();

            e0.Modify<TestData2>();
            e1.Modify<TestData3>();
            em.UpdateWorld();
            Assert.AreEqual(1, system.ModifiedCount);
            system.ModifiedCount = 0;

            e0.Modify<TestData3>();
            e1.Modify<TestData3>();
            em.UpdateWorld();
            Assert.AreEqual(0, system.ModifiedCount);
            system.ModifiedCount = 0;
        }
    }
}
