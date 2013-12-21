using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Neon.Entities.Implementation.Shared;
using Neon.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Neon.Entities.Tests {
    [TestClass]
    public class TemplateSerializationTests {
        private class buff : TraceListener {
            public List<string> messages = new List<string>();

            public override void Write(string message) {
                messages.Add(message);
            }

            public override void WriteLine(string message) {
                messages.Add(message);
            }
        }

        [TestMethod]
        public void SerializeTemplateReference() {
            ITemplate template = new ContentTemplate();

            buff buff = new buff();
            Trace.Listeners.Add(buff);

            string json = SerializationHelpers.Serialize(template,
                RequiredConverters.GetConverters(),
                RequiredConverters.GetContexts(Maybe<GameEngine>.Empty));

            Console.WriteLine();
            // Summary:
            // Represents a trace writer that writes to the application's
            // System.Diagnostics.TraceListener instances.

        }
    }
}