using System;

namespace SipSharp.Messages.Headers
{
    internal class MaxForwards : IHeader
    {
        /// <summary>
        /// Gets header name.
        /// </summary>
        public const string NAME = "Max-Forwards";

        /// <summary>
        /// Gets or sets forward count.
        /// </summary>
        public int Count { get; set; }

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
            return new MaxForwards {Count = Count};
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
        {
            get { return NAME; }
        }

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
                return Count == header.Value;

            MaxForwards forwards = other as MaxForwards;
            if (forwards != null)
                return forwards.Count == Count;

            return false;
        }
    }
}