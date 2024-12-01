using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lucky.Kits.Collections
{
    public class DefaultDict<TKey, TVal> : IDictionary<TKey, TVal>
    {
        private Dictionary<TKey, TVal> dic = new();
        private Func<TVal> getter;


        public DefaultDict(Func<TVal> getter)
        {
            if (getter is null)
                throw new ArgumentNullException("DefaultDict needs a getter!!");
            this.getter = getter;
        }

        public IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator()
        {
            return dic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TVal> item)
        {
            dic[item.Key] = item.Value;
        }

        public void Clear()
        {
            dic.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TVal> item)
        {
            return dic.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TVal>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("index is less than zero");
            if (arrayIndex + dic.Count - 1 >= array.Length)
                throw new ArgumentException(
                    "The number of elements in the source Dictionary<TKey,TValue>.ValueCollection is greater than the available space from index to the end of the destination array");
            foreach (var keyValuePair in dic)
            {
                array[arrayIndex++] = keyValuePair;
            }
        }

        public bool Remove(KeyValuePair<TKey, TVal> item)
        {
            if (dic.ContainsKey(item.Key) && dic[item.Key].Equals(item.Value))
            {
                dic.Remove(item.Key);
                return true;
            }

            return false;
        }

        public int Count => dic.Count;
        public bool IsReadOnly { get; }

        public void Add(TKey key, TVal value)
        {
            dic.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return dic.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return dic.Remove(key);
        }

        public bool TryGetValue(TKey key, out TVal value)
        {
            return dic.TryGetValue(key, out value);
        }

        public TVal this[TKey key]
        {
            get
            {
                if (!dic.ContainsKey(key))
                    dic[key] = getter();
                return dic[key];
            }
            set => dic[key] = value;
        }

        public ICollection<TKey> Keys => dic.Keys;
        public ICollection<TVal> Values => dic.Values;


    }
}