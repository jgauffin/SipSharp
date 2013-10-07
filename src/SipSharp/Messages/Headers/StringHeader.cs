namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Contains a string header value
    /// </summary>
    public class StringHeader : IHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringHeader"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        public StringHeader(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringHeader"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value</param>
        public StringHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets header value.
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        #region IHeader Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new StringHeader(Name) {Value = Value};
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(IHeader other)
        {
            var header = other as StringHeader;
            if (header != null)
                return string.Compare(header.Value, Value, true) == 0;
            return false;
        }

        #endregion
    }
}