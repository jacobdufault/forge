using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Entities.Implementation.Content {
    [JsonObject(MemberSerialization.OptIn)]
    internal class GameSnapshot : IGameSnapshot {
        public GameSnapshot() {
            _entityIdGenerator = new UniqueIntGenerator();
            _templateIdGenerator = new UniqueIntGenerator();

            SingletonEntity = new ContentEntity(_entityIdGenerator.Next(), "Global Singleton");
            ActiveEntities = new List<ContentEntity>();
            AddedEntities = new List<ContentEntity>();
            RemovedEntities = new List<ContentEntity>();
            Systems = new List<ISystem>();
            Templates = new List<ContentTemplate>();
        }

        [JsonProperty("EntityIdGenerator")]
        private UniqueIntGenerator _entityIdGenerator;
        [JsonProperty("TemplateIdGenerator")]
        private UniqueIntGenerator _templateIdGenerator;

        [JsonProperty("SingletonEntity")]
        public ContentEntity SingletonEntity {
            get;
            set;
        }

        [JsonProperty("ActiveEntities")]
        public List<ContentEntity> ActiveEntities {
            get;
            private set;
        }

        [JsonProperty("AddedEntities")]
        public List<ContentEntity> AddedEntities {
            get;
            private set;
        }

        [JsonProperty("RemovedEntities")]
        public List<ContentEntity> RemovedEntities {
            get;
            private set;
        }

        [JsonProperty("Systems")]
        public List<ISystem> Systems {
            get;
            set;
        }

        [JsonProperty("Templates")]
        public List<ContentTemplate> Templates {
            get;
            set;
        }

        public IEntity CreateEntity(EntityAddLocation to, string prettyName = "") {
            ContentEntity added = new ContentEntity(_entityIdGenerator.Next(), prettyName);

            switch (to) {
                case EntityAddLocation.Active:
                    ActiveEntities.Add(added);
                    break;
                case EntityAddLocation.Added:
                    AddedEntities.Add(added);
                    break;
                case EntityAddLocation.Removed:
                    RemovedEntities.Add(added);
                    break;
            }

            return added;
        }

        public ITemplate CreateTemplate() {
            ITemplate template = new ContentTemplate(_templateIdGenerator.Next());
            return template;
        }

        IEntity IGameSnapshot.SingletonEntity {
            get { return SingletonEntity; }
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

        IEnumerable<ITemplate> IGameSnapshot.Templates {
            get { return Templates.Cast<ITemplate>(); }
        }
    }
}