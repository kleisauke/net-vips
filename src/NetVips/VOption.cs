namespace NetVips
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// This class wraps a <see cref="Dictionary{TKey,TValue}"/>.
    /// This is used to call functions with optional arguments. See <see cref="Operation.Call(string, VOption, object[])"/>.
    /// </summary>
    public class VOption : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _internalDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="_internalDictionary"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator"/> structure for the <see cref="_internalDictionary"/>.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _internalDictionary.GetEnumerator();

        /// <inheritdoc cref="GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator() => _internalDictionary.GetEnumerator();

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public object this[string key]
        {
            get => _internalDictionary[key];
            set => _internalDictionary[key] = value;
        }

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="_internalDictionary"/>.
        /// </summary>
        public Dictionary<string, object>.KeyCollection Keys => _internalDictionary.Keys;

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="_internalDictionary"/>.
        /// </summary>
        /// <returns>The number of key/value pairs contained in the <see cref="_internalDictionary"/>.</returns>
        public int Count => _internalDictionary.Count;

        /// <summary>
        /// Adds the specified key and value to the <see cref="_internalDictionary"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public void Add(string key, object value) => _internalDictionary.Add(key, value);

        /// <summary>
        /// Adds the specified key and value to the <see cref="_internalDictionary"/>, if value is present.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIfPresent<T>(string key, T? value) where T : struct
        {
            if (value.HasValue)
            {
                _internalDictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Adds the specified key and class to the <see cref="_internalDictionary"/>, if class is present.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="cls">The value of the element to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIfPresent<T>(string key, T cls) where T : class
        {
            if (cls != null)
            {
                _internalDictionary.Add(key, cls);
            }
        }

        /// <summary>
        /// Adds the specified key and array to the <see cref="_internalDictionary"/>, if array is present.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="array">The value of the element to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIfPresent<T>(string key, T[] array) where T : struct
        {
            if (array != null && array.Length > 0)
            {
                _internalDictionary.Add(key, array);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="_internalDictionary"/> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="_internalDictionary"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="_internalDictionary"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(string key) => _internalDictionary.ContainsKey(key);

        /// <summary>
        /// Removes the value with the specified key from the <see cref="_internalDictionary"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(string key) => _internalDictionary.Remove(key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the <see cref="_internalDictionary"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(string key, out object value) => _internalDictionary.TryGetValue(key, out value);
    }
}