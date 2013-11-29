using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Content.Specifications;
using Neon.Entities.Serialization;
using Neon.FileSaving;
using Neon.Serialization;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neon.Entities.Implementation.Shared {
    internal class EntitiesSaveFileItem : ISaveFileItem, ISavedLevel {
        public EntitiesSaveFileItem() {
            AssemblyInjectionPaths = new List<string>();
            SystemProviderTypes = new List<Type>();
            CurrentState = new GameSnapshot();
            OriginalState = new GameSnapshot();
            Templates = new List<ITemplate>();
            Input = new List<IssuedInput>();
        }

        public Guid Identifier {
            get { return new Guid("C8C0303D-97C9-4876-91C0-F6259CCE6760"); }
        }

        public string PrettyIdentifier {
            get { return "Neon.Entities Saved State (format v1.0)"; }
        }

        private List<ISystemProvider> GetSystemProviders() {
            List<ISystemProvider> providers = new List<ISystemProvider>();

            foreach (var providerType in SystemProviderTypes) {
                ISystemProvider instance = Activator.CreateInstance(providerType) as ISystemProvider;
                if (instance == null) {
                    throw new InvalidOperationException("System provider " + providerType.FullName +
                        " has to derive from ISystemProvider, but it does not");
                }

                providers.Add(instance);
            }

            return providers;
        }

        private void InjectAssemblies() {
            foreach (string assemblyPath in AssemblyInjectionPaths) {
                string fullAssemblyPath = Path.GetFullPath(assemblyPath);
                Log<EntitiesSaveFileItem>.Info("Loading assembly at path {0} (full path is {1})",
                    assemblyPath, fullAssemblyPath);

                Assembly.LoadFile(assemblyPath);
            }
        }

        public void Import(SerializedData data) {
            SerializationConverter converter = new SerializationConverter();

            // load assemblies
            AssemblyInjectionPaths = converter.Import<List<string>>(data.AsDictionary["AssemblyInjectionPaths"]);
            InjectAssemblies();

            // get systems
            List<string> systemProviders = converter.Import<List<string>>(data.AsDictionary["SystemProviders"]);
            SystemProviderTypes = (from providerTypeName in systemProviders
                                   select TypeCache.FindType(providerTypeName)).ToList();

            List<ISystem> systems = (from provider in GetSystemProviders()
                                     from system in provider.GetSystems()
                                     select system).ToList();

            // load templates
            TemplateDeserializer templateDeserializer = new TemplateDeserializer(
                data.AsDictionary["Templates"].AsList, converter);
            List<ITemplate> templates = templateDeserializer.ToList();

            // load the two content databases
            CurrentState = GameSnapshot.Read(data.AsDictionary["Current"], converter, templates, systems);
            OriginalState = GameSnapshot.Read(data.AsDictionary["Original"], converter, templates, systems);

            // get input
            Input = converter.Import<List<IssuedInput>>(data.AsDictionary["Input"]);
        }

        public SerializedData Export() {
            SerializationConverter converter = new SerializationConverter();

            SerializedData result = SerializedData.CreateDictionary();

            // metadata
            result.AsDictionary["AssemblyInjectionPaths"] = converter.Export(AssemblyInjectionPaths);
            result.AsDictionary["SystemProviders"] = converter.Export(SystemProviderTypes.Select(t => t.FullName).ToList());

            // templates
            List<SerializedData> serializedTemplates = new List<SerializedData>();
            TemplateDeserializer.AddTemplateExporter(converter);
            foreach (var template in Templates) {
                TemplateSpecification templateSpec = new TemplateSpecification((Template)template, converter);
                serializedTemplates.Add(templateSpec.Export());
            }
            result.AsDictionary["Templates"] = new SerializedData(serializedTemplates);

            // content databases
            result.AsDictionary["Current"] = ((GameSnapshot)CurrentState).Export(converter);
            result.AsDictionary["Original"] = ((GameSnapshot)OriginalState).Export(converter);

            result.AsDictionary["Input"] = converter.Export(Input);

            return result;
        }

        public List<string> AssemblyInjectionPaths {
            get;
            private set;
        }

        public List<Type> SystemProviderTypes {
            get;
            private set;
        }

        public IGameSnapshot CurrentState {
            get;
            private set;
        }

        public IGameSnapshot OriginalState {
            get;
            private set;
        }

        public List<ITemplate> Templates {
            get;
            private set;
        }

        public List<IssuedInput> Input {
            get;
            private set;
        }
    }
}