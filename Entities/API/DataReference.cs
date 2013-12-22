using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Neon.Entities {
    /// <summary>
    /// Interface used for type erasure by BaseDataReferenceType.
    /// </summary>
    public interface IDataReferenceTypeEraser {
        IQueryableEntity Provider {
            get;
            set;
        }
    }

    internal class BaseDataReferenceConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var entity = serializer.Deserialize<IQueryableEntity>(reader);

            var reference = (IDataReferenceTypeEraser)Activator.CreateInstance(objectType);
            reference.Provider = entity;
            return reference;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            serializer.Serialize(writer, ((IDataReferenceTypeEraser)value).Provider, typeof(IQueryableEntity));
        }
    }

    /// <summary>
    /// Base type for data references for common code.
    /// </summary>
    [JsonConverter(typeof(BaseDataReferenceConverter))]
    public abstract class BaseDataReferenceType : IDataReferenceTypeEraser {
        IQueryableEntity IDataReferenceTypeEraser.Provider {
            get {
                return _queryableEntity;
            }
            set {
                _queryableEntity = value;
                //UpdateSerializedValues();
            }
        }
        private IQueryableEntity _queryableEntity;

        /*
        /// <summary>
        /// Updates that values that will be serialized and used to restore the DataReferenced based
        /// upon the current value in _queryableEntity.
        /// </summary>
        private void UpdateSerializedValues() {
            IEntity asEntity = _queryableEntity as IEntity;
            if (asEntity != null) {
                _referencedId = asEntity.UniqueId;
                _referenceType = EntityReferenceType.EntityReference;
            }

            else {
                ITemplate template = (ITemplate)_queryableEntity;
                _referencedId = template.TemplateId;
                _referenceType = EntityReferenceType.TemplateReference;
            }
        }

        /// <summary>
        /// The id of the entity or template that we reference. We serialize this and it will be
        /// resolved later.
        /// </summary>
        [JsonProperty("ReferencedId")]
        internal int _referencedId;

        /// <summary>
        /// Specifies the types of entities that this DataReference can reference.
        /// </summary>
        internal enum EntityReferenceType {
            TemplateReference,
            EntityReference
        }

        /// <summary>
        /// The type of entity that we are referencing (either an IEntity or an ITemplate).
        /// </summary>
        [JsonProperty("ReferenceType")]
        internal EntityReferenceType _referenceType;

        [OnDeserialized]
        private void ResolveEntity(StreamingContext context) {
            var generalContext = (GeneralStreamingContext)context.Context;
            var engineContext = generalContext.Get<GameEngineContext>();

            if (_referenceType == EntityReferenceType.EntityReference) {
                var entityContext = generalContext.Get<EntityConversionContext>();
                _queryableEntity = entityContext.GetEntityInstance(_referencedId, engineContext);
            }

            else {
                var templateContext = generalContext.Get<TemplateConversionContext>();
                _queryableEntity = templateContext.GetTemplateInstance(_referencedId, engineContext);
            }
        }
        */

        public TData Current<TData>() where TData : IData {
            if (VerifyRequest<TData>() == false) {
                throw new InvalidOperationException("Cannot retrieve " + typeof(TData) +
                    " with DataReference type " + GetType() +
                    "; consider adding the given Data type to the data reference");
            }

            return _queryableEntity.Current<TData>();
        }

        public TData Previous<TData>() where TData : IData {
            if (VerifyRequest<TData>() == false) {
                throw new InvalidOperationException("Cannot retrieve " + typeof(TData) +
                    " with DataReference type " + GetType() +
                    "; consider adding the given Data type to the data reference");
            }

            return _queryableEntity.Previous<TData>();
        }

        protected abstract bool VerifyRequest<TDataRequest>() where TDataRequest : IData;
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam>
    public class DataReference<TData0> : BaseDataReferenceType
        where TData0 : IData {
        public TData0 Current() {
            return Current<TData0>();
        }

        public TData0 Previous() {
            return Previous<TData0>();
        }

        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam>
    public class DataReference<TData0, TData1> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam>
    public class DataReference<TData0, TData1, TData2> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3) ||
                typeof(TDataRequest) == typeof(TData4);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3) ||
                typeof(TDataRequest) == typeof(TData4) ||
                typeof(TDataRequest) == typeof(TData5);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam> <typeparam name="TData6">A referenced data
    /// type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3) ||
                typeof(TDataRequest) == typeof(TData4) ||
                typeof(TDataRequest) == typeof(TData5) ||
                typeof(TDataRequest) == typeof(TData6);
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam> <typeparam name="TData1">A
    /// referenced data type.</typeparam> <typeparam name="TData2">A referenced data
    /// type.</typeparam> <typeparam name="TData3">A referenced data type.</typeparam>
    /// <typeparam name="TData4">A referenced data type.</typeparam> <typeparam name="TData5">A
    /// referenced data type.</typeparam> <typeparam name="TData6">A referenced data
    /// type.</typeparam> <typeparam name="TData7">A referenced data type.</typeparam>
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3) ||
                typeof(TDataRequest) == typeof(TData4) ||
                typeof(TDataRequest) == typeof(TData5) ||
                typeof(TDataRequest) == typeof(TData6) ||
                typeof(TDataRequest) == typeof(TData7);
        }
    }

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
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3) ||
                typeof(TDataRequest) == typeof(TData4) ||
                typeof(TDataRequest) == typeof(TData5) ||
                typeof(TDataRequest) == typeof(TData6) ||
                typeof(TDataRequest) == typeof(TData7) ||
                typeof(TDataRequest) == typeof(TData8);
        }
    }

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
    public class DataReference<TData0, TData1, TData2, TData3, TData4, TData5, TData6, TData7, TData8, TData9> : BaseDataReferenceType {
        protected override bool VerifyRequest<TDataRequest>() {
            return
                typeof(TDataRequest) == typeof(TData0) ||
                typeof(TDataRequest) == typeof(TData1) ||
                typeof(TDataRequest) == typeof(TData2) ||
                typeof(TDataRequest) == typeof(TData3) ||
                typeof(TDataRequest) == typeof(TData4) ||
                typeof(TDataRequest) == typeof(TData5) ||
                typeof(TDataRequest) == typeof(TData6) ||
                typeof(TDataRequest) == typeof(TData7) ||
                typeof(TDataRequest) == typeof(TData8) ||
                typeof(TDataRequest) == typeof(TData9);
        }
    }
}