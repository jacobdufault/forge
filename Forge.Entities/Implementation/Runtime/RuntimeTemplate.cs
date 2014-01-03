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

using Forge.Collections;
using Forge.Entities.Implementation.Content;
using Forge.Entities.Implementation.Runtime;
using Forge.Entities.Implementation.Shared;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Forge.Entities.Implementation.Runtime {
    /// <summary>
    /// A runtime version of an ITemplate designed for efficiency.
    /// </summary>
    [JsonConverter(typeof(QueryableEntityConverter))]
    internal class RuntimeTemplate : ITemplate {
        /// <summary>
        /// The data instances inside of the template.
        /// </summary>
        private SparseArray<Data.IData> _defaultDataInstances;

        /// <summary>
        /// The game engine that entities are added to when they are instantiated.
        /// </summary>
        private GameEngine _gameEngine;

        public RuntimeTemplate(int templateId, GameEngine engine) {
            _defaultDataInstances = new SparseArray<Data.IData>();
            _gameEngine = engine;
            TemplateId = templateId;
            PrettyName = "";
        }

        /// <summary>
        /// Initializes the RuntimeTemplate with data from the given ContentTemplate.
        /// </summary>
        public void Initialize(ContentTemplateSerializationFormat template) {
            TemplateId = template.TemplateId;
            PrettyName = template.PrettyName;

            foreach (Data.IData data in template.DefaultDataInstances) {
                _defaultDataInstances[DataAccessorFactory.GetId(data)] = data;
            }
        }

        /// <summary>
        /// Adds a default data instance to the template. The template "owns" the passed data
        /// instance; a copy is not made of it.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="data">The data instance to copy from.</param>
        public void AddDefaultData(Data.IData data) {
            throw new InvalidOperationException("Template cannot be modified while game is being played");
        }

        /// <summary>
        /// Remove the given type of data from the template instance. New instances will not longer
        /// have this added to the template.
        /// </summary>
        /// <remarks>
        /// If the ITemplate is currently being backed by an IGameEngine, this will throw an
        /// InvalidOperationException.
        /// </remarks>
        /// <param name="accessor">The type of data to remove.</param>
        /// <returns>True if the data was removed.</returns>
        public bool RemoveDefaultData(DataAccessor accessor) {
            throw new InvalidOperationException("Template cannot be modified while game is being played");
        }

        public int TemplateId {
            get;
            private set;
        }

        public IEntity InstantiateEntity() {
            int id = _gameEngine.EntityIdGenerator.Next();
            EventNotifier eventNotifier = _gameEngine.EventNotifier;

            RuntimeEntity entity = new RuntimeEntity(id, this, eventNotifier);

            _gameEngine.AddEntity(entity);

            return entity;
        }

        public ICollection<DataAccessor> SelectData(bool includeRemoved = false,
            Predicate<DataAccessor> filter = null, ICollection<DataAccessor> storage = null) {
            if (storage == null) {
                storage = new List<DataAccessor>();
            }

            foreach (var pair in _defaultDataInstances) {
                DataAccessor accessor = new DataAccessor(pair.Key);
                if (filter == null || filter(accessor)) {
                    storage.Add(accessor);
                }
            }

            return storage;
        }

        public Data.IData Current(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return _defaultDataInstances[accessor.Id];
        }

        public Data.Versioned Previous(DataAccessor accessor) {
            if (ContainsData(accessor) == false) {
                throw new NoSuchDataException(this, accessor);
            }

            return (Data.Versioned)_defaultDataInstances[accessor.Id];
        }

        public bool ContainsData(DataAccessor accessor) {
            return _defaultDataInstances.ContainsKey(accessor.Id);
        }

        public bool WasModified(DataAccessor accessor) {
            return false;
        }

        public string PrettyName {
            get;
            set;
        }

        public override string ToString() {
            if (PrettyName.Length > 0) {
                return string.Format("Template [tid={0}, name={1}]", TemplateId, PrettyName);
            }
            else {
                return string.Format("Template [tid={0}]", TemplateId);
            }
        }
    }
}