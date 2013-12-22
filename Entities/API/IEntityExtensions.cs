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

namespace Neon.Entities {
    /// <summary>
    /// Helper methods built on top of the core IEntity API.
    /// </summary>
    public static class IEntityExtensions {
        public static T Current<T>(this IQueryableEntity entity) where T : IData {
            return (T)entity.Current(DataMap<T>.Accessor);
        }

        public static T Previous<T>(this IQueryableEntity entity) where T : IData {
            return (T)entity.Previous(DataMap<T>.Accessor);
        }

        public static bool ContainsData<T>(this IQueryableEntity entity) where T : IData {
            return entity.ContainsData(DataMap<T>.Accessor);
        }

        public static T AddOrModify<T>(this IEntity entity) where T : IData {
            return (T)entity.AddOrModify(DataMap<T>.Accessor);
        }

        public static T AddData<T>(this IEntity entity) where T : IData {
            return (T)entity.AddData(DataMap<T>.Accessor);
        }

        public static void RemoveData<T>(this IEntity entity) where T : IData {
            entity.RemoveData(DataMap<T>.Accessor);
        }

        public static T Modify<T>(this IEntity entity) where T : IData {
            return (T)entity.Modify(DataMap<T>.Accessor);
        }

        public static bool WasModified<T>(this IEntity entity) where T : IData {
            return entity.WasModified(DataMap<T>.Accessor);
        }

        public static bool WasAdded<T>(this IEntity entity) where T : IData {
            return entity.WasAdded(DataMap<T>.Accessor);
        }

        public static bool WasRemoved<T>(this IEntity entity) where T : IData {
            return entity.WasRemoved(DataMap<T>.Accessor);
        }
    }
}