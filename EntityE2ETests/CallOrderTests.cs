// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities.Implementation.Content;
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
    internal class TriggerEventLogger : BaseSystem, Trigger.Added, Trigger.Removed, Trigger.Modified, Trigger.Update, Trigger.GlobalPreUpdate, Trigger.GlobalPostUpdate, Trigger.Input, Trigger.GlobalInput {
        public Type[] RequiredDataTypes() {
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

        public Type[] InputTypes {
            get { return new Type[] { typeof(int) }; }
        }

        public void OnInput(IGameInput input, IEntity entity) {
            Events.Add(TriggerEvent.OnInput);
        }

        public void OnGlobalInput(IGameInput input, IEntity singletonEntity) {
            Events.Add(TriggerEvent.OnGlobalInput);
        }
    }

    public static class IGameEngineExtensions {
        public static TSystem GetSystem<TSystem>(this IGameEngine engine) where TSystem : BaseSystem {
            foreach (BaseSystem system in engine.TakeSnapshot().Systems) {
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
            GameSnapshot snapshot = (GameSnapshot)CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);

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
        private class AddAndModifyOnAddedSystem : BaseSystem, Trigger.Added {
            public void OnAdded(IEntity entity) {
                entity.AddData<TestData0>();
                entity.Modify<TestData0>();
            }

            public Type[] RequiredDataTypes() {
                return new Type[] { };
            }
        }

        [TestMethod]
        public void AddEntityAndModifyInAdd() {
            GameSnapshot snapshot = (GameSnapshot)CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));
            snapshot.Systems.Add(new AddAndModifyOnAddedSystem());

            snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);

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
        private class ModifyOnUpdateSystem : BaseSystem, Trigger.Update {
            public void OnUpdate(IEntity entity) {
                entity.Modify<TestData0>();
            }

            public Type[] RequiredDataTypes() {
                return new Type[] { };
            }
        }

        [TestMethod]
        public void AddEntityAndModifyInUpdate() {
            GameSnapshot snapshot = (GameSnapshot)CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { typeof(TestData0) }));
            snapshot.Systems.Add(new ModifyOnUpdateSystem());

            {
                IEntity e = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);
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
            GameSnapshot snapshot = (GameSnapshot)CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);

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
            GameSnapshot snapshot = (GameSnapshot)CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
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
        private class ModifyOnRemovedTrigger : BaseSystem, Trigger.Removed {

            public void OnRemoved(IEntity entity) {
                entity.Modify<TestData0>();
            }

            public Type[] RequiredDataTypes() {
                return new Type[] { };
            }
        }

        /// <summary>
        /// An entity is being removed from the engine. When systems get the OnRemoved notification,
        /// they modify the entity.
        /// </summary>
        [TestMethod]
        public void RemoveEntityAndModifyInRemoveNotification() {
            GameSnapshot snapshot = (GameSnapshot)CreateEmptySnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new TriggerEventLogger(new Type[] { }));
            snapshot.Systems.Add(new ModifyOnRemovedTrigger());

            {
                IEntity e = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
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