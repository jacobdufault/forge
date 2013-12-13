using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class ProxyTypeConverter : ITypeConverter {
        /// <summary>
        /// Map of all types which have proxies to their proxy.
        /// </summary>
        private static Dictionary<Type, ISerializationProxy> _proxies;

        static ProxyTypeConverter() {
            UpdateSerializationProxies();
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => {
                UpdateSerializationProxies();
            };
        }
        private static void UpdateSerializationProxies() {
            var proxyProviders = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                 from type in assembly.GetTypes()
                                 where type.IsImplementationOf(typeof(ISerializationProxy))
                                 where type.IsInterface == false
                                 where type.IsAbstract == false
                                 select type;

            _proxies = new Dictionary<Type, ISerializationProxy>();
            foreach (var proxyProvider in proxyProviders) {
                Type proxyFor = proxyProvider.GetInterface(typeof(ISerializationProxy<>)).GetGenericArguments()[0];
                _proxies[proxyFor] = (ISerializationProxy)Activator.CreateInstance(proxyProvider);
            }
        }

        internal static ProxyTypeConverter TryCreate(Type type) {
            ISerializationProxy proxy;
            if (_proxies.TryGetValue(type, out proxy)) {
                return new ProxyTypeConverter(type, proxy);
            }

            return null;
        }

        private ProxyTypeConverter(Type baseType, ISerializationProxy proxy) {
            throw new NotImplementedException();
        }

        public object Import(SerializedData data, ObjectGraphReader graph) {
            throw new NotImplementedException();
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            throw new NotImplementedException();
        }
    }
}