using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Forge.Entities.Tests {
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
    internal class SystemEventLogger : BaseSystem, Trigger.Added, Trigger.Removed, Trigger.Modified,
        Trigger.Update, Trigger.GlobalPreUpdate, Trigger.GlobalPostUpdate, Trigger.Input,
        Trigger.GlobalInput {
        public Type[] RequiredDataTypes {
            get { return EntityFilter; }
        }

        [JsonProperty("EntityFilter")]
        public Type[] EntityFilter;

        public SystemEventLogger() {
            EntityFilter = new Type[0];
        }

        public SystemEventLogger(Type[] entityFilter) {
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

        public void OnGlobalPreUpdate() {
            Events.Add(TriggerEvent.OnGlobalPreUpdate);
        }

        public void OnGlobalPostUpdate() {
            Events.Add(TriggerEvent.OnGlobalPostUpdate);
        }

        public Type[] InputTypes {
            get { return new Type[] { }; }
        }

        public void OnInput(IGameInput input, IEntity entity) {
            Events.Add(TriggerEvent.OnInput);
        }

        public void OnGlobalInput(IGameInput input) {
            Events.Add(TriggerEvent.OnGlobalInput);
        }
    }
}