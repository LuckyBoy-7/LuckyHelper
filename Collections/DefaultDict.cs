using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lucky.Kits.Collections
{
    public class DefaultDict<TKey, TVal> : IDictionary<TKey, TVal>
    {
        private readonly Dictionary<TKey, TVal> _dic = new();
        private readonly Func<TVal> _getter;


        public DefaultDict(Func<TVal> getter)
        {
            // ReSharper disable once NotResolvedInText
            this._getter = getter ?? throw new ArgumentNullException("DefaultDict needs a getter!");
        }

        public IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TVal> item)
        {
            _dic[item.Key] = item.Value;
        }

        public void Clear()
        {
            _dic.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TVal> item)
        {
            return _dic.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TVal>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("index is less than zero");
            if (arrayIndex + _dic.Count - 1 >= array.Length)
                throw new ArgumentException(
                    "The number of elements in the source Dictionary<TKey,TValue>.ValueCollection is greater than the available space from index to the end of the destination array");
            foreach (var keyValuePair in _dic)
            {
                array[arrayIndex++] = keyValuePair;
            }
        }

        public bool Remove(KeyValuePair<TKey, TVal> item)
        {
            if (_dic.ContainsKey(item.Key) && _dic[item.Key].Equals(item.Value))
            {
                _dic.Remove(item.Key);
                return true;
            }

            return false;
        }

        public int Count => _dic.Count;
        public bool IsReadOnly { get; }

        public void Add(TKey key, TVal value)
        {
            _dic.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dic.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return _dic.Remove(key);
        }

        public bool TryGetValue(TKey key, out TVal value)
        {
            return _dic.TryGetValue(key, out value);
        }

        public TVal this[TKey key]
        {
            get
            {
                if (!_dic.ContainsKey(key))
                    _dic[key] = _getter();
                return _dic[key];
            }
            set => _dic[key] = value;
        }

        public ICollection<TKey> Keys => _dic.Keys;
        public ICollection<TVal> Values => _dic.Values;
    }
}