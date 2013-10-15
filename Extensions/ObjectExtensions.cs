public static class ObjectExtensions {
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
