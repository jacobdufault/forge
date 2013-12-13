using System;
using System.Collections.Generic;
using System.Reflection;

namespace Neon.Serialization {
    /// <summary>
    /// Caches type name to type lookups. Type lookups occur in all loaded assemblies.
    /// </summary>
    public static class TypeCache {
        /// <summary>
        /// Cache from fully qualified type name to type instances.
        /// </summary>
        private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Cache from types to its associated model.
        /// </summary>
        private static Dictionary<Type, TypeModel> _cachedTypeModels = new Dictionary<Type, TypeModel>();

        /// <summary>
        /// Find a type with the given name. An exception is thrown if no type with the given name
        /// can be found. This method searches all currently loaded assemblies for the given type.
        /// </summary>
        /// <param name="name">The fully qualified name of the type.</param>
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
                Type type = assembly.GetType(name);
                if (type != null) {
                    _cachedTypes[name] = type;
                    return type;
                }
            }

            // couldn't find the type; throw an exception
            throw new Exception(string.Format("Unable to find the type for {0}", name));
        }

        /// <summary>
        /// Finds the type model associated with the given type. If there is currently no model,
        /// then this method creates it. Otherwise, it returns a cached instance.
        /// </summary>
        public static TypeModel GetTypeModel(Type type) {
            TypeModel model;
            if (_cachedTypeModels.TryGetValue(type, out model) == false) {
                model = new TypeModel(type);
                _cachedTypeModels[type] = model;
            }

            return model;
        }
    }
}