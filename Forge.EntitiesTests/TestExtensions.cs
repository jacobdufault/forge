using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Forge.Entities.Tests {
    public static class TestExtensions {
        public static Task Update(this IGameEngine engine) {
            return engine.Update(new List<IGameInput>());
        }

        public static TSystem GetSystem<TSystem>(this IGameEngine engine) where TSystem : BaseSystem {
            foreach (BaseSystem system in engine.TakeSnapshot().Systems) {
                if (system is TSystem) {
                    return (TSystem)system;
                }
            }

            throw new InvalidOperationException("No system of type " + typeof(TSystem) +
                " is in the engine");
        }

        public static void Add<T>(this IList<T> list, params T[] elements) {
            foreach (var element in elements) {
                list.Add(element);
            }
        }
    }
}