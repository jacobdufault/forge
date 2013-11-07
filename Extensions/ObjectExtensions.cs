/// <summary>
/// Extensions for types which are objects.
/// </summary>
public static class ObjectExtensions {
    /// <summary>
    /// Helpful function for calculating the hash code for a set of heterogeneous types.
    /// </summary>
    // TODO: this can be optimized (also use generics to avoid boxing)
    public static int CalculateHashCode(this object obj, params object[] members) {
        // Overflow is okay; just wrap around
        unchecked {
            int hash = 5;
            for (int i = 0; i < members.Length; ++i) {
                hash = hash * 29 + members[i].GetHashCode();
            }
            return hash;
        }
    }
}
