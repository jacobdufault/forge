using LitJson;
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

            JsonReader reader = new JsonReader(fileText);
            reader.SkipNonMembers = false;
            reader.AllowComments = true;

            LevelJson level = JsonMapper.ToObject<LevelJson>(reader);
            return level.Restore();
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
                    JsonData savedState = restorableSystem.Save();
                    if (savedState.GetJsonType() != JsonType.None) {
                        SavedSystemStateJson systemJson = new SavedSystemStateJson() {
                            RestorationGUID = restorableSystem.RestorationGUID,
                            SavedState = savedState
                        };

                        level.SavedSystemStates.Add(systemJson);
                    }
                }

            }

            // Serialize entities
            level.SingletonEntity = ((Entity)entityManager.SingletonEntity).ToJson(false, false);

            List<Entity> removing = entityManager.GetEntitiesToRemove();

            level.Entities = new List<EntityJson>();
            foreach (var entity in entityManager.Entities) {
                level.Entities.Add(((Entity)entity).ToJson(entityIsAdding: false, entityIsRemoving: removing.Contains(((Entity)entity))));
            }

            List<Entity> adding = entityManager.GetEntitiesToAdd();
            foreach (var entity in adding) {
                level.Entities.Add(((Entity)entity).ToJson(entityIsAdding: true, entityIsRemoving: false));
            }

            level.Templates = metadata.Templates;

            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;
            JsonMapper.ToJson(level, writer);

            return writer.ToString();
        }
    }
}