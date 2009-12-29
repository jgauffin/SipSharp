using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parses CSeq headers.
    /// </summary>
    [ParserFor("CSeq", char.MinValue)]
    public class CSeqParser : IHeaderParser
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
            reader.ConsumeWhiteSpaces();
            string number = reader.ReadWord();
            reader.ConsumeWhiteSpaces();
            string method = reader.ReadWord();

            int sequence;
            if (!int.TryParse(number, out sequence))
                throw new ParseException("Sequence number in CSeq must only contain digits, invalid value: " + number);

            return new CSeq(sequence, method);
        }

        #endregion
    }
}