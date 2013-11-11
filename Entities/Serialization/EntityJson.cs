using Neon.Serialization;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Serialization {
    /// <summary>
    /// JSON specification for an instance of Data inside of an IEntity.
    /// </summary>
    public class DataJson {
        /// <summary>
        /// The type name of the data that this data item maps to.
        /// </summary>
        public string DataType;

        /// <summary>
        /// True if the data was modified in the last update.
        /// </summary>
        public bool WasModified;

        /// <summary>
        /// True if the data needs to be added in the next update.
        /// </summary>
        public bool IsAdding;

        /// <summary>
        /// True if the data needs to be removed in the next update.
        /// </summary>
        public bool IsRemoving;

        /// <summary>
        /// The previous state of the data. We can only deserialize this when we have resolved the
        /// data type.
        /// </summary>
        public SerializedData PreviousState;

        /// <summary>
        /// The current state of the data, in JSON form. We can only deserialize this when we have
        /// resolved the data type.
        /// </summary>
        public SerializedData CurrentState;

        [NonSerialized]
        private Data _deserializedPreviousState;
        public Data GetDeserializedPreviousState(SerializationConverter converter) {
            if (_deserializedPreviousState == null) {
                _deserializedPreviousState = (Data)converter.Import(TypeCache.FindType(DataType), PreviousState);
            }
            return _deserializedPreviousState;
        }

        [NonSerialized]
        private Data _deserializedCurrentState;
        public Data GetDeserializedCurrentState(SerializationConverter converter) {
            if (_deserializedCurrentState == null) {
                _deserializedCurrentState = (Data)converter.Import(TypeCache.FindType(DataType), CurrentState);
            }
            return _deserializedCurrentState;
        }
    }

    /// <summary>
    /// JSON specification for an IEntity instance.
    /// </summary>
    public class EntityJson {
        /// <summary>
        /// The pretty name for the entity. This is optional and can be null (on read).
        /// </summary>
        public string PrettyName;

        /// <summary>
        /// The entities unique id.
        /// </summary>
        public int UniqueId;

        /// <summary>
        /// The data that is contained within the entity.
        /// </summary>
        public List<DataJson> Data;

        /// <summary>
        /// Does the entity need to be added to the EntityManager in the next update?
        /// </summary>
        public bool IsAdding;

        /// <summary>
        /// Does the entity need to be removed from the EntityManager in the next update?
        /// </summary>
        public bool IsRemoving;
    }
}