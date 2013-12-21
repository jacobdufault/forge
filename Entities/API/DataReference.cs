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

    /// <summary>
    /// Base type for data references for common code.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseDataReferenceType : IDataReferenceTypeEraser {
        IQueryableEntity IDataReferenceTypeEraser.Provider {
            get {
                return _queryableEntity;
            }
            set {
                _queryableEntity = value;
                UpdateSerializedValues();
            }
        }
        private IQueryableEntity _queryableEntity;

        /// <summary>
        /// Updates that values that will be serialized and used to restore the DataReferenced based
        /// upon the current value in _queryableEntity.
        /// </summary>
        private void UpdateSerializedValues() {
            IEntity asEntity = _queryableEntity as IEntity;
            if (asEntity != null) {
                ReferencedId = asEntity.UniqueId;
                IsEntityReference = true;
            }

            else {
                ITemplate template = (ITemplate)_queryableEntity;
                ReferencedId = template.TemplateId;
                IsEntityReference = false;
            }
        }

        /// <summary>
        /// The id of the entity or template that we reference. We serialize this and it will be
        /// resolved later.
        /// </summary>
        [JsonProperty("ReferencedId")]
        internal int ReferencedId;

        [JsonProperty("IsEntityReference")]
        internal bool IsEntityReference;

        /// <summary>
        /// Resolves the reference to a specific entity.
        /// </summary>
        /// <param name="entity">The entity that the reference will reference.</param>
        internal void ResolveEntityId(IQueryableEntity entity) {
            // validate that the given entity is actually the entity we are saved to
            if (IsEntityReference && entity is IEntity == false) {
                throw new InvalidOperationException("Reference references IEntity but resolved entity is not an IEntity");
            }
            if (IsEntityReference == false && entity is ITemplate == false) {
                throw new InvalidOperationException("Reference references ITemplate but resolved entity is not an ITemplate");
            }

            int id = -1;
            if (IsEntityReference) {
                id = ((IEntity)entity).UniqueId;
            }
            else {
                id = ((ITemplate)entity).TemplateId;
            }

            if (id != ReferencedId) {
                throw new InvalidOperationException("The resolved entity has a different id than " +
                    "the one the reference references (got " + id + ", expected " + ReferencedId +
                    ")");
            }

            // done validating; assign the entity reference
            _queryableEntity = entity;
        }

        /// <summary>
        /// This method will automatically be called when we are deserializing the object. We want
        /// to add ourselves to the list of data references in the context so that we will get
        /// restored.
        /// </summary>
        [OnDeserializing]
        private void OnSerialization(StreamingContext context) {
            GeneralStreamingContext generalContext = (GeneralStreamingContext)context.Context;
            generalContext.Get<DataReferenceContextObject>().DataReferences.Add(this);
        }

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