using Forge.Entities.Implementation.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Forge.Entities.Tests {
    public class CallOrderTests {
        [Fact]
        public void AddAndUpdateEntity() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));

            IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnAdded);
            events.Add(TriggerEvent.OnGlobalPreUpdate);
            events.Add(TriggerEvent.OnUpdate);
            events.Add(TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();

                events.Add(TriggerEvent.OnGlobalPreUpdate);
                events.Add(TriggerEvent.OnUpdate);
                events.Add(TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class AddAndModifyOnAddedSystem : BaseSystem, Trigger.Added {
            public void OnAdded(IEntity entity) {
                entity.AddData<DataEmpty>();
                entity.Modify<DataEmpty>();
            }

            public Type[] RequiredDataTypes() {
                return new Type[] { };
            }
        }

        [Fact]
        public void AddEntityAndModifyInAdd() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            snapshot.Systems.Add(new SystemEventLogger(new Type[] { }));
            snapshot.Systems.Add(new AddAndModifyOnAddedSystem());

            snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Added);

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();

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

            public Type[] RequiredDataTypes() {
                return new Type[] { };
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

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnAdded,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();

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

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();

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

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(TriggerEvent.OnRemoved);
            events.Add(TriggerEvent.OnGlobalPreUpdate);
            events.Add(TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();

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

            public Type[] RequiredDataTypes() {
                return new Type[] { };
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

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);

            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            List<TriggerEvent> events = new List<TriggerEvent>();
            events.Add(
                TriggerEvent.OnRemoved,
                TriggerEvent.OnGlobalPreUpdate,
                TriggerEvent.OnGlobalPostUpdate);
            Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();

                events.Add(
                    TriggerEvent.OnGlobalPreUpdate,
                    TriggerEvent.OnGlobalPostUpdate);
                Assert.Equal(events, engine.GetSystem<SystemEventLogger>().Events);
            }
        }
    }
}