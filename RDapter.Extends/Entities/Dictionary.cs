using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RDapter.Extends.Entities
{
    internal sealed class Dictionary<TKey, TValue1, TValue2> : IDictionary<TKey, ValueTuple<TValue1, TValue2>>
    {
        private Dictionary<TKey, ValueTuple<TValue1, TValue2>> data = new Dictionary<TKey, ValueTuple<TValue1, TValue2>>();
        public (TValue1, TValue2) this[TKey key]
        {
            get
            {
                if (data.TryGetValue(key, out var tp)) return (tp.Item1, tp.Item2);
                return (default, default);
            }
            set
            {
                data[key] = value;
            }
        }

        public ICollection<TKey> Keys => data.Keys;

        public ICollection<(TValue1, TValue2)> Values => data.Values;

        public int Count => data.Count;

        public bool IsReadOnly => true;

        public void Add(TKey key, (TValue1, TValue2) value)
        {
            data.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, (TValue1, TValue2)> item)
        {
            data.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(KeyValuePair<TKey, (TValue1, TValue2)> item)
        {
            return data.ContainsKey(item.Key);
        }

        public bool ContainsKey(TKey key)
        {
            return data.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, (TValue1, TValue2)>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, (TValue1, TValue2)>> GetEnumerator()
        {
            foreach (var d in data)
            {
                yield return d;
            }
        }

        public bool Remove(TKey key)
        {
            return data.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, (TValue1, TValue2)> item)
        {
            return data.Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out (TValue1, TValue2) value)
        {
            return data.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
