using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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

    [JsonObject(MemberSerialization.OptIn)]
    internal class TriggerEventLogger : ITriggerAdded, ITriggerRemoved, ITriggerModified, ITriggerUpdate, ITriggerGlobalPreUpdate, ITriggerGlobalPostUpdate, ITriggerInput, ITriggerGlobalInput {
        public Type[] ComputeEntityFilter() {
            return EntityFilter;
        }

        [JsonProperty("EntityFilter")]
        public Type[] EntityFilter;

        public TriggerEventLogger() {
            EntityFilter = new Type[0];
        }

        public TriggerEventLogger(Type[] entityFilter) {
            EntityFilter = entityFilter;
        }

        [JsonProperty("Events")]
        public List<TriggerEvent> Events = new List<TriggerEvent>();

        public void ClearEvents() {
            Events.Clear();
        }

        public void OnAdded(IEntity entity) {
            Events.Add(TriggerEvent.OnAdded);
        }

        public void OnRemoved(IEntity entity) {
            Events.Add(TriggerEvent.OnRemoved);
        }

        public void OnModified(IEntity entity) {
            Events.Add(TriggerEvent.OnModified);
        }

        public void OnUpdate(IEntity entity) {
            Events.Add(TriggerEvent.OnUpdate);
        }

        public void OnGlobalPreUpdate(IEntity singletonEntity) {
            Events.Add(TriggerEvent.OnGlobalPreUpdate);
        }

        public void OnGlobalPostUpdate(IEntity singletonEntity) {
            Events.Add(TriggerEvent.OnGlobalPostUpdate);
        }

        public Type IStructuredInputType {
            get { return typeof(int); }
        }

        public void OnInput(IGameInput input, IEntity entity) {
            Events.Add(TriggerEvent.OnInput);
        }

        public void OnGlobalInput(IGameInput input, IEntity singletonEntity) {
            Events.Add(TriggerEvent.OnGlobalInput);
        }
    }

    public static class IGameEngineExtensions {
        public static TSystem GetSystem<TSystem>(this IGameEngine engine) where TSystem : ISystem {
            foreach (ISystem system in engine.TakeSnapshot().Systems) {
                if (system is TSystem) {
                    return (TSystem)system;
                }
            }

            throw new Exception("No system of type " + typeof(TSystem) + " is in the engine");
        }

        public static void Add<T>(this IList<T> list, params T[] elements) {
            foreach (var element in elements) {
                list.Add(element);
            }
        }
    }

    [TestClass]
    public class CallOrderTests {
        public static IGameSnapshot CreateEmptySnapshot() {
            return LevelManager.CreateSnapshot();
        }

        [TestMethod]
        public void AddAndUpdateEntity() {
            IGameSnapshot snapshot = CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(EntityAddTarget.Added);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnAdded);
            events.Add(TriggerEvent.OnGlobalPreUpdate);
            events.Add(TriggerEvent.OnUpdate);
            events.Add(TriggerEvent.OnGlobalPostUpdate);
            CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                events.Add(TriggerEvent.OnGlobalPreUpdate);
                events.Add(TriggerEvent.OnUpdate);
                events.Add(TriggerEvent.OnGlobalPostUpdate);
                CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class AddAndModifyOnAddedSystem : ITriggerAdded {
            public void OnAdded(IEntity entity) {
                entity.AddData<TestData0>();
                entity.Modify<TestData0>();
            }

            public Type[] ComputeEntityFilter() {
                return new Type[] { };
            }
        }

        [TestMethod]
        public void AddEntityAndModifyInAdd() {
            IGameSnapshot snapshot = CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));
            snapshot.Systems.Add(new AddAndModifyOnAddedSystem());

            snapshot.CreateEntity(EntityAddTarget.Added);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class ModifyOnUpdateSystem : ITriggerUpdate {
            public void OnUpdate(IEntity entity) {
                entity.Modify<TestData0>();
            }

            public Type[] ComputeEntityFilter() {
                return new Type[] { };
            }
        }

        [TestMethod]
        public void AddEntityAndModifyInUpdate() {
            IGameSnapshot snapshot = CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { typeof(TestData0) }));
            snapshot.Systems.Add(new ModifyOnUpdateSystem());

            {
                IEntity e = snapshot.CreateEntity(EntityAddTarget.Added);
                e.AddData<TestData0>();
            }

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                events.Add(TriggerEvent.OnModified,
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);
            }
        }

        [TestMethod]
        public void RemoveEntityWithNoData() {
            IGameSnapshot snapshot = CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(EntityAddTarget.Removed);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);
            }
        }

        [TestMethod]
        public void RemoveEntityWithData() {
            IGameSnapshot snapshot = CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(EntityAddTarget.Removed);
            entity.AddData<TestData0>();

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnRemoved);
            events.Add(TriggerEvent.OnGlobalPreUpdate);
            events.Add(TriggerEvent.OnGlobalPostUpdate);
            CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                events.Add(TriggerEvent.OnGlobalPreUpdate);
                events.Add(TriggerEvent.OnGlobalPostUpdate);
                CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class ModifyOnRemovedTrigger : ITriggerRemoved {

            public void OnRemoved(IEntity entity) {
                entity.Modify<TestData0>();
            }

            public Type[] ComputeEntityFilter() {
                return new Type[] { };
            }
        }

        /// <summary>
        /// An entity is being removed from the engine. When systems get the OnRemoved notification,
        /// they modify the entity.
        /// </summary>
        [TestMethod]
        public void RemoveEntityAndModifyInRemoveNotification() {
            IGameSnapshot snapshot = CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));
            snapshot.Systems.Add(new ModifyOnRemovedTrigger());

            {
                IEntity e = snapshot.CreateEntity(EntityAddTarget.Removed);
                e.AddData<TestData0>();
            }

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                CollectionAssert.AreEqual(events, engine.GetSystem<TriggerEventLogger>().Events);
            }
        }
    }
}