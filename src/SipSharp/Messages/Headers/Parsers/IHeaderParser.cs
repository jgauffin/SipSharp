using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Interface for header parsers.
    /// </summary>
    public interface IHeaderParser
    {
        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="name">Name of header being parsed.</param>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        IHeader Parse(string name, ITextReader reader);
    }
}