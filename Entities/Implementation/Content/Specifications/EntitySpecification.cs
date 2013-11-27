using Neon.Serialization;
using System.Collections.Generic;

namespace Neon.Entities.Implementation.Content.Specifications {
    /// <summary>
    /// Serialization specification for a ContentEntitySpecification instance.
    /// </summary>
    internal class EntitySpecification {
        public EntitySpecification(SerializedData entityData) {
            PrettyName = entityData.AsDictionary["PrettyName"].AsString;
            UniqueId = entityData.AsDictionary["UniqueId"].AsReal.AsInt;
            IsAdding = entityData.AsDictionary["IsAdding"].AsBool;
            IsRemoving = entityData.AsDictionary["IsRemoving"].AsBool;

            Data = new List<EntityDataSpecification>();
            foreach (var data in entityData.AsDictionary["Data"].AsList) {
                Data.Add(new EntityDataSpecification(data));
            }
        }

        public SerializedData Export() {
            Dictionary<string, SerializedData> dict = new Dictionary<string, SerializedData>();

            dict["PrettyName"] = new SerializedData(PrettyName);
            dict["UniqueId"] = new SerializedData(UniqueId);
            dict["IsAdding"] = new SerializedData(IsAdding);
            dict["IsRemoving"] = new SerializedData(IsRemoving);

            List<SerializedData> data = new List<SerializedData>();
            foreach (var dataSpec in Data) {
                data.Add(dataSpec.Export());
            }

            dict["Data"] = new SerializedData(data);

            return new SerializedData(dict);
        }

        public EntitySpecification(IEntity entity, bool isAdding, bool isRemoving, SerializationConverter converter) {
            PrettyName = entity.PrettyName;
            UniqueId = entity.UniqueId;

            IsAdding = isAdding;
            IsRemoving = isRemoving;

            Data = new List<EntityDataSpecification>();
            foreach (var data in entity.SelectCurrentData()) {
                DataAccessor accessor = new DataAccessor(data);

                EntityDataSpecification spec = new EntityDataSpecification(
                    current: entity.Current(accessor),
                    previous: entity.Previous(accessor),
                    wasModified: entity.WasModified(accessor),
                    wasAdded: entity.WasAdded(accessor),
                    wasRemoved: entity.WasRemoved(accessor),
                    converter: converter);
                Data.Add(spec);
            }
        }

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
        public List<EntityDataSpecification> Data;

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