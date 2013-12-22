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
    internal class TemplateConversionContext : IContextObject {
        public SparseArray<ITemplate> CreatedTemplates = new SparseArray<ITemplate>();

        /// <summary>
        /// Returns a template instance for the given TemplateId. If an instance for the given id
        /// already exists, then it is returned. Otherwise, either a RuntimeTemplate or
        /// ContentTemplate is created with an associated id based on the GameEngineContext.
        /// </summary>
        /// <param name="templateId">The id of the template to get an instance for.</param>
        /// <param name="context">The GameEngineContext, used to determine if we should create a
        /// ContentTemplate or RuntimeTemplate instance.</param>
        public ITemplate GetTemplateInstance(int templateId, GameEngineContext context) {
            if (CreatedTemplates.Contains(templateId)) {
                return CreatedTemplates[templateId];
            }

            ITemplate template;
            if (context.GameEngine.IsEmpty) {
                template = new ContentTemplate(templateId);
            }
            else {
                template = new RuntimeTemplate(templateId, context.GameEngine.Value);
            }

            CreatedTemplates[templateId] = template;
            return template;
        }
    }
}