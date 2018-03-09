using System.Collections;
using System.Collections.Generic;

namespace NetVips
{
    /// <summary>
    /// This class wraps a <see cref="Dictionary{String, Object}" />.
    /// This is used to call functions with optional arguments. See <see cref="Operation.Call(string, VOption, object[])" />.
    /// </summary>
    public class VOption : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> internalDictionary = new Dictionary<string, object>();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => internalDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => internalDictionary.GetEnumerator();

        public object this[string key]
        {
            get => internalDictionary[key];
            set => Add(key, value);
        }

        public int Count => internalDictionary.Count;

        public void Add(string key, object value) => internalDictionary.Add(key, value);

        public bool Remove(string key) => internalDictionary.Remove(key);

        public bool TryGetValue(string key, out object value) => internalDictionary.TryGetValue(key, out value);
    }
}