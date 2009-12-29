using System;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Sip header
    /// </summary>
    public interface IHeader : ICloneable, IEquatable<IHeader>
    {
        /// <summary>
        /// Gets header name
        /// </summary>
        string Name { get; }
    }
    /*
    /// <summary>
    /// Header class is used for multiple headers.
    /// </summary>
    public interface IGenericHeader : IHeader
    {
        /// <summary>
        /// Assign header name.
        /// </summary>
        /// <param name="value">Name of header</param>
        /// <exception cref="InvalidOperationException">Name have already been assigned.</exception>
        /// <remarks>
        /// Generic header classes doesn't have a standard name since they are used
        /// for multiple headers.
        /// </remarks>
        void SetHeaderName(string value);
    }
    */
}