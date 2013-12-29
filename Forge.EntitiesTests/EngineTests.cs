using Forge.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Forge.Entities.Tests {
    public class EngineTests {
        /// <summary>
        /// Tests just creating an engine instance and then updating it a bunch of times.
        /// </summary>
        [Theory, ClassData(typeof(SnapshotTemplateData))]
        public void CreateEngine(IGameSnapshot snapshot, ITemplateGroup templateGroup) {
            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templateGroup);

            for (int i = 0; i < 20; ++i) {
                engine.SynchronizeState().WaitOne();
                engine.Update().WaitOne();
                engine.DispatchEvents();
            }
        }
    }
}