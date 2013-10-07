using System;
using System.Collections;
using System.Collections.Generic;

namespace SipSharp.Messages.Headers
{
    internal class HeaderKeyValueCollection : IEnumerable<IHeader>, IKeyValueCollection<string, IHeader>
    {
        private readonly Dictionary<string, IHeader> _headers = new Dictionary<string, IHeader>();

        #region IEnumerable<IHeader> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<IHeader> IEnumerable<IHeader>.GetEnumerator()
        {
            return _headers.Values.GetEnumerator();
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
            return _headers.Values.GetEnumerator();
        }

        #endregion

        #region IKeyValueCollection<string,IHeader> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<KeyValuePair<string, IHeader>> IEnumerable<KeyValuePair<string, IHeader>>.GetEnumerator()
        {
            return _headers.GetEnumerator();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        object ICloneable.Clone()
        {
            return null;
        }

        /// <summary>
        /// Gets if the collection is read only.
        /// </summary>
        bool IKeyValueCollection<string, IHeader>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets a value.
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        IHeader IKeyValueCollection<string, IHeader>.this[string key]
        {
            get
            {
                IHeader value;
                return _headers.TryGetValue(key, out value) ? value : null;
            }
            set { _headers[key] = value; }
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="key">Key identifying the value.</param>
        /// <param name="value">value</param>
        void IKeyValueCollection<string, IHeader>.Add(string key, IHeader value)
        {
            _headers.Add(key, value);
        }

        /// <summary>
        /// Create a read only wrapper.
        /// </summary>
        /// <returns></returns>
        IKeyValueCollection<string, IHeader> IKeyValueCollection<string, IHeader>.AsReadOnly()
        {
            return null;
        }

        /// <summary>
        /// Determines if the collection has the specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        bool IKeyValueCollection<string, IHeader>.Contains(string key)
        {
            return _headers.ContainsKey(key);
        }

        /// <summary>
        /// Remove a value using the key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <remarks><c>true</c> if value was found; otherwise <c>false</c>.</remarks>
        bool IKeyValueCollection<string, IHeader>.Remove(string key)
        {
            return _headers.Remove(key);
        }

        /// <summary>
        /// Gets numer of items.
        /// </summary>
        int IKeyValueCollection<string, IHeader>.Count
        {
            get { return _headers.Count; }
        }

        #endregion
    }
}