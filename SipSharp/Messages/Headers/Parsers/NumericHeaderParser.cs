using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parses numeric headers
    /// </summary>
    [ParserFor("Max-Forwards", 'f')]
    [ParserFor("Expires", char.MinValue)]
    [ParserFor("Content-Length", 'l')]
    public class NumericHeaderParser : IHeaderParser
    {
        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets short version of header name (as defined in RFC3261).
        /// </summary>
        public string ShortName
        {
            get { return string.Empty; }
        }

        #region IHeaderParser Members

        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="name">Name of header being parsed.</param>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        public IHeader Parse(string name, ITextReader reader)
        {
            string temp = reader.ReadToEnd();
            int value;
            if (!int.TryParse(temp, out value))
                throw new ParseException("Failed to convert '" + temp + "' to int.");
            return new NumericHeader(name) {Value = value};
        }

        #endregion
    }
}