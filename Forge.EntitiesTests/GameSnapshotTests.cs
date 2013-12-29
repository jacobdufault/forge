using Forge.Entities.Implementation.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace Forge.Entities.Tests {
    public class GameSnapshotTests {

        [Theory, ClassData(typeof(SnapshotTemplateData))]
        public void GotAddedEventsForInitialDatabase(IGameSnapshot snapshot, ITemplateGroup templates) {
            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            int notifiedCount = 0;
            engine.EventNotifier.OnEvent<EntityAddedEvent>(evnt => {
                ++notifiedCount;
            });

            engine.DispatchEvents();

            Assert.Equal(1 + snapshot.AddedEntities.Count() + snapshot.ActiveEntities.Count() +
                snapshot.RemovedEntities.Count(), notifiedCount);

            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            notifiedCount = 0;
            engine.DispatchEvents();
            Assert.Equal(0, notifiedCount);
        }

        [Fact]
        public void CorrectUpdateCount() {
            GameSnapshot snapshot = (GameSnapshot)LevelManager.CreateSnapshot();
            ITemplateGroup templates = LevelManager.CreateTemplateGroup();

            for (int i = 0; i < 10; ++i) {
                IEntity entity = snapshot.CreateEntity(GameSnapshot.EntityAddTarget.Active);
                entity.AddData<DataEmpty>();
            }

            snapshot.Systems.Add(new SystemCounter() {
                Filter = new[] { typeof(DataEmpty) }
            });

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            Assert.Equal(snapshot.ActiveEntities.Count(), engine.GetSystem<SystemCounter>().UpdateCount);
        }

        [Theory, ClassData(typeof(SnapshotTemplateData))]
        public void ToFromContentDatabase(IGameSnapshot snapshot0, ITemplateGroup templates) {
            IGameEngine engine0 = GameEngineFactory.CreateEngine(snapshot0, templates);

            IGameSnapshot snapshot1 = engine0.TakeSnapshot();
            IGameEngine engine1 = GameEngineFactory.CreateEngine(snapshot1, templates);

            Assert.Equal(engine0.GetVerificationHash(), engine1.GetVerificationHash());
        }

        [Theory, ClassData(typeof(SnapshotTemplateData))]
        public void SendRemoveFromContentDatabase(IGameSnapshot snapshot, ITemplateGroup templates) {
            snapshot.Systems.Add(new SystemCounter() {
                Filter = new Type[] { }
            });

            IGameEngine engine = GameEngineFactory.CreateEngine(snapshot, templates);
            engine.SynchronizeState().WaitOne();
            engine.Update().WaitOne();

            Assert.Equal(snapshot.RemovedEntities.Count(), engine.GetSystem<SystemCounter>().RemovedCount);
        }
    }

}