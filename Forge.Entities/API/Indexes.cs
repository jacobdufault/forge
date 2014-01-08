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
using System;
using System.Collections.Generic;

namespace Forge.Entities {
    /// <summary>
    /// Provides access to all entities in a game engine via the entity's UniqueId.
    /// </summary>
    public sealed class EntityIndex {
        private SparseArray<IEntity> _entities;

        internal EntityIndex() {
            _entities = new SparseArray<IEntity>();
        }

        /// <summary>
        /// Adds the given entity to the index.
        /// </summary>
        internal void AddEntity(IEntity entity) {
            _entities[entity.UniqueId] = entity;
        }

        /// <summary>
        /// Removes the given entity from the index.
        /// </summary>
        internal void RemoveEntity(IEntity entity) {
            bool success = _entities.Remove(entity.UniqueId);
            if (success == false) {
                throw new InvalidOperationException("Failed to remove entity " + entity +
                    " from the entity index");
            }
        }

        /// <summary>
        /// Returns the entity with the given id. If no entity exists with the given id, then an
        /// exception is thrown.
        /// </summary>
        public IEntity this[int uniqueId] {
            get {
                IEntity entity;
                if (_entities.TryGetValue(uniqueId, out entity)) {
                    return entity;
                }

                throw new InvalidOperationException("No entity with UniqueId=" + uniqueId);
            }
        }
    }

    /// <summary>
    /// Provides access to all templates in a game engine via the template's TemplateId.
    /// </summary>
    public sealed class TemplateIndex {
        private Dictionary<int, ITemplate> _templates;

        /// <summary>
        /// Creates a template index with the given templates.
        /// </summary>
        internal TemplateIndex(IEnumerable<ITemplate> templates) {
            _templates = new Dictionary<int, ITemplate>();
            foreach (ITemplate template in templates) {
                _templates[template.TemplateId] = template;
            }
        }

        /// <summary>
        /// Returns the template with the given TemplateId. If no template with the given TemplateId
        /// exists, then an exception is thrown.
        /// </summary>
        public ITemplate this[int templateId] {
            get {
                ITemplate template;
                if (_templates.TryGetValue(templateId, out template)) {
                    return template;
                }

                throw new InvalidOperationException("No template with TemplateId=" + templateId);
            }
        }
    }

}