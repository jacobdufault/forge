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

using Forge.Entities.Implementation.ContextObjects;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Forge.Entities.Implementation.Content {
    [JsonObject(MemberSerialization.OptIn)]
    internal class GameSnapshot : IGameSnapshot {
        public GameSnapshot() {
            EntityIdGenerator = new UniqueIntGenerator();

            GlobalEntity = new ContentEntity(EntityIdGenerator.Next(), "Global Entity");
            ActiveEntities = new List<IEntity>();
            AddedEntities = new List<IEntity>();
            RemovedEntities = new List<IEntity>();
            _systems = new SystemSerializationContainer();
        }

        [JsonProperty("EntityIdGenerator")]
        public UniqueIntGenerator EntityIdGenerator;

        public IEntity GlobalEntity {
            get;
            set;
        }

        public List<IEntity> ActiveEntities {
            get;
            private set;
        }

        public List<IEntity> AddedEntities {
            get;
            private set;
        }

        public List<IEntity> RemovedEntities {
            get;
            private set;
        }

        [JsonProperty("GlobalEntity")]
        private EntitySerializationContainer _globalEntityContainer;
        [JsonProperty("AddedEntities")]
        private EntitySerializationContainer _addedEntitiesContainer;
        [JsonProperty("ActiveEntities")]
        private EntitySerializationContainer _activeEntitiesContainer;
        [JsonProperty("RemovedEntities")]
        private EntitySerializationContainer _removedEntitiesContainer;
        [JsonProperty("Systems")]
        private SystemSerializationContainer _systems;

        public List<ISystem> Systems {
            get {
                return _systems.Systems;
            }
            set {
                _systems.Systems = value;
            }
        }

        [OnSerializing]
        private void CreateConverter(StreamingContext context) {
            _globalEntityContainer = new EntitySerializationContainer() {
                Entities = new List<IEntity>() { GlobalEntity }
            };
            _addedEntitiesContainer = new EntitySerializationContainer() {
                Entities = AddedEntities
            };
            _activeEntitiesContainer = new EntitySerializationContainer() {
                Entities = ActiveEntities
            };
            _removedEntitiesContainer = new EntitySerializationContainer() {
                Entities = RemovedEntities
            };
        }

        [OnDeserializing]
        private void CreateContext(StreamingContext context) {
            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Create<EntityConversionContext>();
        }

        /// <summary>
        /// This method is called after deserialization is finished. It goes through all
        /// DataReferences and restores them.
        /// </summary>
        [OnDeserialized]
        private void RestoreDataReferences(StreamingContext context) {
            GlobalEntity = _globalEntityContainer.Entities[0];
            AddedEntities = _addedEntitiesContainer.Entities;
            ActiveEntities = _activeEntitiesContainer.Entities;
            RemovedEntities = _removedEntitiesContainer.Entities;

            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Remove<EntityConversionContext>();
        }

        public IEntity CreateEntity(string prettyName = "") {
            ContentEntity added = new ContentEntity(EntityIdGenerator.Next(), prettyName);
            AddedEntities.Add(added);
            return added;
        }

        /// <summary>
        /// Helper for tests to create snapshots with entities within more collections than just
        /// AddedEntities.
        /// </summary>
        internal enum EntityAddTarget {
            Added,
            Active,
            Removed
        }
        /// <summary>
        /// Helper for tests to create snapshots with entities within more collections than just
        /// AddedEntities.
        /// </summary>
        internal IEntity CreateEntity(EntityAddTarget target, string prettyName = "") {
            ContentEntity added = new ContentEntity(EntityIdGenerator.Next(), prettyName);

            if (target == EntityAddTarget.Added) {
                AddedEntities.Add(added);
            }
            else if (target == EntityAddTarget.Active) {
                ActiveEntities.Add(added);
            }
            else {
                RemovedEntities.Add(added);
            }

            return added;
        }

        public GameSnapshotEntityRemoveResult RemoveEntity(IEntity entity) {
            if (GlobalEntity == entity) {
                return GameSnapshotEntityRemoveResult.Failed;
            }

            for (int i = 0; i < AddedEntities.Count; ++i) {
                if (entity.UniqueId == AddedEntities[i].UniqueId) {
                    AddedEntities.RemoveAt(i);
                    return GameSnapshotEntityRemoveResult.Destroyed;
                }
            }

            for (int i = 0; i < ActiveEntities.Count; ++i) {
                if (entity.UniqueId == ActiveEntities[i].UniqueId) {
                    IEntity removed = ActiveEntities[i];
                    ActiveEntities.RemoveAt(i);
                    RemovedEntities.Add(removed);
                    return GameSnapshotEntityRemoveResult.IntoRemoved;
                }
            }

            foreach (IEntity removed in RemovedEntities) {
                if (entity.UniqueId == removed.UniqueId) {
                    return GameSnapshotEntityRemoveResult.Failed;
                }
            }

            throw new InvalidOperationException("Unable to find entity with UniqueId = " + entity.UniqueId);
        }

        IEntity IGameSnapshot.GlobalEntity {
            get { return GlobalEntity; }
        }

        IEnumerable<IEntity> IGameSnapshot.ActiveEntities {
            get { return ActiveEntities.Cast<IEntity>(); }
        }

        IEnumerable<IEntity> IGameSnapshot.RemovedEntities {
            get { return RemovedEntities.Cast<IEntity>(); }
        }

        IEnumerable<IEntity> IGameSnapshot.AddedEntities {
            get { return AddedEntities.Cast<IEntity>(); }
        }

        List<ISystem> IGameSnapshot.Systems {
            get { return Systems; }
        }
    }
}