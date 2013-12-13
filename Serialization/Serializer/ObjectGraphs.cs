using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Neon.Serialization {
    /// <summary>
    /// Helper code that supports restoring an object graph that contains cycles; that is, an object
    /// graph that contains a set of cyclic references.
    /// </summary>
    internal class ObjectGraphReader {
        /// <summary>
        /// Stores an object that we are restoring. We first have to create an object reference
        /// before actually restoring the object, so initially the reference inside of
        /// RestoredObject is not initialized.
        /// </summary>
        private struct RestoredObject {
            public object Reference;
            public SerializedData SerializedState;
        }

        /// <summary>
        /// The objects to restore. The dictionary is indexed by the type of object and then by the
        /// object id.
        /// </summary>
        private Dictionary<Type, Dictionary<int, RestoredObject>> _objects;

        /// <summary>
        /// Creates a new empty serialization graph importer.
        /// </summary>
        /// <param name="data">The object graph that was exported using the
        /// SerializationGraphExporter.</param>
        public ObjectGraphReader(SerializedData data) {
            _objects = new Dictionary<Type, Dictionary<int, RestoredObject>>();

            foreach (var entry in data.AsDictionary) {
                // get the type of the current set of objects
                Type type = TypeCache.FindType(entry.Key);
                TypeModel model = TypeCache.GetTypeModel(type);

                _objects[type] = new Dictionary<int, RestoredObject>();

                // create our initial references for the given objects in the graph
                foreach (var item in entry.Value.AsList) {
                    int id = item.AsObjectDefinition;
                    object instance = model.CreateInstance();

                    _objects[type][id] = new RestoredObject() {
                        Reference = instance,
                        SerializedState = item
                    };
                }
            }
        }

        /// <summary>
        /// Restores all object references in the graph.
        /// </summary>
        public void RestoreGraph() {
            foreach (var entry in _objects) {
                Type type = entry.Key;
                ITypeConverter converter = TypeConverterResolver.GetTypeConverter(type, allowCyclic: false);

                foreach (var item in entry.Value) {
                    RestoredObject restored = item.Value;
                    converter.Import(restored.SerializedState, this);
                }
            }
        }

        /// <summary>
        /// Returns the object reference for the given type with the given reference value.
        /// </summary>
        /// <param name="model">The type of the object we are retrieving the reference for.</param>
        /// <param name="objectReference">The reference identifier.</param>
        /// <returns>An object reference.</returns>
        internal object GetObjectInstance(TypeModel model, SerializedData data) {
            Type type = model.ReflectedType;

            if (data.IsObjectDefinition) {
                return _objects[type][data.AsObjectDefinition].Reference;
            }
            if (data.IsObjectReference) {
                return _objects[type][data.AsObjectReference].Reference;
            }

            return model.CreateInstance();
        }

    }

    /// <summary>
    /// Helper code that supports the serialization of object graphs which have reference cycles
    /// within them. Stated differently, this class assists the serialization converter for
    /// exporting references which, via internal data, reference themselves across some reference
    /// chain.
    /// </summary>
    internal class ObjectGraphWriter {
        /// <summary>
        /// Each type has its own id generator. This dictionary contains the id generator for each
        /// type.
        /// </summary>
        private Dictionary<Type, ObjectIDGenerator> _ids;

        /// <summary>
        /// Contains all of the objects which are in the serialization graph. Items are added to
        /// this dictionary when a reference has been requested to it. When an item is added to the
        /// dictionary, it is also pushed onto _toExport, which means that it will be fully exported
        /// in the second serialization pass.
        /// </summary>
        private Dictionary<Type, Dictionary<int, object>> _objects;

        /// <summary>
        /// The list of items that need to be serialized. The type is the type of item to serialize,
        /// and the int is the id of the item that was generated using the ObjectIDGenerator. The
        /// reference for the original item can be recovered from _objects.
        /// </summary>
        private Stack<Tuple<Type, int>> _toExport;

        /// <summary>
        /// Creates a new empty serialization export graph.
        /// </summary>
        public ObjectGraphWriter() {
            _ids = new Dictionary<Type, ObjectIDGenerator>();
            _objects = new Dictionary<Type, Dictionary<int, object>>();
            _toExport = new Stack<Tuple<Type, int>>();
        }

        /// <summary>
        /// Helper method that returns the object id generator for the given type.
        /// </summary>
        private ObjectIDGenerator GetIdGenerator(Type type) {
            ObjectIDGenerator value;
            if (_ids.TryGetValue(type, out value) == false) {
                value = new ObjectIDGenerator();
                _ids[type] = value;
            }
            return value;
        }

        /// <summary>
        /// Helper method that returns the unique identifier for the given reference, assuming the
        /// given type for the reference.
        /// </summary>
        /// <param name="type">The type to use when retrieving the id.</param>
        /// <param name="reference">The reference itself to retrieve the id for.</param>
        /// <returns>A unique identifier that is unique to the given reference.</returns>
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
        /// <param name="reference">The object instance to reference.</param>
        /// <returns>A SerializedData instance that is an object reference.</returns>
        internal SerializedData GetObjectReference(object instance) {
            return SerializedData.CreateObjectReference(GetId(instance.GetType(), instance));
        }

        /// <summary>
        /// Exports all data inside of the serialization graph. The graph can be reconstructed using
        /// the SerializationGraphImporter.
        /// </summary>
        /// <param name="converter">The serialization converter to use when exporting the
        /// graph.</param>
        /// <returns>The serialized graph.</returns>
        public SerializedData Export() {
            Dictionary<string, SerializedData> result = new Dictionary<string, SerializedData>();

            while (_toExport.Count > 0) {
                Tuple<Type, int> tuple = _toExport.Pop();
                Type type = tuple.Item1;
                int id = tuple.Item2;
                object reference = _objects[type][id];

                ITypeConverter converter = TypeConverterResolver.GetTypeConverter(type, allowCyclic: false);
                SerializedData definition = converter.Export(reference, this);
                definition.SetObjectDefinition(id);

                SerializedData items;
                if (result.TryGetValue(type.FullName, out items) == false) {
                    items = SerializedData.CreateList();
                    result[type.FullName] = items;
                }

                items.AsList.Add(definition);
            }

            return new SerializedData(result);
        }
    }
}