namespace Neon.Entities {
    /// <summary>
    /// Helper methods built on top of the core IEntity API.
    /// </summary>
    public static class IEntityExtensions {
        /// <summary>
        /// Adds the given data type, or modifies an instance of it.
        /// </summary>
        /// <remarks>
        /// This is a helper method that captures a common pattern.
        /// </remarks>
        /// <typeparam name="T">The type of data modified</typeparam>
        /// <returns>A modifiable instance of data of type T</returns>
        public static T AddOrModify<T>(this IEntity entity) where T : Data {
            DataAccessor accessor = DataMap<T>.Accessor;

            if (entity.ContainsData(accessor) == false) {
                return (T)entity.AddData(accessor);
            }
            return (T)entity.Modify(accessor);
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

        public static T Current<T>(this IEntity entity) where T : Data {
            return (T)entity.Current(DataMap<T>.Accessor);
        }

        public static T Previous<T>(this IEntity entity) where T : Data {
            return (T)entity.Previous(DataMap<T>.Accessor);
        }

        public static bool ContainsData<T>(this IEntity entity) where T : Data {
            return entity.ContainsData(DataMap<T>.Accessor);
        }

        public static bool WasModified<T>(this IEntity entity) where T : Data {
            return entity.WasModified(DataMap<T>.Accessor);
        }

    }
}