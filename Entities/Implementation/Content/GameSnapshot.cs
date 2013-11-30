using Neon.Entities.Implementation.Content.Specifications;
using Neon.Entities.Implementation.Shared;
using Neon.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Entities.Implementation.Content {
    internal class GameSnapshot : IGameSnapshot {
        public static IGameSnapshot Read(SerializedData data, SerializationConverter converter, List<ISystem> systems) {
            GameSnapshot contentDatabase = new GameSnapshot();

            var dict = data.AsDictionary;

            // restore the entities (use EntityDeserializer to support circular references)
            EntityDeserializer entityDeserializer = new EntityDeserializer(converter);
            contentDatabase.SingletonEntity = entityDeserializer.AddEntity(dict["SingletonEntity"]);
            contentDatabase.ActiveEntities = entityDeserializer.AddEntities(dict["ActiveEntities"].AsList);
            contentDatabase.AddedEntities = entityDeserializer.AddEntities(dict["AddedEntities"].AsList);
            contentDatabase.RemovedEntities = entityDeserializer.AddEntities(dict["RemovedEntities"].AsList);
            entityDeserializer.Run();

            // get systems that need restoration
            List<IRestoredSystem> restorableSystems = (from s in systems
                                                       where s is IRestoredSystem
                                                       select (IRestoredSystem)s).ToList();

            // restore systems
            contentDatabase.Systems = new List<ISystem>(systems);
            foreach (var system in data.AsDictionary["Systems"].AsList) {
                RestorableSystemSpecification systemSpec = new RestorableSystemSpecification(system);

                foreach (var restorableSystem in restorableSystems) {
                    if (systemSpec.RestorationGuid == restorableSystem.RestorationGuid) {
                        restorableSystem.ImportState(systemSpec.SavedState);
                        restorableSystems.Remove(restorableSystem);
                        break;
                    }
                }
            }

            if (restorableSystems.Count > 0) {
                throw new InvalidOperationException("Not all systems which requested restoration were restored");
            }

            return contentDatabase;
        }

        public SerializedData Export(SerializationConverter converter) {
            Dictionary<string, SerializedData> dict = new Dictionary<string, SerializedData>();

            // entities
            dict["SingletonEntity"] = new EntitySpecification(SingletonEntity, false, false, converter).Export();

            List<SerializedData> active = new List<SerializedData>();
            foreach (var entity in ActiveEntities) {
                active.Add(new EntitySpecification(entity, false, false, converter).Export());
            }
            dict["ActiveEntities"] = new SerializedData(active);

            List<SerializedData> added = new List<SerializedData>();
            foreach (var entity in AddedEntities) {
                added.Add(new EntitySpecification(entity, false, false, converter).Export());
            }
            dict["AddedEntities"] = new SerializedData(added);

            List<SerializedData> removed = new List<SerializedData>();
            foreach (var entity in RemovedEntities) {
                removed.Add(new EntitySpecification(entity, false, false, converter).Export());
            }
            dict["RemovedEntities"] = new SerializedData(removed);

            // save system state
            List<SerializedData> systems = new List<SerializedData>();
            foreach (var system in Systems) {
                if (system is IRestoredSystem) {
                    IRestoredSystem restoredSystem = (IRestoredSystem)system;

                    RestorableSystemSpecification restoreSpec = new RestorableSystemSpecification(restoredSystem, converter);
                    systems.Add(restoreSpec.Export());
                }
            }
            dict["Systems"] = new SerializedData(systems);

            return new SerializedData(dict);
        }

        public GameSnapshot() {
            SingletonEntity = new ContentEntity();
            ActiveEntities = new List<IEntity>();
            AddedEntities = new List<IEntity>();
            RemovedEntities = new List<IEntity>();
            Systems = new List<ISystem>();
        }

        public IEntity SingletonEntity {
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

        public List<ISystem> Systems {
            get;
            set;
        }
    }
}