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
                template = new ContentTemplate();
            }
            else {
                template = new RuntimeTemplate(context.GameEngine.Value);
            }

            CreatedTemplates[templateId] = template;
            return template;
        }
    }
}