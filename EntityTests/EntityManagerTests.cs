using LitJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities;
using System;
using System.Collections.Generic;

namespace EntityTests {
    internal class TestData2 : GameData<TestData2> {
        public int Value;

        public override void DoCopyFrom(TestData2 source) {
            Value = source.Value;
        }

        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override int HashCode {
            get { return Value; }
        }
    }

    internal class TestData3 : GameData<TestData3> {
        public int Value;

        public override void DoCopyFrom(TestData3 source) {
            Value = source.Value;
        }

        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override int HashCode {
            get { return Value; }
        }
    }

    internal class DelegateAllTriggers : ITriggerAdded, ITriggerRemoved, ITriggerModified, ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate, ITriggerInput, ITriggerGlobalInput {
        public Type[] EntityFilter = new Type[] { };
        public Type[] ComputeEntityFilter() {
            return EntityFilter;
        }

        public event Action<IEntity> Added;
        public void OnAdded(IEntity entity) {
            if (Added != null) Added(entity);
        }

        public event Action<IEntity> Removed;
        public void OnRemoved(IEntity entity) {
            if (Removed != null) Removed(entity);
        }

        public event Action<IEntity> Modified;
        public void OnModified(IEntity entity) {
            if (Modified != null) Modified(entity);
        }

        public event Action<IEntity> Update;
        public void OnUpdate(IEntity entity) {
            if (Update != null) Update(entity);
        }

        public event Action<IEntity> GlobalPreUpdate;
        public void OnGlobalPreUpdate(IEntity singletonEntity) {
            if (GlobalPreUpdate != null) GlobalPreUpdate(singletonEntity);
        }

        public event Action<IEntity> GlobalPostUpdate;
        public void OnGlobalPostUpdate(IEntity singletonEntity) {
            if (GlobalPostUpdate != null) GlobalPostUpdate(singletonEntity);
        }

        public Type StructedInputType = typeof(int);
        public Type IStructuredInputType {
            get { return StructedInputType; }
        }

        public event Action<IStructuredInput, IEntity> Input;
        public void OnInput(IStructuredInput input, IEntity entity) {
            if (Input != null) Input(input, entity);
        }

        public event Action<IStructuredInput, IEntity> GlobalInput;
        public void OnGlobalInput(IStructuredInput input, IEntity singletonEntity) {
            if (GlobalInput != null) GlobalInput(input, singletonEntity);
        }

        public string RestorationGUID {
            get { return "fbaaedd2c5b94a9d919bf3c72f035641"; }
        }

        public JsonData Save() {
            return new JsonData();
        }


        public void Restore(JsonData data) {
        }
    }

    internal class CountUpdatesTrigger : ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate {
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

        public string RestorationGUID {
            get { return "ecbdc58543cf4d44923e7706a933aab1"; }
        }

        public JsonData Save() {
            return new JsonData();
        }

        public void Restore(JsonData data) {
        }
    }

    internal class CountModifiesTrigger : ITriggerModified {
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

        public string RestorationGUID {
            get { return "6b901ea1c9ce4e7a9a8d32ca25e383d8"; }
        }

        public JsonData Save() {
            return new JsonData();
        }


        public void Restore(JsonData data) {
        }
    }

    public static class EntityManagerExtensions {
        public static void UpdateWorld(this EntityManager em) {
            List<IStructuredInput> commands = new List<IStructuredInput>();
            em.UpdateWorld(commands).Wait();
        }
    }

    [TestClass]
    public class EntityManagerTests {
        [TestMethod]
        public void Creation() {
            EntityManager em = new EntityManager(new Entity());

            Assert.IsNotNull(em.SingletonEntity);
        }

        [TestMethod]
        public void UpdateNumber() {
            EntityManager em = new EntityManager(new Entity());

            for (int i = 0; i < 50; ++i) {
                Assert.AreEqual(em.UpdateNumber, i);
                em.UpdateWorld();
            }
        }

        [TestMethod]
        public void AddTrigger() {
            EntityManager em = new EntityManager(new Entity());

            CountUpdatesTrigger updates = new CountUpdatesTrigger();
            em.AddSystem(updates);
            em.AddSystem(new DelegateAllTriggers());

            em.UpdateWorld();
            Assert.AreEqual(1, updates.PreCount);
            Assert.AreEqual(1, updates.PostCount);
            Assert.AreEqual(0, updates.Count);
            updates.PreCount = 0;
            updates.PostCount = 0;

            int numEntities = 25;
            for (int i = 0; i < numEntities; ++i) {
                IEntity e = new Entity();
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
            EntityManager em = new EntityManager(new Entity());
            IEntity e = new Entity();
            em.AddEntity(e);
        }

        [TestMethod]
        public void InitializeData() {
            EntityManager em = new EntityManager(new Entity());

            IEntity e = new Entity();
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
            EntityManager em = new EntityManager(new Entity());

            IEntity e = new Entity();
            em.AddEntity(e);

            // use modify, not the object we get from AddData, to initialize the data
            {
                TestData2 added = e.AddData<TestData2>();
                TestData2 modified = e.Modify<TestData2>();
                added.Value = 32;
                modified.Value = 33;
            }

            em.UpdateWorld();
            Assert.AreEqual(33, e.Previous<TestData2>().Value);
            Assert.AreEqual(33, e.Current<TestData2>().Value);

            // by using ++ we ensure that the modify value is currently 33
            e.Modify<TestData2>().Value++;
            em.UpdateWorld();
            Assert.AreEqual(33, e.Previous<TestData2>().Value);
            Assert.AreEqual(34, e.Current<TestData2>().Value);
        }

        [TestMethod]
        public void SystemModificationNotificationWithoutPassingFilter() {
            EntityManager em = new EntityManager(new Entity());

            CountModifiesTrigger system = new CountModifiesTrigger(new Type[] {
                typeof(TestData2)
            });
            em.AddSystem(system);

            IEntity e0 = new Entity();
            em.AddEntity(e0);
            IEntity e1 = new Entity();
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

        [TestMethod]
        public void ModifiedTrigger() {
            EntityManager em = new EntityManager(new Entity());

            DelegateAllTriggers system = new DelegateAllTriggers();
            em.AddSystem(system);

            int modifiedCount = 0;
            system.Modified += modified => {
                Assert.AreEqual(modifiedCount++, modified.Current<TestData2>().Value);
            };

            IEntity entity = new Entity();
            entity.AddData<TestData2>();
            em.AddEntity(entity);
            em.UpdateWorld();

            for (int i = 0; i < 20; ++i) {
                entity.Modify<TestData2>().Value++;
                em.UpdateWorld();
                Assert.IsTrue(entity.WasModified<TestData2>());
                Assert.AreEqual(entity.Previous<TestData2>().Value, i);
                Assert.AreEqual(entity.Current<TestData2>().Value, i + 1);
            }
        }

        [TestMethod]
        public void AddAndRemoveInSequentialFrames() {
            // setup
            EntityManager em = new EntityManager(new Entity());
            IEntity entity = new Entity();
            entity.AddData<TestData2>();
            em.AddEntity(entity);
            em.UpdateWorld();

            // remove in frame 1
            entity.RemoveData<TestData2>();
            em.UpdateWorld();

            // add in next frame
            entity.AddData<TestData2>();
            em.UpdateWorld();

            // make sure it stays true
            for (int i = 0; i < 10; ++i) {
                Assert.IsTrue(entity.ContainsData<TestData2>());
                entity.Current<TestData2>();
                entity.Modify<TestData2>();
                em.UpdateWorld();
            }
        }

        // TODO: renable this test
        //[TestMethod]
        public void RemoveAndModifySameFrame() {
            // setup
            EntityManager em = new EntityManager(new Entity());
            IEntity entity = new Entity();
            entity.AddData<TestData2>();
            em.AddEntity(entity);
            em.UpdateWorld();

            // modify & remove in the same frame
            entity.RemoveData<TestData2>();
            entity.Modify<TestData2>().Value = 2;
            em.UpdateWorld();

            // make sure the modification didn't apply itself
            Assert.AreEqual(0, entity.Current<TestData2>().Value);
        }

        [TestMethod]
        public void UpdateTriggerCalled() {
            // setup
            EntityManager em = new EntityManager(new Entity());

            DelegateAllTriggers system = new DelegateAllTriggers();
            system.EntityFilter = new[] { typeof(TestData2) };
            int updateCount = 0;
            system.Update += e => {
                ++updateCount;
            };
            em.AddSystem(system);

            IEntity entity = new Entity();
            entity.AddData<TestData2>();
            em.AddEntity(entity);

            for (int i = 0; i < 20; ++i) {
                Assert.AreEqual(i, updateCount);
                em.UpdateWorld();
            }
        }

        [TestMethod]
        public void DelayedAddToSystem() {
             // setup
            EntityManager em = new EntityManager(new Entity());

            DelegateAllTriggers system = new DelegateAllTriggers();
            system.EntityFilter = new[] { typeof(TestData2) };
            int addedCount = 0;
            system.Added += e => ++addedCount;
            int modifiedCount = 0;
            system.Modified += e => ++modifiedCount;
            em.AddSystem(system);

            IEntity entity = new Entity();
            em.AddEntity(entity);
            entity.AddData<TestData0>();

            // update the world a couple of times
            for (int i = 0; i < 20; ++i) {
                em.UpdateWorld();                
            }

            DataAllocator.AddAuxiliaryAllocator<TestData2>((data, ent) => {
                try {
                    ent.AddData<TestData2>();
                    Assert.Fail();
                }
                catch (AlreadyAddedDataException) {}
            });
            entity.AddData<TestData2>();
            Assert.AreEqual(0, addedCount);

            for (int i = 0; i < 20; ++i) {
                em.UpdateWorld();
                Assert.AreEqual(1, addedCount);
            }

            // make sure modification dispatch works
            Assert.AreEqual(0, modifiedCount);
            entity.Modify<TestData2>();
            Assert.AreEqual(0, modifiedCount);
            em.UpdateWorld();
            Assert.AreEqual(1, modifiedCount);
        }
    }
}