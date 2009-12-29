using System;
using System.Collections.Generic;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Contains a list of SIP methods.
    /// </summary>
    /// <remarks>
    /// Used for ProxyRequire, Allow, Supported headers etc.
    /// </remarks>
    public class MethodsHeader : IHeader
    {
        public MethodsHeader(string name)
        {
            Name = name;
            Methods = new List<string>();
        }

        /// <summary>
        /// Gets or sets methods not found in <see cref="SipMethod"/> enumeration.
        /// </summary>
        /// <remarks>All methods are in upper case.</remarks>
        public ICollection<string> Methods { get; private set; }

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
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name { get; private set; }

        #endregion
    }
}