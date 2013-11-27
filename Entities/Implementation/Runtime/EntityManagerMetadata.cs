using Neon.Collections;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Runtime {
    /// <summary>
    /// Metadata that the EntityManager requires.
    /// </summary>
    internal class EntityManagerMetadata {
        private static UniqueIntGenerator _unorderedListKeys = new UniqueIntGenerator();
        public static int GetUnorderedListMetadataIndex() {
            return _unorderedListKeys.Next();
        }

        public SparseArray<UnorderedListMetadata> UnorderedListMetadata = new SparseArray<UnorderedListMetadata>();
    }
}