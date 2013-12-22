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

using Newtonsoft.Json;
using System;

namespace Neon.Entities {
    /// <summary>
    /// Interface used for type erasure by BaseDataReferenceType.
    /// </summary>
    public interface IDataReference {
        /// <summary>
        /// The entity that this reference uses to resolve its current/previous data references.
        /// </summary>
        IQueryableEntity Provider {
            get;
            set;
        }
    }

    /// <summary>
    /// Base type for data references for common code.
    /// </summary>
    [JsonConverter(typeof(BaseDataReferenceConverter))]
    public abstract class BaseDataReferenceType : IDataReference {
        IQueryableEntity IDataReference.Provider {
            get {
                return _provider;
            }
            set {
                _provider = value;
            }
        }
        private IQueryableEntity _provider;

        /// <summary>
        /// Returns the current data value for the given data type.
        /// </summary>
        /// <typeparam name="TData">The type of data to retrieve. It has to be one of the generic
        /// parameters for this type; if it is not, then an exception is thrown.</typeparam>
        /// <returns>The current value for the given data type.</returns>
        public TData Current<TData>() where TData : IData {
            if (VerifyRequest<TData>() == false) {
                throw new InvalidOperationException("Cannot retrieve " + typeof(TData) +
                    " with DataReference type " + GetType() +
                    "; consider adding the given Data type to the data reference");
            }

            return _provider.Current<TData>();
        }

        /// <summary>
        /// Returns the previous data value for the given data type.
        /// </summary>
        /// <typeparam name="TData">The type of data to retrieve. It has to be one of the generic
        /// parameters for this type; if it is not, then an exception is thrown.</typeparam>
        /// <returns>The current value for the given data type.</returns>
        public TData Previous<TData>() where TData : IData {
            if (VerifyRequest<TData>() == false) {
                throw new InvalidOperationException("Cannot retrieve " + typeof(TData) +
                    " with DataReference type " + GetType() +
                    "; consider adding the given Data type to the data reference");
            }

            return _provider.Previous<TData>();
        }

        /// <summary>
        /// Helper method to verify that the given generic type is one of the generic parameters for
        /// this type.
        /// </summary>
        private bool VerifyRequest<TDataRequest>() {
            Type[] acceptedDataTypes = GetType().BaseType.GetGenericArguments();
            for (int i = 0; i < acceptedDataTypes.Length; ++i) {
                if (acceptedDataTypes[i] == typeof(TDataRequest)) {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// JsonConverter used to serialize a DataReference directly as an IQueryableEntity (to avoid an
    /// extraneous object definition in the JSON output).
    /// </summary>
    internal class BaseDataReferenceConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var entity = serializer.Deserialize<IQueryableEntity>(reader);

            var reference = (IDataReference)Activator.CreateInstance(objectType);
            reference.Provider = entity;
            return reference;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            IQueryableEntity entity = ((IDataReference)value).Provider;
            serializer.Serialize(writer, entity);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam>
    public class DataReference<TData0> : BaseDataReferenceType
        where TData0 : IData {
        /// <summary>
        /// Returns the current value for the type of data that this DataReference references.
        /// </summary>
        public TData0 Current() {
            return Current<TData0>();
        }

        /// <summary>
        /// Returns the previous value for the type of data that this DataReference references.
        /// </summary>
        public TData0 Previous() {
            return Previous<TData0>();
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam>
    public class DataReference<TData0, TData1> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam>
    public class DataReference<TData0, TData1, TData2> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam> <typeparam name="TData6">A referenced data
    /// type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam> <typeparam name="TData6">A referenced data
    /// type.</typeparam> <typeparam name="TData7">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam> <typeparam name="TData6">A referenced data
    /// type.</typeparam> <typeparam name="TData7">A referenced data type.</typeparam>
    /// <typeparam name="TData8">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8> : BaseDataReferenceType { }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam> <typeparam name="TData6">A referenced data
    /// type.</typeparam> <typeparam name="TData7">A referenced data type.</typeparam>
    /// <typeparam name="TData8">A referenced data type.</typeparam> <typeparam name="TData9">A
    /// referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8, TData9> : BaseDataReferenceType { }
}