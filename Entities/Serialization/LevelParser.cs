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
        /// <summary>
        /// Loads a level structure from a NES file at the given path.
        /// </summary>
        public static LevelJson LoadLevelJson(string levelPath, SerializationConverter converter = null) {
            if (converter == null) {
                converter = new SerializationConverter();
            }

            String fileText = File.ReadAllText(levelPath);
            SerializedData data = Parser.Parse(fileText);
            return converter.Import<LevelJson>(data);
        }

        public static Tuple<EntityManager, LoadedMetadata> LoadEntityManager(string levelPath) {
            SerializationConverter converter = new SerializationConverter();
            LevelJson level = LoadLevelJson(levelPath, converter);
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