using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parser for <see cref="ContentType"/>.
    /// </summary>
    [ParserFor(ContentType.NAME, 'c')]
    public class ContentTypeParser : IHeaderParser
    {
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
            string type = reader.ReadUntil("/");
            reader.Consume('/');
            string subType = reader.ReadToEnd(';');
            reader.Consume(';', ' ', '\t');

            var contentType = new ContentType {SubType = subType, Type = type};
            UriParser.ParseParameters(contentType.Parameters, reader);
            return contentType;
        }

        #endregion
    }
}