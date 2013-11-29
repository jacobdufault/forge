using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.E2ETests {
    internal class LambdaSystem : ITriggerAdded, ITriggerRemoved, ITriggerUpdate {
        public Type[] ComputeEntityFilter() {
            return EntityFilter;
        }

        public Type[] EntityFilter;

        public LambdaSystem(Type[] entityFilter) {
            EntityFilter = entityFilter;
        }

        public Action<IEntity> OnAdded;
        void ITriggerAdded.OnAdded(IEntity entity) {
            if (OnAdded != null) OnAdded(entity);
        }

        public Action<IEntity> OnUpdate;
        void ITriggerUpdate.OnUpdate(IEntity entity) {
            if (OnUpdate != null) OnUpdate(entity);
        }

        public Action<IEntity> OnRemoved;
        void ITriggerRemoved.OnRemoved(IEntity entity) {
            if (OnRemoved != null) OnRemoved(entity);
        }
    }
}