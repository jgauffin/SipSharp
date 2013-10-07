using System;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Specifies which headers a parser is for.
    /// </summary>
    /// <remarks>
    /// This attribute is used when the frameworks automatically scans
    /// a namespace in an assembly for header parsers.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ParserForAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserForAttribute"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="compactName">Compact form of header name.</param>
        public ParserForAttribute(string name, char compactName)
        {
            Name = name;
            CompactName = compactName;
        }

        /// <summary>
        /// Gets compact form of header name
        /// </summary>
        public char CompactName { get; private set; }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name { get; private set; }
    }
}