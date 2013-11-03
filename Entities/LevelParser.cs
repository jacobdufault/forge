using LitJson;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Neon.Entities {
    static class TypeCache {
        private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        public static Type FindType(string name) {
            // see if the type is in the cache; if it is, then just return it
            {
                Type type;
                if (_cachedTypes.TryGetValue(name, out type)) {
                    return type;
                }
            }

            // cache lookup failed; search all loaded assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    if (type.FullName == name) {
                        _cachedTypes[name] = type;
                        return type;
                    }
                }
            }

            // couldn't find the type... oops
            throw new Exception("Unable to find the type for " + name + "; is the configuration file for DLL loading correct?");
        }
    }

    public class DataJson {
        /// <summary>
        /// The type name of the data that this data item maps to.
        /// </summary>
        public string DataType;

        /// <summary>
        /// True if the data was modified in the last update.
        /// </summary>
        public bool WasModified;

        /// <summary>
        /// True if the data needs to be added in the next update.
        /// </summary>
        public bool IsAdding;

        /// <summary>
        /// True if the data needs to be removed in the next update.
        /// </summary>
        public bool IsRemoving;

        /// <summary>
        /// The previous state of the data, in JSON form. We can only deserialize this when we have
        /// resolved the data type.
        /// </summary>
        public JsonData PreviousState;

        /// <summary>
        /// The current state of the data, in JSON form. We can only deserialize this when we have
        /// resolved the data type.
        /// </summary>
        public JsonData CurrentState;

        public Data DeserializedPreviousState {
            get {
                object deserialized = JsonMapper.ReadValue(DeserializedType, PreviousState);
                return (Data)deserialized;
            }
        }

        public Data DeserializedCurrentState {
            get {
                object deserialized = JsonMapper.ReadValue(DeserializedType, CurrentState);
                return (Data)deserialized;
            }
        }

        private Type DeserializedType {
            get {
                return TypeCache.FindType(DataType);
            }
        }

        public void Print() {
            Console.WriteLine("\tData");
            Console.WriteLine("\t\tDataType = " + DataType);
            Console.WriteLine("\t\tWasModified = " + WasModified);
            Console.WriteLine("\t\tIsAdding = " + IsAdding);
            Console.WriteLine("\t\tIsRemoving = " + IsRemoving);
            Console.WriteLine("\t\tPreviousState = " + DeserializedPreviousState);
            Console.WriteLine("\t\tCurrentState = " + DeserializedCurrentState);
        }
    }

    public class TemplateDataJson {
        public string DataType;
        public JsonData State;
    }

    public class TemplateJson {
        public string PrettyName;
        public int UniqueId;
        public List<TemplateDataJson> Data;
    }

    public class EntityJson {
        public string PrettyName;
        public int UniqueId;
        public List<DataJson> Data;
        public bool IsAdding;
        public bool IsRemoving;

        public Entity Restore(out bool hasStateChange, out bool hasModification) {
            hasStateChange = false;
            hasModification = false;

            List<SerializedEntityData> restoredData = new List<SerializedEntityData>();
            foreach (var dataJson in Data) {
                hasModification = hasModification || dataJson.WasModified;
                hasStateChange = hasStateChange || dataJson.IsAdding || dataJson.IsRemoving;

                SerializedEntityData data = new SerializedEntityData(
                    wasModifying: dataJson.WasModified,
                    isAdding: dataJson.IsAdding,
                    isRemoving: dataJson.IsRemoving,
                    previous: dataJson.DeserializedPreviousState,
                    current: dataJson.DeserializedCurrentState
                );
                restoredData.Add(data);
            }

            return new Entity(UniqueId, restoredData);
        }

        public void Print() {
            Console.WriteLine("Entity");
            Console.WriteLine("\tPrettyName = " + PrettyName);
            Console.WriteLine("\tUniqueId = " + UniqueId);
            foreach (var data in Data) {
                data.Print();
            }
        }
    }

    public class LevelJson {
        public string ConfigurationPath;
        public int CurrentUpdateNumber;
        public List<TemplateJson> Templates;
        public EntityJson SingletonEntity;
        public List<EntityJson> Entities;
        public List<SystemJson> Systems;

        private ConfigurationJson ConfigurationJson {
            get {
                JsonReader reader = new JsonReader(File.ReadAllText(ConfigurationPath));
                reader.AllowComments = true;
                reader.SkipNonMembers = false;

                return JsonMapper.ToObject<ConfigurationJson>(File.ReadAllText(ConfigurationPath));
            }
        }

        public EntityManager Restore() {
            // get configuration
            ConfigurationJson config = ConfigurationJson;
            config.Restore();


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


            // create all of our systems
            List<ISystem> systems = new List<ISystem>();
            foreach (var provider in config.Providers) {
                systems.AddRange(provider.GetSystems());
            }

            // restore them
            foreach (SystemJson systemJson in Systems) {
                bool found = false;
                foreach (var system in systems) {
                    if (system.RestorationGUID == systemJson.RestorationGUID) {
                        system.Restore(systemJson.SavedState);
                        found = true;
                        break;
                    }
                }

                if (found == false) {
                    throw new Exception("No system was registered with GUID=" + systemJson.RestorationGUID);
                }
            }

            return new EntityManager(CurrentUpdateNumber, singleton, restoredEntities, systems);
        }

        public void Print() {
            Console.WriteLine("ConfigurationPath = " + ConfigurationPath);
            foreach (var entity in Entities) {
                entity.Print();
            }
        }
    }

    public class ConfigurationJson {
        public bool EnableMultithreading;
        public List<string> DllInjections;
        public List<string> SystemProviders;

        private List<ISystemProvider> _providers;
        public List<ISystemProvider> Providers {
            get {
                if (_providers == null) {
                    throw new InvalidOperationException("Restore the configuration before retrieving the list of providers");
                }

                return _providers;
            }
        }

        public void Restore() {
            if (EnableMultithreading) {
                EntityManager.EnableMultithreading = true;
            }

            // load the dlls
            foreach (var dllInjectionPath in DllInjections) {
                string dllPath = Path.GetFullPath(dllInjectionPath);
                Console.WriteLine("Loading DLL " + dllPath);
                Assembly.LoadFile(dllPath);
            }

            // create the providers
            _providers = new List<ISystemProvider>();
            foreach (var providerName in SystemProviders) {
                Type providerType = TypeCache.FindType(providerName);
                object instance = Activator.CreateInstance(providerType);

                Console.WriteLine("Injecting systems from provider " + instance + " (an instance of type " + providerType + ")");

                if (instance is ISystemProvider == false) {
                    throw new Exception("System provider " + providerName + " has to derive from ISystemProvider, but it does not");
                }
                _providers.Add((ISystemProvider)instance);
            }
        }
    }

    public class SystemJson {
        public string RestorationGUID;
        public JsonData SavedState;
    }

    /// <summary>
    /// Provides a set of instantiated systems that will be used when the engine is executing.
    /// </summary>
    public interface ISystemProvider {
        /// <summary>
        /// Return all systems that should be processed while the engine is executing.
        /// </summary>
        ISystem[] GetSystems();
    }
}