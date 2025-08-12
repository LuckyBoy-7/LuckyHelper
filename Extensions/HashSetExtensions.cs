namespace LuckyHelper.Extensions;

// https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/c3595e5af47bde0bca28e4693c80c180434c218c/StudioCommunication/Util/CommonExtensions.cs#L313
public static class HashSetExtensions {
    /// Adds a range of elements to the set
    public static void AddRange<T>(this HashSet<T> set, params IEnumerable<T> items) {
        switch (items) {
            case IList<T> list: {
#if NET7_0_OR_GREATER
                set.EnsureCapacity(set.Count + list.Count);
#endif
                for (int i = 0; i < list.Count; i++) {
                    set.Add(list[i]);
                }
                break;
            }
            case ICollection<T> collection: {
#if NET7_0_OR_GREATER
                set.EnsureCapacity(set.Count + collection.Count);
#endif
                foreach (var item in collection) {
                    set.Add(item);
                }
                break;
            }
            default: {
                foreach (var item in items) {
                    set.Add(item);
                }
                break;
            }
        }
    }
}