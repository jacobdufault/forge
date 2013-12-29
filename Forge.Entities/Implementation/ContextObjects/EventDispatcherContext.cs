using Forge.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.Entities.Implementation.ContextObjects {
    internal class EventDispatcherContext : IContextObject {
        public IEventDispatcher Dispatcher {
            get;
            private set;
        }

        public EventDispatcherContext(IEventDispatcher dispatcher) {
            Dispatcher = dispatcher;
        }
    }
}