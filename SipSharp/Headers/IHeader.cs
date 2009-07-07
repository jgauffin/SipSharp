using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Headers
{
    /// <summary>
    /// Sip header
    /// </summary>
    public interface IHeader
    {
        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets header value(s).
        /// </summary>
        /// <remarks>
        /// Values will be combined (separated by comma) if more than
        /// one value exists.
        /// </remarks>
        string Value { get; }

        /// <summary>
        /// Gets all header values.
        /// </summary>
        string[] Values { get; }

        /// <summary>
        /// Parse a value.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times.
        /// </remarks>
        /// <param name="value">string containing one or more values.</param>
        /// <exception cref="FormatException">If value format is incorrect.</exception>
        void Parse(string value);
    }
}
