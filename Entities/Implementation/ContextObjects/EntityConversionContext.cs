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
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.ContextObjects {
    internal class EntityConversionContext : IContextObject {
        public SparseArray<IEntity> CreatedEntities = new SparseArray<IEntity>();

        /// <summary>
        /// Returns an entity instance for the given entity UniqueId. If an instance for the given
        /// id already exists, then it is returned. Otherwise, either a RuntimeEntity or
        /// ContentEntity is created.
        /// </summary>
        /// <param name="entityId">The id of the entity to get an instance for.</param>
        /// <param name="context">The GameEngineContext, used to determine if we should create a
        /// ContentTemplate or RuntimeTemplate instance.</param>
        public IEntity GetEntityInstance(int entityId, GameEngineContext context) {
            if (CreatedEntities.Contains(entityId)) {
                return CreatedEntities[entityId];
            }

            IEntity entity;
            if (context.GameEngine.IsEmpty) {
                entity = new ContentEntity();
            }
            else {
                entity = new RuntimeEntity();
            }

            CreatedEntities[entityId] = entity;
            return entity;
        }
    }
}