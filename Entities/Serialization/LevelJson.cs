using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Neon.Entities.Serialization {
    public class LevelJson {
        /// <summary>
        /// The relative paths of DLLs to inject.
        /// </summary>
        public List<string> DllInjections;

        /// <summary>
        /// The types of ISystemProviders that should be used to inject systems.
        /// </summary>
        public List<string> SystemProviders;

        /// <summary>
        /// The update number that the level is currently on.
        /// </summary>
        public int CurrentUpdateNumber;

        /// <summary>
        /// The templates that are used in this level.
        /// </summary>
        public List<TemplateJson> Templates;

        /// <summary>
        /// Singleton entity that the EntityManager owns
        /// </summary>
        public EntityJson SingletonEntity;

        /// <summary>
        /// All entities that are being simulated (at the current update).
        /// </summary>
        public List<EntityJson> Entities;

        /// <summary>
        /// Saved system states.
        /// </summary>
        public List<SavedSystemStateJson> SavedSystemStates;

        private void InjectDlls() {
            // load the dlls
            foreach (var dllInjectionPath in DllInjections) {
                string dllPath = Path.GetFullPath(dllInjectionPath);
                Console.WriteLine("Loading DLL " + dllPath);
                Assembly.LoadFile(dllPath);
            }
        }

        private List<ISystemProvider> GetSystemProviders() {
            List<ISystemProvider> providers = new List<ISystemProvider>();
            foreach (var providerName in SystemProviders) {
                Type providerType = TypeCache.FindType(providerName);
                object instance = Activator.CreateInstance(providerType);

                Console.WriteLine("Injecting systems from provider " + instance + " (an instance of type " + providerType + ")");

                if (instance is ISystemProvider == false) {
                    throw new Exception("System provider " + providerName + " has to derive from ISystemProvider, but it does not");
                }
                providers.Add((ISystemProvider)instance);
            }

            return providers;
        }

        /// <summary>
        /// Returns all systems that are loaded by the level (from the registered ISystemProviders)
        /// that have also had their states restored.
        /// </summary>

        private List<ISystem> GetRestoredSystems() {
            // create our systems from the system providers
            List<ISystem> systems = new List<ISystem>();
            foreach (var provider in GetSystemProviders()) {
                foreach (var system in provider.GetSystems()) {
                    Console.WriteLine("Adding system " + system);
                    systems.Add(system);
                }
            }

            // restore them
            foreach (SavedSystemStateJson systemJson in SavedSystemStates) {
                bool found = false;
                foreach (var system in systems) {
                    if (system.RestorationGUID == systemJson.RestorationGUID) {
                        system.Restore(systemJson.SavedState);
                        found = true;
                        break;
                    }
                }

                if (found == false) {
                    throw new Exception(string.Format("Unable to find a system with GUID={0} " +
                        "when restoring systems", systemJson.RestorationGUID));
                }
            }

            return systems;
        }

        public Tuple<EntityManager, LoadedMetadata> Restore() {
            // inject dlls so that type lookups resolve correctly
            // TODO: consider using an AppDomain so we can unload the previous EntitySystem
            InjectDlls();

            // load our template cache (so that EntityTemplates resolve correctly)
            TemplateJson.ClearCache();
            TemplateJson.LoadTemplates(Templates);

            // restore entities
            bool hasStateChange;
            bool hasModification;

            Entity singleton = SingletonEntity.Restore(out hasStateChange, out hasModification);

            List<EntityManager.RestoredEntity> restoredEntities = new List<EntityManager.RestoredEntity>();
            foreach (var entityJson in Entities) {
                Entity restoredEntity = entityJson.Restore(out hasStateChange, out hasModification);

                restoredEntities.Add(new EntityManager.RestoredEntity() {
                    Entity = restoredEntity,
                    HasModification = hasModification,
                    HasStateChange = hasStateChange,
                    IsAdding = entityJson.IsAdding,
                    IsRemoving = entityJson.IsRemoving
                });
            }

            // get restored systems
            List<ISystem> systems = GetRestoredSystems();

            EntityManager entityManager = new EntityManager(CurrentUpdateNumber, singleton, restoredEntities, systems);
            EntityTemplate.EntityManager = entityManager;

            LoadedMetadata metadata = new LoadedMetadata();
            metadata.DllInjections = DllInjections;
            metadata.SystemProviders = SystemProviders;
            metadata.Systems = systems;
            metadata.Templates = Templates;

            return Tuple.Create(entityManager, metadata);
        }
    }
}