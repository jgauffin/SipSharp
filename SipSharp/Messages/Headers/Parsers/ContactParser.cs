using System;
using System.Collections.Generic;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// A contact
    /// </summary>
    /// <example>
    /// Contact: "Mr. Watson" <sip:watson@worcester.bell-telephone.com> 
    ///  ;q=0.7; expires=3600, 
    ///  "Mr. Watson" <mailto:watson@bell-telephone.com> 
    ///  ;q=0.1 
    /// </example>
    [ParserFor("Contact", 'm')]
    [ParserFor("To", 't')]
    [ParserFor("From", 'f')]
    public class ContactParser : IHeaderParser
    {
        #region IHeaderParser Members

        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        /// <example>
        /// Contact: "Mr. Watson" <sip:watson@worcester.bell-telephone.com> 
        ///  ;q=0.7; expires=3600, 
        ///  "Mr. Watson" <mailto:watson@bell-telephone.com>;q=0.1 
        /// </example>
        public IHeader Parse(string name, ITextReader reader)
        {
            var contacts = new List<Contact>();
            try
            {
                Contact item = UriParser.ParseContact(reader);
                contacts.Add(item);
                while (reader.Current == ',')
                {
                    reader.ConsumeWhiteSpaces(',');
                    if (!reader.EOF)
                        item = UriParser.ParseContact(reader);
                    contacts.Add(item);
                }
            }
            catch (FormatException err)
            {
                throw new ParseException("Failed to parse header '" + name + "'.", err);
            }

            return new ContactHeader(name) {Contacts = contacts};
        }

        #endregion
    }
}