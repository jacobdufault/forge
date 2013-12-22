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

using Neon.Collections;
using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.Entities.Implementation.Content {
    [JsonObject(MemberSerialization.OptIn)]
    internal class ContentEntitySerializationFormat {
        [JsonProperty("Data")]
        public List<ContentEntity.DataInstance> Data;

        [JsonProperty("UniqueId")]
        public int UniqueId;

        [JsonProperty("PrettyName")]
        public string PrettyName;
    }

    [JsonConverter(typeof(QueryableEntityConverter))]
    internal class ContentEntity : IEntity {
        [JsonObject(MemberSerialization.OptIn)]
        public class DataInstance {
            [JsonProperty("CurrentData")]
            public IData CurrentData;
            [JsonProperty("PreviousData")]
            public IData PreviousData;

            /// <summary>
            /// Did the data get added in the last updated frame?
            /// </summary>
            [JsonProperty("WasAdded")]
            public bool WasAdded;

            /// <summary>
            /// Did the data get removed in the last update frame?
            /// </summary>
            [JsonProperty("WasRemoved")]
            public bool WasRemoved;

            /// <summary>
            /// Did the data get modified in the last update frame?
            /// </summary>
            [JsonProperty("WasModified")]
            public bool WasModified;
        }

        private SparseArray<DataInstance> _data;

        private EventNotifier _eventNotifier;

        public int UniqueId {
            get;
            private set;
        }

        public string PrettyName {
            get;
            set;
        }

        public ContentEntitySerializationFormat GetSerializedFormat() {
            return new ContentEntitySerializationFormat() {
                PrettyName = PrettyName,
                UniqueId = UniqueId,
                Data = _data.Select(pair => pair.Value).ToList()
            };
        }

        public ContentEntity() {
            _data = new SparseArray<DataInstance>();
            _eventNotifier = new EventNotifier();
        }

        public ContentEntity(int uniqueId, string prettyName)
            : this() {
            UniqueId = uniqueId;
            PrettyName = prettyName ?? "";
        }

        public void Initialize(ContentEntitySerializationFormat format) {
            PrettyName = format.PrettyName ?? "";
            UniqueId = format.UniqueId;

            foreach (DataInstance data in format.Data) {
                _data[DataAccessorFactory.GetId(data.CurrentData)] = data;
            }
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Entity [uid={0}, name={1}]", UniqueId, PrettyName);
            }
            else {
                return string.Format("Entity [uid={0}]", UniqueId);
            }
        }

        public void Destroy() {
            throw new InvalidOperationException("Cannot destroy a ContentEntity; use GameSnapshot.Remove instead");
        }

        public IData AddOrModify(DataAccessor accessor) {
            throw new InvalidOperationException("Cannot AddOrModify data in a ContentEntity (use Add)");
        }

        public IData AddData(DataAccessor accessor) {
            if (ContainsData(accessor) == true) {
                throw new AlreadyAddedDataException(this, accessor);
            }

            Type dataType = DataAccessorFactory.GetTypeFromAccessor(accessor);
            _data[accessor.Id] = new DataInstance() {
                CurrentData = (IData)Activator.CreateInstance(dataType),
                PreviousData = (IData)Activator.CreateInstance(dataType),
                WasAdded = true,
                WasModified = false,
                WasRemoved = false
            };

            return _data[accessor.Id].CurrentData;
        }

        public void RemoveData(DataAccessor accessor) {
            if (_data.Contains(accessor.Id) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            // If the data is being added, then just remove it. Otherwise, add to to the removed
            // collection.

            if (_data[accessor.Id].WasAdded) {
                _data.Remove(accessor.Id);
            }
            else {
                _data[accessor.Id].WasRemoved = true;
            }
        }

        public IData Modify(DataAccessor accessor) {
            throw new InvalidOperationException("Cannot modify data in a ContentEntity");
        }

        public ICollection<IData> SelectCurrentData(Predicate<IData> filter = null,
            ICollection<IData> storage = null) {
            if (storage == null) {
                storage = new List<IData>();
            }

            foreach (var pair in _data) {
                IData data = pair.Value.CurrentData;
                if (filter == null || filter(data)) {
                    storage.Add(data);
                }
            }

            return storage;
        }

        public IEventNotifier EventNotifier {
            get { return _eventNotifier; }
        }

        public IData Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id].CurrentData;
        }

        public IData Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id].PreviousData;
        }

        public bool ContainsData(DataAccessor accessor) {
            return _data.Contains(accessor.Id) && _data[accessor.Id].WasRemoved == false;
        }

        public bool WasModified(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id].WasModified;
        }

        public bool WasAdded(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _data[accessor.Id].WasAdded;
        }

        public bool WasRemoved(DataAccessor accessor) {
            return _data.Contains(accessor.Id) && _data[accessor.Id].WasRemoved;
        }
    }
}