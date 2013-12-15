// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class ProxyTypeConverter : BaseTypeConverter {
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
                return new ProxyTypeConverter(TypeCache.GetTypeModel(type), proxy);
            }

            return null;
        }

        private ISerializationProxy _proxy;

        private ProxyTypeConverter(TypeModel model, ISerializationProxy proxy) {
            _proxy = proxy;
        }

        protected override object DoImport(SerializedData data, ObjectGraphReader graph, object instance) {
            if (instance != null) {
                throw new InvalidOperationException("Cannot handle preallocated objects");
            }

            return _proxy.Import(data);
        }

        protected override SerializedData DoExport(object instance, ObjectGraphWriter graph) {
            return _proxy.Export(instance);
        }
    }
}