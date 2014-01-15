using System;
using System.Collections.Generic;
using System.Reflection;

namespace Forge.Utilities {
    /// <summary>
    /// Caches type name to type lookups. Type lookups occur in all loaded assemblies.
    /// </summary>
    public static class TypeCache {
        /// <summary>
        /// Cache from fully qualified type name to type instances.
        /// </summary>
        // TODO: verify that type names will be unique
        private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Cache from Type to the respective TypeMetadata.
        /// </summary>
        private static Dictionary<Type, TypeMetadata> _cachedMetadata;

        /// <summary>
        /// Assemblies indexed by their name.
        /// </summary>
        private static Dictionary<string, Assembly> _assemblies;

        static TypeCache() {
            _assemblies = new Dictionary<string, Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                _assemblies[assembly.FullName] = assembly;
            }

            _cachedTypes = new Dictionary<string, Type>();
            _cachedMetadata = new Dictionary<Type, TypeMetadata>();

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
        }

        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
            _assemblies[args.LoadedAssembly.FullName] = args.LoadedAssembly;
        }

        /// <summary>
        /// Does a direct lookup for the given type, ie, goes directly to the assembly identified by
        /// assembly name and finds it there.
        /// </summary>
        /// <param name="assemblyName">The assembly to find the type in.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="type">The found type.</param>
        /// <returns>True if the type was found, false otherwise.</returns>
        private static bool TryDirectTypeLookup(string assemblyName, string typeName, out Type type) {
            if (assemblyName != null) {
                Assembly assembly;
                if (_assemblies.TryGetValue(assemblyName, out assembly)) {
                    type = assembly.GetType(typeName);
                    return type != null;
                }
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Tries to do an indirect type lookup by scanning through every loaded assembly until the
        /// type is found in one of them.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="type">The found type.</param>
        /// <returns>True if the type was found, false otherwise.</returns>
        private static bool TryIndirectTypeLookup(string typeName, out Type type) {
            foreach (Assembly assembly in _assemblies.Values) {
                type = assembly.GetType(typeName);
                if (type != null) {
                    return true;
                }
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Find a type with the given name. An exception is thrown if no type with the given name
        /// can be found. This method searches all currently loaded assemblies for the given type.
        /// </summary>
        /// <param name="name">The fully qualified name of the type.</param>
        /// <param name="assemblyHint">A hint for the assembly to start the search with</param>
        public static Type FindType(string name, string assemblyHint = null) {
            Type type;
            if (_cachedTypes.TryGetValue(name, out type) == false) {
                // if both the direct and indirect type lookups fail, then throw an exception
                if (TryDirectTypeLookup(assemblyHint, name, out type) == false &&
                    TryIndirectTypeLookup(name, out type) == false) {

                    throw new InvalidOperationException(
                        string.Format("Cannot locate type = {0}, {1}", name, assemblyHint));
                }

                _cachedTypes[name] = type;
            }

            return type;
        }

        /// <summary>
        /// Finds the associated TypeMetadata for the given type.
        /// </summary>
        /// <param name="type">The type to find the type metadata for.</param>
        /// <returns>A TypeMetadata that models the given type.</returns>
        public static TypeMetadata FindTypeMetadata(Type type) {
            TypeMetadata metadata;
            if (_cachedMetadata.TryGetValue(type, out metadata) == false) {
                metadata = new TypeMetadata(type);
                _cachedMetadata[type] = metadata;
            }
            return metadata;
        }
    }
}