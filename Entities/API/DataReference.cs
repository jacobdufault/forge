using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public abstract class BaseDataReferenceType : IDataReferenceTypeEraser {
        private IQueryableEntity _entity;

        public TData Get<TData>() where TData : IData {
            if (VerifyRequest<TData>() == false) {
                throw new InvalidOperationException("Cannot retrieve " + typeof(TData) +
                    " with DataReference type " + GetType() +
                    "; consider adding the given Data type to the data reference");
            }

            return _entity.Current<TData>();
        }

        protected abstract bool VerifyRequest<TDataRequest>() where TDataRequest : IData;

        IQueryableEntity IDataReferenceTypeEraser.Provider {
            get {
                return _entity;
            }
            set {
                _entity = value;
            }
        }
    }

    /// <summary>
    /// Data reference that references some data defined in an entity or template.
    /// </summary>
    /// <typeparam name="TData0">A referenced data type.</typeparam>
    public class DataReference<TData0> : BaseDataReferenceType
        where TData0 : IData {
        public TData0 Get() {
            return Get<TData0>();
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