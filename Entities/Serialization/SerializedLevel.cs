using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// Serialization specification for an EntityManager state (a level).
    /// </summary>
    public class SerializedLevel {
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
        public List<SerializedTemplate> Templates;

        /// <summary>
        /// Singleton entity that the EntityManager owns
        /// </summary>
        public SerializedEntity SingletonEntity;

        /// <summary>
        /// All entities that are being simulated (at the current update).
        /// </summary>
        public List<SerializedEntity> Entities;

        /// <summary>
        /// Saved system states.
        /// </summary>
        public List<SerializedSystem> SavedSystemStates;

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
                ISystemProvider instance = Activator.CreateInstance(providerType) as ISystemProvider;
                if (instance == null) {
                    throw new InvalidOperationException("System provider " + providerName +
                        " has to derive from ISystemProvider, but it does not");
                }
                providers.Add(instance);
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
            foreach (SerializedSystem serializedSystem in SavedSystemStates) {
                bool found = false;
                foreach (var system in systems) {
                    if (system is IRestoredSystem) {
                        IRestoredSystem restorableSystem = (IRestoredSystem)system;
                        if (restorableSystem.RestorationGUID == serializedSystem.RestorationGUID) {
                            restorableSystem.Restore(serializedSystem.SavedState);
                            found = true;
                            break;
                        }
                    }
                }

                if (found == false) {
                    throw new Exception(string.Format("Unable to find a system with GUID={0} " +
                        "when restoring systems", serializedSystem.RestorationGUID));
                }
            }

            return systems;
        }

        public Tuple<EntityManager, LoadedMetadata> Restore(SerializationConverter converter) {
            // inject dlls so that type lookups resolve correctly
            // TODO: consider using an AppDomain so we can unload the previous EntitySystem
            InjectDlls();

            EntityTemplateDeserializer templateDeserializer = new EntityTemplateDeserializer(Templates, converter);
            templateDeserializer.AddTemplateImporter(converter);

            var restoredSystems = GetRestoredSystems();

            EntityManager entityManager = new EntityManager(
                CurrentUpdateNumber,
                SingletonEntity,
                Entities,
                restoredSystems,
                converter);
            EntityTemplate.EntityManager = entityManager;

            LoadedMetadata metadata = new LoadedMetadata();
            metadata.DllInjections = DllInjections;
            metadata.SystemProviders = SystemProviders;
            metadata.Systems = restoredSystems;
            metadata.Templates = Templates;
            metadata.Converter = converter;

            templateDeserializer.RemoveTemplateConverter(converter);

            return Tuple.Create(entityManager, metadata);
        }
    }
}