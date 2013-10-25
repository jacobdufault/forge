using Neon.Utilities;
using System.Collections.Generic;

public static class ListExtensions {
    /// <summary>
    /// Shuffle the specified list.
    /// </summary>
    /// <param name="list">The list to shuffle.</param>
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = GameRandom.Range(0, n);

            // swap list[n] and list[k]
            T tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    /// <summary>
    /// Checks if the given list is empty.
    /// </summary>
    public static bool IsEmpty<T>(this IList<T> list) {
        return list.Count == 0;
    }
}