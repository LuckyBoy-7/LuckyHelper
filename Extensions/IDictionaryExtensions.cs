using System.Collections.Generic;

namespace Lucky.Kits.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void Merge<TKey, TVal>(this IDictionary<TKey, TVal> orig, IDictionary<TKey, TVal> other)
        {
            foreach (var (key, value) in other)
            {
                orig[key] = value;
            }
        }
    }
}