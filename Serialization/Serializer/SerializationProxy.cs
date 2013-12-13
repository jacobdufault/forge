using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Serialization {
    internal interface ISerializationProxy {
        object Import(SerializedData data);
        SerializedData Export(object instance);
    }

    internal interface ISerializationProxy<TSerializedType> : ISerializationProxy {
    }

    public abstract class SerializationProxy<TSerializedType> : ISerializationProxy<TSerializedType> {
        public abstract TSerializedType Import(SerializedData data);

        public abstract SerializedData Export(TSerializedType instance);

        object ISerializationProxy.Import(SerializedData data) {
            return Import(data);
        }

        SerializedData ISerializationProxy.Export(object instance) {
            return Export((TSerializedType)instance);
        }
    }
}