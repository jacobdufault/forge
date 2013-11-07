using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// Metadata about a loaded EntityManager. This is used when the level is saved.
    /// </summary>
    public class LoadedMetadata {
        /// <summary>
        /// The DLLs that were injected / loaded.
        /// </summary>
        public List<string> DllInjections;

        /// <summary>
        /// The types used for loading systems.
        /// </summary>
        public List<string> SystemProviders;

        /// <summary>
        /// The active systems.
        /// </summary>
        public List<ISystem> Systems;

        /// <summary>
        /// The templates used for creating levels.
        /// </summary>
        public List<TemplateJson> Templates;

        /// <summary>
        /// Converter used for exporting entities
        /// </summary>
        public SerializationConverter Converter;
    }

    /// <summary>
    /// Handles loading and unloading an EntityManager from a file.
    /// </summary>
    public static class Loader {
        public static List<IEntity> LoadEntities(string levelPath) {
            throw new NotImplementedException();
        }

        public static List<EntityTemplate> LoadTemplates(string levelPath) {
            throw new NotImplementedException();
        }

        public static IEntity LoadSingletonEntity(string levelPath) {
            throw new NotImplementedException();
        }

        public static void SaveLevel(List<IEntity> entities, List<EntityTemplate> templates,
            IEntity singletonEntity, List<string> dlls, List<string> systemProviders) {
            throw new NotImplementedException();
        }

        public static Tuple<EntityManager, LoadedMetadata> LoadEntityManager(string levelPath) {
            String fileText = File.ReadAllText(levelPath);

            SerializedData data = Parser.Parse(fileText);
            SerializationConverter converter = new SerializationConverter();

            LevelJson level = converter.Import<LevelJson>(data);
            return level.Restore(converter);
        }

        public static string SaveEntityManager(EntityManager entityManager, LoadedMetadata metadata) {
            LevelJson level = new LevelJson();

            level.DllInjections = metadata.DllInjections;
            level.SystemProviders = metadata.SystemProviders;
            level.CurrentUpdateNumber = entityManager.UpdateNumber;

            // Serialize systems
            level.SavedSystemStates = new List<SavedSystemStateJson>();
            foreach (var system in metadata.Systems) {
                if (system is IRestoredSystem) {
                    IRestoredSystem restorableSystem = (IRestoredSystem)system;
                    SerializedData savedState = restorableSystem.Save();
                    SavedSystemStateJson systemJson = new SavedSystemStateJson() {
                        RestorationGUID = restorableSystem.RestorationGUID,
                        SavedState = savedState
                    };

                    level.SavedSystemStates.Add(systemJson);
                }

            }

            // Serialize entities
            level.SingletonEntity = ((Entity)entityManager.SingletonEntity).ToJson(false, false, metadata.Converter);

            List<Entity> removing = entityManager.GetEntitiesToRemove();

            level.Entities = new List<EntityJson>();
            foreach (var entity in entityManager.Entities) {
                level.Entities.Add(((Entity)entity).ToJson(entityIsAdding: false, entityIsRemoving: removing.Contains(((Entity)entity)), converter: metadata.Converter));
            }

            List<Entity> adding = entityManager.GetEntitiesToAdd();
            foreach (var entity in adding) {
                level.Entities.Add(((Entity)entity).ToJson(entityIsAdding: true, entityIsRemoving: false, converter: metadata.Converter));
            }

            level.Templates = metadata.Templates;

            return metadata.Converter.Export(level).PrettyPrinted;
        }
    }
}