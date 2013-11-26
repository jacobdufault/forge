using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// Metadata about a loaded EntityManager. This is used when the level is saved.
    /// </summary>
    internal class LoadedMetadata {
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
        public List<SerializedTemplate> Templates;

        /// <summary>
        /// Converter used for exporting entities
        /// </summary>
        public SerializationConverter Converter;
    }

    /// <summary>
    /// Handles loading and unloading an EntityManager from a file.
    /// </summary>
    internal static class Loader {
        /// <summary>
        /// Loads a level structure from a NES file at the given path.
        /// </summary>
        public static SerializedLevel LoadSerializedLevel(string levelPath,
            SerializationConverter converter = null) {
            if (converter == null) {
                converter = new SerializationConverter();
            }

            String fileText = File.ReadAllText(levelPath);
            SerializedData data = Parser.Parse(fileText);
            return converter.Import<SerializedLevel>(data);
        }

        public static Tuple<GameEngine, LoadedMetadata> LoadEntityManager(string levelPath) {
            SerializationConverter converter = new SerializationConverter();
            SerializedLevel level = LoadSerializedLevel(levelPath, converter);
            return level.Restore(converter);
        }

        public static string SaveGameEngine(GameEngine entityManager, LoadedMetadata metadata) {
            SerializedLevel level = new SerializedLevel();

            level.DllInjections = metadata.DllInjections;
            level.SystemProviders = metadata.SystemProviders;
            level.CurrentUpdateNumber = entityManager.UpdateNumber;

            // Serialize systems
            level.SavedSystemStates = new List<SerializedSystem>();
            foreach (var system in metadata.Systems) {
                if (system is IRestoredSystem) {
                    IRestoredSystem restorableSystem = (IRestoredSystem)system;
                    SerializedData savedState = restorableSystem.Save();
                    SerializedSystem serializedSystem = new SerializedSystem() {
                        RestorationGUID = restorableSystem.RestorationGUID,
                        SavedState = savedState
                    };

                    level.SavedSystemStates.Add(serializedSystem);
                }

            }

            // Serialize entities
            level.SingletonEntity = ((Entity)entityManager.SingletonEntity).ToSerializedEntity(
                false, false, metadata.Converter);

            List<IEntity> removing = entityManager.GetEntitiesToRemove();

            level.Entities = new List<SerializedEntity>();
            foreach (var entity in entityManager.Entities) {
                level.Entities.Add(((Entity)entity).ToSerializedEntity(entityIsAdding: false,
                    entityIsRemoving: removing.Contains(((Entity)entity)),
                    converter: metadata.Converter));
            }

            List<IEntity> adding = entityManager.GetEntitiesToAdd();
            foreach (var entity in adding) {
                level.Entities.Add(((Entity)entity).ToSerializedEntity(entityIsAdding: true,
                    entityIsRemoving: false, converter: metadata.Converter));
            }

            level.Templates = metadata.Templates;

            return metadata.Converter.Export(level).PrettyPrinted;
        }
    }
}