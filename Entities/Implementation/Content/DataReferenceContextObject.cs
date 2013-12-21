using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neon.Entities.Implementation.Shared {
    /// <summary>
    /// Object that is used by data references when they are deserializing to list all references of
    /// DataReferences that are created so that they can be restored later on. Object that
    /// implements the streaming context that all converters which expect a streaming context expect
    /// the streaming context to be a type of.
    /// </summary>
    internal class DataReferenceContextObject : IContextObject {
        /// <summary>
        /// The data references that have been created thus far.
        /// </summary>
        public List<BaseDataReferenceType> DataReferences = new List<BaseDataReferenceType>();
    }
}