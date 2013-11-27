using Neon.Serialization;
using System;

namespace Neon.Entities.Implementation.Content {
    /// <summary>
    /// Serialization specification for an instance of Data inside of an IEntity.
    /// </summary>
    internal class EntityDataSpecification {
        public EntityDataSpecification(SerializedData data) {
            DataType = data.AsDictionary["DataType"].AsString;

            WasModified = data.AsDictionary["WasModified"].AsBool;
            WasAdded = data.AsDictionary["WasAdded"].AsBool;
            WasRemoved = data.AsDictionary["WasRemoved"].AsBool;

            PreviousState = data.AsDictionary["Previous"];
            CurrentState = data.AsDictionary["Current"];
        }

        public EntityDataSpecification(IData current, IData previous, bool wasModified,
            bool wasAdded, bool wasRemoved, SerializationConverter converter) {
            Type dataType = current.GetType();

            DataType = dataType.FullName;

            WasModified = wasModified;
            WasAdded = wasAdded;
            WasRemoved = wasRemoved;

            PreviousState = converter.Export(dataType, previous);
            CurrentState = converter.Export(dataType, current);
        }

        /// <summary>
        /// The type name of the data that this data item maps to.
        /// </summary>
        public string DataType;

        /// <summary>
        /// True if the data was modified in the last update.
        /// </summary>
        public bool WasModified;

        /// <summary>
        /// True if the data was added in the last update.
        /// </summary>
        public bool WasAdded;

        /// <summary>
        /// True if the data was removed in the last update.
        /// </summary>
        public bool WasRemoved;

        /// <summary>
        /// The previous state of the data. We can only deserialize this when we have resolved the
        /// data type. If the previous state is identical to the current state, then this can be
        /// null.
        /// </summary>
        public SerializedData PreviousState;

        /// <summary>
        /// The current serialized state of the data. We can only deserialize this when we have
        /// resolved the data type.
        /// </summary>
        public SerializedData CurrentState;
    }
}