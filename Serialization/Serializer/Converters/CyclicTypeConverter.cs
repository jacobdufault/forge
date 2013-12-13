using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization.Converters {
    internal class CyclicTypeConverter : ITypeConverter {
        private TypeModel _model;

        public CyclicTypeConverter(TypeModel model) {
            _model = model;
        }

        public static CyclicTypeConverter TryCreate(Type type) {
            TypeModel model = TypeCache.GetTypeModel(type);

            if (model.SupportsCyclicReferences) {
                return new CyclicTypeConverter(model);
            }

            return null;
        }

        public object Import(SerializedData data, ObjectGraphReader graph) {
            return graph.GetObjectInstance(_model, data);
        }

        public SerializedData Export(object instance, ObjectGraphWriter graph) {
            return graph.GetObjectReference(instance);
        }
    }
}