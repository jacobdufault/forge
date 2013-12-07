using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Neon.Serialization {
    public class SerializationGraphExporter {
        private Dictionary<Type, ObjectIDGenerator> _ids;
        private List<SerializedData> _data;
        private Dictionary<Type, Dictionary<int, object>> _objects;

        private Stack<Tuple<Type, int>> _toExport;

        public SerializationGraphExporter() {
            _ids = new Dictionary<Type, ObjectIDGenerator>();
            _data = new List<SerializedData>();
            _objects = new Dictionary<Type, Dictionary<int, object>>();
            _toExport = new Stack<Tuple<Type, int>>();
        }

        private ObjectIDGenerator GetIdGenerator(Type type) {
            ObjectIDGenerator value;
            if (_ids.TryGetValue(type, out value) == false) {
                value = new ObjectIDGenerator();
                _ids[type] = value;
            }
            return value;
        }

        private int GetId(Type type, object reference) {
            bool firstTime;
            int id = (int)GetIdGenerator(type).GetId(reference, out firstTime);

            if (firstTime) {
                Dictionary<int, object> definitions;
                if (_objects.TryGetValue(type, out definitions) == false) {
                    definitions = new Dictionary<int, object>();
                    _objects[type] = definitions;
                }

                definitions[id] = reference;

                _toExport.Push(Tuple.Create(type, id));
            }

            return id;
        }

        /// <summary>
        /// Returns a SerializedData instance that is a reference to the given object instance.
        /// </summary>
        /// <param name="type">The type of the object instance.</param>
        /// <param name="reference">The object instance to reference.</param>
        /// <returns>A SerializedData instance that is an object reference.</returns>
        public SerializedData GetReferenceForObject(Type type, object reference) {
            return SerializedData.CreateObjectReference(GetId(type, reference));
        }

        /*
        /// <summary>
        /// Marks the given data instance as the object definition for the given object. Only one
        /// serialized data instance can be the object definition; if another serialized data
        /// instance has been marked as the object definition for the given reference, then an
        /// InvalidOperationException is thrown.
        /// </summary>
        /// <param name="reference">The object reference that is the primary definition.</param>
        /// <param name="type">The type of the object reference.</param>
        /// <param name="data">The data that contains the serialized state of the primary reference.
        /// This is modified to signify that it is the object definition for this reference.</param>
        public void MarkAsObjectDefinition(object reference, Type type, SerializedData data) {
            int id = GetId(type, reference);

            if (_objects[type][id].Exists) {
                throw new InvalidOperationException("The given object reference " + reference +
                    " has already been marked as an object definition");
            }
            _objects[type][id] = Maybe.Just(data);

            data.SetObjectDefinition(id);
        }
        */

        public SerializedData Export(SerializationConverter converter) {
            Dictionary<string, SerializedData> result = new Dictionary<string, SerializedData>();

            while (_toExport.Count > 0) {
                Tuple<Type, int> tuple = _toExport.Pop();
                Type type = tuple.Item1;
                int id = tuple.Item2;
                object reference = _objects[type][id];

                SerializedData definition = converter.Export(type, reference, forceCyclic: true);
                definition.SetObjectDefinition(id);

                SerializedData items;
                if (result.TryGetValue(type.FullName, out items) == false) {
                    items = SerializedData.CreateDictionary();
                    result[type.FullName] = items;
                }

                items.AsDictionary[id.ToString()] = definition;
            }

            return new SerializedData(result);
        }

        /*
        public SerializedData Export(SerializationConverter converter) {
            Dictionary<string, SerializedData> result = new Dictionary<string, SerializedData>();

            try {

                foreach (var groupEntry in _objects) {
                    Type type = groupEntry.Key;
                    Dictionary<int, object> items = groupEntry.Value;

                    Dictionary<string, SerializedData> serializedItems = new Dictionary<string, SerializedData>();
                    foreach (var entry in items) {
                        SerializedData definition = converter.Export(type, entry.Value, forceCyclic: true);
                        definition.SetObjectDefinition(entry.Key);

                        serializedItems.Add(entry.Key.ToString(), definition);
                    }

                    result[type.FullName] = new SerializedData(serializedItems);
                }
            }
            catch (InvalidOperationException e) {
                throw new InvalidOperationException("Failed to export all referenced items in " +
                    "object graph", e);
            }

            return new SerializedData(result);
        }
        */

        /*
        public void Add(object reference, SerializationConverter converter) {
            bool firstTime;
            long id = _ids.GetId(reference, out firstTime);

            Dictionary<string, SerializedData> data = new Dictionary<string, SerializedData>();

            data["ReferenceId"] = new SerializedData(id);
            data["IsDefinition"] = new SerializedData(firstTime);
            data["Content"] = converter.Export(_type, reference);
        }

        public SerializedData GetGraph() {
            return new SerializedData(_data);
        }
        */
    }

    public class SerializationGraphImporter {
        private struct RestoredObject {
            public object Reference;
            public SerializedData SerializedState;
        }

        private Dictionary<Type, Dictionary<int, RestoredObject>> _objects;
        private Stack<int> _toRestore;

        public SerializationGraphImporter(SerializedData data) {
            _objects = new Dictionary<Type, Dictionary<int, RestoredObject>>();
            _toRestore = new Stack<int>();

            foreach (var entry in data.AsDictionary) {
                // get the type of the current set of objects
                Type type = TypeCache.FindType(entry.Key);
                TypeMetadata metadata = TypeCache.GetMetadata(type);

                _objects[type] = new Dictionary<int, RestoredObject>();

                // create our initial references for the given objects in the graph
                foreach (var item in entry.Value.AsDictionary) {
                    int id = item.Value.AsObjectDefinition;
                    object instance = metadata.CreateInstance();

                    _objects[type][id] = new RestoredObject() {
                        Reference = instance,
                        SerializedState = item.Value
                    };
                }
            }
        }

        /// <summary>
        /// Restores all object references in the graph.
        /// </summary>
        public void RestoreGraph(SerializationConverter converter) {
            foreach (var entry in _objects) {
                Type type = entry.Key;
                foreach (var item in entry.Value) {
                    RestoredObject restored = item.Value;
                    converter.Import(type, restored.SerializedState, restored.Reference);
                }
            }
        }

        /// <summary>
        /// Returns the object reference for the given type with the given reference value.
        /// </summary>
        /// <param name="type">The type of the object we are retrieving the reference for.</param>
        /// <param name="objectReference">The reference identifier.</param>
        /// <returns>An object reference.</returns>
        public object GetObjectInstance(Type type, int objectReference) {
            return _objects[type][objectReference].Reference;
        }

        /// <summary>
        /// Restores all objects in the graph that are of the given type, and returns all of the
        /// restored objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
        /// <returns></returns>
        /*
         * TODO: should this be a method that is called directly from the converter?
        public List<T> Restore<T>(SerializationConverter converter) {
            List<T> result = new List<T>();

            foreach (var obj in _objects[typeof(T)]) {
                converter.Import(typeof(T), obj.Value.SerializedState, obj.Value.Reference);
                result.Add((T)obj.Value.Reference);
            }

            return result;
        }
        */

        /*
        public List<object> Import(SerializedData graph, SerializationConverter converter) {
            foreach (SerializedData graphEntry in graph.AsList) {
                Add(graphEntry);
            }

            RestoreObjects(converter);

            List<object> items = new List<object>();
            foreach (var value in _objects.Values) {
                items.Add(value.Item2);
            }
            return items;
        }

        private void Add(SerializedData data) {
            //
            // cyclic item definition
            //
            // {
            // IsDefinition: true
            // ReferenceId: 0
            // Content: # whatever
            // }
            //
            // reference to another cyclic item
            //
            // {
            // IsDefinition: false
            // ReferenceId: 0
            // }
            //

            bool isDefinition = data.AsDictionary["IsDefinition"].AsBool;
            int id = data.AsDictionary["ReferenceId"].AsReal.AsInt;

            // if we are a definition, then we contain the actual data to deserialize the data
            if (isDefinition) {
                SerializedData content = data.AsDictionary["Content"];

                // but if we already have a key for the id, then we have the object instance to
                // deserialize into but not the serialized data to restore from
                if (_objects.ContainsKey(id)) {
                    _objects[id].Item1 = content;
                }
                // we don't have a key for this id yet, so create the instance
                else {
                    _toRestore.Push(id);
                    _objects[id] = new Tuple<SerializedData, object>(content, _type.CreateInstance());
                }
            }

            // we're not a definition, so we need to create an object instance but only if we don't
            // already have one
            else if (_objects.ContainsKey(id) == false) {
                _toRestore.Push(id);
                _objects[id] = new Tuple<SerializedData, object>(null, _type.CreateInstance());
            }
        }

        //public object GetReference(int id) {
        //    return _objects[id].Item2;
        //}

        /// <summary>
        /// Uses the given serialization converter to restore all of the objects in the graph. Use
        /// GetReference to get the instance reference to the target object.
        /// </summary>
        /// <remarks>
        /// This throws an InvalidOperationException if a reference in the graph cannot be restored
        /// because it has no data to restore from.
        /// </remarks>
        /// <param name="converter">The converter to use when restoring objects.</param>
        public void RestoreObjects(SerializationConverter converter) {
            while (_toRestore.Count > 0) {
                int id = _toRestore.Pop();

                SerializedData content = _objects[id].Item1;
                object reference = _objects[id].Item2;

                if (content == null) {
                    throw new InvalidOperationException("No content found for reference " + id +
                        " (with type " + _type.ReflectedType + ")");
                }

                converter.Import(_type.ReflectedType, content, reference);
            }
        }
        */
    }
}