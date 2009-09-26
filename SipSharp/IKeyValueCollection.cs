using System;
using System.Collections;
using System.Collections.Generic;

namespace SipSharp
{
    /// <summary>
    /// Collection of key value.
    /// </summary>
    /// <remarks>
    /// I've read somewhere that NameValueCollection is not very effecient.
    /// And Dictionary is not case insensitive. Therefore we'll be working 
    /// with this interface and therefore be able to change implementation
    /// without any hassle.
    /// </remarks>
    public interface IKeyValueCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, ICloneable
    {
        /// <summary>
        /// Gets if the collection is read only.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets or sets a value.
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        TValue this[TKey key] { get; set; }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <param name="value">value</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Create a read only wrapper.
        /// </summary>
        /// <returns></returns>
        IKeyValueCollection<TKey, TValue> AsReadOnly();

        /// <summary>
        /// Determines if the collection has the specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        bool Contains(TKey key);

        /// <summary>
        /// Remove a value using the key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks><c>true</c> if value was found; otherwise <c>false</c>.</remarks>
        bool Remove(TKey key);

        /// <summary>
        /// Gets numer of items.
        /// </summary>
        int Count { get; }

    }

    /// <summary>
    /// String version of the collection.
    /// </summary>
    public interface IKeyValueCollection : IKeyValueCollection<string, string>
    {
    }

    public class KeyValueCollection : IKeyValueCollection
    {
        public IDictionary<string, string> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueCollection"/> class.
        /// </summary>
        public KeyValueCollection()
        {
            _items = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        #region IKeyValueCollection Members

        /// <summary>
        /// Gets or sets a value.
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        public string this[string key]
        {
            get
            {
                string value;
                return _items.TryGetValue(key, out value) ? value : null;
            }
            set { _items[key] = value; }
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <param name="value">value</param>
        public void Add(string key, string value)
        {
            _items.Add(key, value);
        }

        /// <summary>
        /// Remove a value using the key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks><c>true</c> if value was found; otherwise <c>false</c>.</remarks>
        public bool Remove(string key)
        {
            _items.Remove(key);
            return true;
        }

        /// <summary>
        /// Gets numer of items.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Determines if the collection has the specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool Contains(string key)
        {
            return _items.ContainsKey(key);
        }

        /// <summary>
        /// Gets if the collection is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            var collection = new KeyValueCollection();
            foreach (var item in _items)
                collection.Add(item.Key, item.Value);
            return collection;
        }

        public IKeyValueCollection<string, string> AsReadOnly()
        {
            return new ReadOnlyKeyCollection<string, string>(this);
        }

        #endregion

        public void Clear()
        {
            _items.Clear();
        }
    }

    public class KeyValueCollection<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private IDictionary<TKey, TValue> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueCollection"/> class.
        /// </summary>
        public KeyValueCollection()
        {
            _items = new Dictionary<TKey, TValue>();
        }

        #region IKeyValueCollection Members

        /// <summary>
        /// Gets or sets a value.
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        public TValue this[TKey key]
        {
            get { return _items[key]; }
            set { _items[key] = value; }
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <param name="value">value</param>
        public void Add(TKey key, TValue value)
        {
            _items.Add(key, value);
        }

        /// <summary>
        /// Remove a value using the key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks><c>true</c> if value was found; otherwise <c>false</c>.</remarks>
        public bool Remove(TKey key)
        {
            _items.Remove(key);
            return true;
        }

        /// <summary>
        /// Gets numer of items.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Determines if the collection has the specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool Contains(TKey key)
        {
            return _items.ContainsKey(key);
        }

        /// <summary>
        /// Gets if the collection is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            var collection = new KeyValueCollection<TKey, TValue>();
            foreach (var item in _items)
                collection.Add(item.Key, item.Value);
            return collection;
        }

        public IKeyValueCollection<TKey, TValue> AsReadOnly()
        {
            return new ReadOnlyKeyCollection<TKey, TValue>(this);
        }

        #endregion
    }

    public class ReadOnlyKeyCollection<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private readonly IKeyValueCollection<TKey, TValue> _items;

        public ReadOnlyKeyCollection(IKeyValueCollection<TKey, TValue> items)
        {
            _items = items;
        }

        #region IKeyValueCollection<TKey,TValue> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets or sets a value.
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        public TValue this[TKey key]
        {
            get { return _items[key]; }
            set { throw new InvalidOperationException("Collection is read only."); }
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <param name="value">value</param>
        public void Add(TKey key, TValue value)
        {
            throw new InvalidOperationException("Collection is read only.");
        }

        /// <summary>
        /// Remove a value using the key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks><c>true</c> if value was found; otherwise <c>false</c>.</remarks>
        public bool Remove(TKey key)
        {
            throw new InvalidOperationException("Collection is read only.");
        }

        /// <summary>
        /// Gets numer of items.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Determines if the collection has the specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool Contains(TKey key)
        {
            return _items.Contains(key);
        }

        /// <summary>
        /// Gets if the collection is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Create a read only wrapper.
        /// </summary>
        /// <returns></returns>
        public IKeyValueCollection<TKey, TValue> AsReadOnly()
        {
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return _items.Clone();
        }

        #endregion
    }
}