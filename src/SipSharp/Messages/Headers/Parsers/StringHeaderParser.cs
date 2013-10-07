using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Used on all headers that are not identified.
    /// </summary>
    [ParserFor("Call-Id", 'i')]
    public class StringHeaderParser : IHeaderParser
    {
        #region IHeaderParser Members

        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        public IHeader Parse(string name, ITextReader reader)
        {
            return new StringHeader(name) {Value = reader.ReadToEnd()};
        }

        #endregion
    }
}