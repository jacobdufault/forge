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

using Forge.Entities.Implementation.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Forge.Entities.Tests {
    public class CallOrderTests {
        [Fact]
        public void AddAndUpdateEntity() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates).Value;

            engine.Update().Wait();
            engine.SynchronizeState().Wait();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnAdded);
            events.Add(TriggerEvent.OnGlobalPreUpdate);
            events.Add(TriggerEvent.OnUpdate);
            events.Add(TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.Update().Wait();
                engine.SynchronizeState().Wait();

                events.Add(TriggerEvent.OnGlobalPreUpdate);
                events.Add(TriggerEvent.OnUpdate);
                events.Add(TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class AddAndModifyOnAddedSystem : BaseSystem, Trigger.Added {
            private HashSet<int> called = new HashSet<int>();

            public void OnAdded(IEntity entity) {
                if (called.Add(entity.UniqueId) == false) {
                    Console.WriteLine("Called");
                }
                entity.AddData<DataEmpty>();
                entity.Modify<DataEmpty>();
            }

            public Type[] RequiredDataTypes {
                get { return new Type[] { }; }
            }
        }

        [Fact]
        public void AddEntityAndModifyInAdd() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));
            snapshot.Systems.Add(new AddAndModifyOnAddedSystem());

            snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates).Value;

            engine.Update().Wait();
            engine.SynchronizeState().Wait();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.Update().Wait();
                engine.SynchronizeState().Wait();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class ModifyOnUpdateSystem : BaseSystem, Trigger.Update {
            public void OnUpdate(IEntity entity) {
                entity.Modify<DataEmpty>();
            }

            public Type[] RequiredDataTypes {
                get { return new Type[] { }; }
            }
        }

        [Fact]
        public void AddEntityAndModifyInUpdate() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { typeof(DataEmpty) }));
            snapshot.Systems.Add(new ModifyOnUpdateSystem());

            {
                IEntity e = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);
                e.AddData<DataEmpty>();
            }

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates).Value;

            engine.Update().Wait();
            engine.SynchronizeState().Wait();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.Update().Wait();
                engine.SynchronizeState().Wait();

                events.Add(TriggerEvent.OnModified,
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }

        [Fact]
        public void RemoveEntityWithNoData() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates).Value;

            engine.Update().Wait();
            engine.SynchronizeState().Wait();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.Update().Wait();
                engine.SynchronizeState().Wait();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }

        [Fact]
        public void RemoveEntityWithData() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
            entity.AddData<DataEmpty>();

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates).Value;

            engine.Update().Wait();
            engine.SynchronizeState().Wait();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnRemoved);
            events.Add(TriggerEvent.OnGlobalPreUpdate);
            events.Add(TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.Update().Wait();
                engine.SynchronizeState().Wait();

                events.Add(TriggerEvent.OnGlobalPreUpdate);
                events.Add(TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class ModifyOnRemovedTrigger : BaseSystem, Trigger.Removed {

            public void OnRemoved(IEntity entity) {
                entity.Modify<DataEmpty>();
            }

            public Type[] RequiredDataTypes {
                get { return new Type[] { }; }
            }
        }

        /// <summary>
        /// An entity is being removed from the engine. When systems get the OnRemoved notification,
        /// they modify the entity.
        /// </summary>
        [Fact]
        public void RemoveEntityAndModifyInRemoveNotification() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));
            snapshot.Systems.Add(new ModifyOnRemovedTrigger());

            {
                IEntity e = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Removed);
                e.AddData<DataEmpty>();
            }

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates).Value;

            engine.Update().Wait();
            engine.SynchronizeState().Wait();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.Update().Wait();
                engine.SynchronizeState().Wait();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }
    }
}