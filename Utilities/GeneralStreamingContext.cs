using System;
using System.Collections.Generic;

namespace Neon.Utilities {
    /// <summary>
    /// An object that can be used as a value in a GeneralStreamingContext instance.
    /// </summary>
    public interface IContextObject { }

    /// <summary>
    /// Object that implements the streaming context that all converters which expect a streaming
    /// context expect the streaming context to be a type of.
    /// </summary>
    public class GeneralStreamingContext {
        /// <summary>
        /// The context objects
        /// </summary>
        private Dictionary<Type, IContextObject> _contextObjects =
            new Dictionary<Type, IContextObject>();

        /// <summary>
        /// Creates a new GeneralStreamingContext with the given initial objects.
        /// </summary>
        public GeneralStreamingContext(params IContextObject[] initialObjects) {
            foreach (IContextObject obj in initialObjects) {
                _contextObjects[obj.GetType()] = obj;
            }
        }

        /// <summary>
        /// Returns the context object associated with the type T.
        /// </summary>
        public T Get<T>() where T : IContextObject {
            IContextObject value;
            if (_contextObjects.TryGetValue(typeof(T), out value) == false) {
                throw new InvalidOperationException("There is no context object of type " + typeof(T));
            }
            return (T)value;
        }

        /// <summary>
        /// Sets the context object of type T to an instance of new T().
        /// </summary>
        public void Create<T>() where T : IContextObject, new() {
            Set(new T());
        }

        /// <summary>
        /// Sets the context object of type T with the given value.
        /// </summary>
        public void Set<T>(T instance) where T : IContextObject {
            if (_contextObjects.ContainsKey(typeof(T))) {
                throw new InvalidOperationException("There is already a context object of type " + typeof(T));
            }

            _contextObjects[typeof(T)] = instance;
        }

        /// <summary>
        /// Removes the context object associated with type T.
        /// </summary>
        public void Remove<T>() where T : IContextObject {
            if (_contextObjects.Remove(typeof(T)) == false) {
                throw new InvalidOperationException("There was not a context object of type " + typeof(T) + " to remove");
            }
        }
    }
}