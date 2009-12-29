using System;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Contains an int value.
    /// </summary>
    public class NumericHeader : IHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericHeader"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NumericHeader(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericHeader"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">Numerical value.</param>
        public NumericHeader(string name, int value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public int Value { get; set; }

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
            return new NumericHeader(Name) {Value = Value};
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name { get; private set; }

        #endregion

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
            NumericHeader header = other as NumericHeader;
            if (header != null)
                return header.Value == Value;
            return false;
        }

        public override bool Equals(object obj)
        {
            NumericHeader header = obj as NumericHeader;
            if (header != null)
                return header.Value == Value;
            return false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}