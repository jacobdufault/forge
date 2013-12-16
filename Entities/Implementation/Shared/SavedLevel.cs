using Neon.Entities.Implementation.Content;
using Neon.Entities.Serialization;
using Neon.FileSaving;
using Neon.Utilities;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    internal class SystemsContainer {
        private List<ISystem> _systems;

        private List<Type> _systemProviders;

        private List<IRestoredSystem> _restorableSystems;

    }

    internal class ISystemSurrogate {

    }

    [ProtoContract]
    internal class SavedLevel : ISavedLevel {
        [ProtoMember(1)]
        private List<string> _assemblyInjectionPaths = new List<string>();
        [ProtoMember(2)]
        private List<Type> _systemProviderTypes = new List<Type>();
        private GameSnapshot _currentState = new GameSnapshot();
        private GameSnapshot _originalState = new GameSnapshot();
        private List<ContentTemplate> _templates = new List<ContentTemplate>();
        private List<IssuedInput> _input = new List<IssuedInput>();

        private List<ISystemProvider> _systemProviders;

        List<string> ISavedLevel.AssemblyInjectionPaths {
            get {
                return _assemblyInjectionPaths;
            }
        }

        List<Type> ISavedLevel.SystemProviderTypes {
            get {
                return _systemProviderTypes;
            }
        }

        IGameSnapshot ISavedLevel.CurrentState {
            get {
                return _currentState;
            }
        }

        IGameSnapshot ISavedLevel.OriginalState {
            get {
                return _originalState;
            }
        }

        List<ITemplate> ISavedLevel.Templates {
            get {
                throw new NotImplementedException();
            }
        }

        List<IssuedInput> ISavedLevel.Input {
            get {
                return _input;
            }
        }

        [ProtoAfterDeserialization]
        private void GetSystemProviders() {
            _systemProviders = new List<ISystemProvider>();

            foreach (var providerType in _systemProviderTypes) {
                ISystemProvider instance = Activator.CreateInstance(providerType) as ISystemProvider;
                if (instance == null) {
                    throw new InvalidOperationException("System provider " + providerType.FullName +
                        " has to derive from ISystemProvider, but it does not");
                }

                _systemProviders.Add(instance);
            }
        }

        [ProtoAfterDeserialization]
        private void InjectAssemblies() {
            foreach (string assemblyPath in _assemblyInjectionPaths) {
                string fullAssemblyPath = Path.GetFullPath(assemblyPath);
                Log<SavedLevel>.Info("Loading assembly at path {0} (full path is {1})",
                    assemblyPath, fullAssemblyPath);

                Assembly.LoadFile(assemblyPath);
            }
        }
    }
}