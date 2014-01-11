using Forge.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Forge.Entities.Tests {
    public class EngineTests {
        [JsonObject(MemberSerialization.OptIn)]
        private class OnEngineLoadedSystem : BaseSystem, Trigger.OnEngineLoaded {
            [JsonProperty]
            public int CallCount;

            public void OnEngineLoaded(IEventDispatcher eventDispatcher) {
                ++CallCount;
            }
        }

        /// <summary>
        /// Tests that Trigger.OnEngineLoaded is called.
        /// </summary>
        /// <param name="snapshot"></param>
        /// <param name="templates"></param>
        [Theory, ClassData(typeof(SnapshotTemplateData))]
        public void CreateEngineOnEngineLoaded(IGameSnapshot snapshot, ITemplateGroup templates) {
            snapshot.Systems.Add(new OnEngineLoadedSystem());

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().Wait();

            Assert.Equal(1, engine.GetSystem<OnEngineLoadedSystem>().CallCount);
        }

        /// <summary>
        /// Tests just creating an engine instance and then updating it a bunch of times.
        /// </summary>
        [Theory, ClassData(typeof(SnapshotTemplateData))]
        public void CreateEngine(IGameSnapshot snapshot, ITemplateGroup templateGroup) {
            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templateGroup);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().Wait();
                engine.Update().Wait();
                engine.DispatchEvents();
            }
        }

        [Fact]
        public void CreateEngineUpdateNonList() {
            IGameEngine engine = GameEngineFactory.CreateEngine(LevelManager.CreateSnapshot(),
                LevelManager.CreateTemplateGroup());

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().Wait();
                engine.Update(new LinkedList<IGameInput>()).Wait();
                engine.DispatchEvents();
            }
        }
    }
}