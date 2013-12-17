using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Entities.Implementation.Content {
    [ProtoContract]
    internal class GameSnapshot : IGameSnapshot {
        /// <summary>
        /// Make sure that the SerializableContainer is registered with the default type metadata
        /// </summary>
        static GameSnapshot() {
            SerializableContainer.RegisterWithTypeModel(RuntimeTypeModel.Default);
        }

        public GameSnapshot() {
            _entityIdGenerator = new UniqueIntGenerator();

            SingletonEntity = new ContentEntity(_entityIdGenerator.Next(), "Global Singleton");
            ActiveEntities = new List<ContentEntity>();
            AddedEntities = new List<ContentEntity>();
            RemovedEntities = new List<ContentEntity>();
            Systems = new List<ISystem>();
            Templates = new List<ContentTemplate>();

            _templateResolver = new TemplateResolver();
        }

        [ProtoMember(1)]
        private UniqueIntGenerator _entityIdGenerator;

        [ProtoMember(2)]
        public ContentEntity SingletonEntity {
            get;
            set;
        }

        [ProtoMember(3)]
        public List<ContentEntity> ActiveEntities {
            get;
            private set;
        }

        [ProtoMember(4)]
        public List<ContentEntity> AddedEntities {
            get;
            private set;
        }

        [ProtoMember(5)]
        public List<ContentEntity> RemovedEntities {
            get;
            private set;
        }

        [ProtoMember(6)]
        private SerializableContainer _serializedSystems;

        [ProtoBeforeSerialization]
        private void ExportSystems() {
            _serializedSystems = new SerializableContainer(Systems);
        }

        [ProtoAfterDeserialization]
        private void ImportSystems() {
            if (ActiveEntities == null) ActiveEntities = new List<ContentEntity>();
            if (RemovedEntities == null) RemovedEntities = new List<ContentEntity>();
            if (Templates == null) Templates = new List<ContentTemplate>();

            Systems = _serializedSystems.ToList<ISystem>();
            SetTemplateReferences(Templates.Cast<ITemplate>());
        }

        public List<ISystem> Systems {
            get;
            set;
        }

        [ProtoMember(7)]
        public List<ContentTemplate> Templates {
            get;
            set;
        }

        /// <summary>
        /// Template resolver that should be used only within this snapshot; ie, all data and entity
        /// instances *within* this snapshot (reachable from this object instance) should all point
        /// to this template resolver, but no other reference should point to this resolver.
        /// </summary>
        [ProtoMember(8)]
        private TemplateResolver _templateResolver;

        /// <summary>
        /// Sets the templates that all entities will reference.
        /// </summary>
        /// <param name="templates">The templates that all entity/data references inside of this
        /// object will reference</param>
        public void SetTemplateReferences(IEnumerable<ITemplate> templates) {
            _templateResolver.SetTemplates(templates);
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

        public TemplateReference CreateTemplate() {
            return _templateResolver.CreateTemplate();
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