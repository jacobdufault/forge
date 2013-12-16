using Neon.Collections;
using Neon.Entities.Implementation.Content;
using Neon.Utilities;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities {
    /// <summary>
    /// An object that can be used to reference an ITemplate instance.
    /// </summary>
    [ProtoContract]
    public sealed class TemplateReference {
        /// <summary>
        /// The template resolver that we use to resolve our ITemplate instance at runtime. This is
        /// a shared instance among all TemplateReferences inside of the "island" of data that is a
        /// loaded level.
        /// </summary>
        [ProtoMember(1)]
        private TemplateResolver _resolver;

        /// <summary>
        /// The template id that we are referencing.
        /// </summary>
        [ProtoMember(2)]
        private int _templateId;

        /// <summary>
        /// Constructor used to create the template reference by the resolver.
        /// </summary>
        internal TemplateReference(int templateId, TemplateResolver resolver) {
            _templateId = templateId;
            _resolver = resolver;
        }

        /// <summary>
        /// Resolves this template reference to the correct ITemplate instance.
        /// </summary>
        /// <returns>An ITemplate instance that can either be modified or used to instantiate
        /// entities.</returns>
        public ITemplate Resolve() {
            return _resolver.Resolve(_templateId);
        }
    }

    /// <summary>
    /// Resolves TemplateReferences to their appropriate ITemplate instance.
    /// </summary>
    // We want to serialize as a reference default so that all TemplateReferences all share the same resolver
    // instances, even after they have been restored.
    [ProtoContract(AsReferenceDefault = true)]
    internal class TemplateResolver {
        /// <summary>
        /// The templates that will be resolved. This is not serialized, as its content is
        /// dynamically populated depending on what mode Neon.Entities is currently operating in.
        /// </summary>
        private SparseArray<ITemplate> _templates;

        /// <summary>
        /// Id generator used to generate new template ids.
        /// </summary>
        private UniqueIntGenerator _idGenerator;

        public void SetTemplates(IEnumerable<ITemplate> templates) {
            _templates = new SparseArray<ITemplate>();
            _idGenerator = new UniqueIntGenerator();

            foreach (ITemplate template in templates) {
                _templates[template.TemplateId] = template;
                _idGenerator.Consume(template.TemplateId);
            }
        }

        /// <summary>
        /// Resolve a template id to its ITemplate instance. The ITemplate instance may be either a
        /// ContentTemplate or a RuntimeTemplate.
        /// </summary>
        /// <param name="id">The id of the template that we are referencing.</param>
        public ITemplate Resolve(int id) {
            if (_templates == null) {
                throw new InvalidOperationException("Failed to restore the TemplateResolver");
            }

            return _templates[id];
        }

        /// <summary>
        /// Creates a new template.
        /// </summary>
        public TemplateReference CreateTemplate() {
            if (_templates == null) {
                throw new InvalidOperationException("Failed to restore the TemplateResolver");
            }

            int id = _idGenerator.Next();
            _templates[id] = new ContentTemplate(id);
            return new TemplateReference(id, this);
        }
    }
}