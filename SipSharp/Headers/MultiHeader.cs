using System;
using System.Collections;
using System.Collections.Generic;
using SipSharp.Tools;

namespace SipSharp.Headers
{
    /// <summary>
    /// A header containing multiple values.
    /// </summary>
    /// <typeparam name="T">Type of header item</typeparam>
    public abstract class MultiHeader<T> : IHeader, IEnumerable<T>, ICloneable where T : class, IHeader, ICloneable
    {
        private readonly List<T> _items = new List<T>();
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHeader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        protected MultiHeader(string name)
        {
            _name = name;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiHeader&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="header">Header to deep copy information from.</param>
		protected MultiHeader(MultiHeader<T> header)
		{
			_name = header._name;
			foreach (T item in header._items)
				_items.Add((T)item.Clone());
		}

        /// <summary>
        /// All entries.
        /// </summary>
        public List<T> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Create a new header value instance.
        /// </summary>
        /// <remarks>
        /// Activator.CreateInstance is quite slow. Either replace
        /// this method with a IL generated one or override it to
        /// specialized versions.
        /// </remarks>
        /// <returns>A newly created header.</returns>
        protected virtual IHeader CreateHeader()
        {
            return (T) Activator.CreateInstance(typeof (T));
        }

        /// <summary>
        /// Gets first header entry
        /// </summary>
        public T First
        {
            get
            {
                return Items.Count > 0 ? Items[0] : null;
            }
        }


        #region IHeader Members

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets header value(s).
        /// </summary>
        /// <remarks>
        /// Values will be combined (separated by comma) if more than
        /// one value exists.
        /// </remarks>
        string IHeader.Value
        {
            get
            {
                string temp = string.Empty;
                foreach (T item in Items)
                    temp += item + ",";
                return temp.Remove(temp.Length - 1);
            }
        }

        /// <summary>
        /// Gets all header values.
        /// </summary>
        string[] IHeader.Values
        {
            get
            {
                var values = new string[Items.Count];
                for (int i = 0; i < _items.Count; ++i)
                    values[i] = _items[i].ToString();
                return values;
            }
        }

        /// <summary>
        /// Parse a value.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times.
        /// </remarks>
        /// <param name="value">string containing one or more values.</param>
        /// <exception cref="FormatException">If value format is incorrect.</exception>
        void IHeader.Parse(string value)
        {
            var reader = new StringReader(value);
            while (!reader.IsEOF)
            {
                reader.SkipWhiteSpaces();
                string headerValue = reader.ReadToEnd(',', true);
                var entry = (T) CreateHeader();
                entry.Parse(headerValue);
                reader.MoveNext();
            }
        }

        #endregion

        /// <summary>
        ///                     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///                     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        ///                     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    	/// <summary>
    	///                     Creates a new object that is a copy of the current instance.
    	/// </summary>
    	/// <returns>
    	///                     A new object that is a copy of this instance.
    	/// </returns>
    	/// <filterpriority>2</filterpriority>
    	public abstract object Clone();
    }
}