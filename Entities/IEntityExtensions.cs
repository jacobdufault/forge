namespace Neon.Entities {
    /// <summary>
    /// Helper methods built on top of the core IEntity API.
    /// </summary>
    public static class IEntityExtensions {
        public static T AddOrModify<T>(this IEntity entity) where T : Data {
            return (T)entity.AddOrModify(DataMap<T>.Accessor);
        }

        public static T AddData<T>(this IEntity entity) where T : Data {
            return (T)entity.AddData(DataMap<T>.Accessor);
        }

        public static void RemoveData<T>(this IEntity entity) where T : Data {
            entity.RemoveData(DataMap<T>.Accessor);
        }

        public static T Modify<T>(this IEntity entity, bool force = false) where T : Data {
            return (T)entity.Modify(DataMap<T>.Accessor, force);
        }

        public static T Current<T>(this IQueryableEntity entity) where T : Data {
            return (T)entity.Current(DataMap<T>.Accessor);
        }

        public static T Previous<T>(this IQueryableEntity entity) where T : Data {
            return (T)entity.Previous(DataMap<T>.Accessor);
        }

        public static bool ContainsData<T>(this IQueryableEntity entity) where T : Data {
            return entity.ContainsData(DataMap<T>.Accessor);
        }

        public static bool WasModified<T>(this IQueryableEntity entity) where T : Data {
            return entity.WasModified(DataMap<T>.Accessor);
        }

    }
}