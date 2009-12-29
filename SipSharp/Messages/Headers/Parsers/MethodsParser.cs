using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parses <see cref="MethodsHeader"/>.
    /// </summary>
    [ParserFor("Proxy-Require", char.MinValue)]
    [ParserFor("Allow", char.MinValue)]
    [ParserFor("Supported", char.MinValue)]
    public class MethodsParser : IHeaderParser
    {
        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="name">Name of header being parsed.</param>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        public IHeader Parse(string name, ITextReader reader)
        {
            MethodsHeader header = new MethodsHeader(name);

            string method = reader.ReadToEnd(',');
            do
            {
                header.Methods.Add(method);
                method = reader.ReadToEnd(',');
            } while (reader.Current == ',');

            header.Methods.Add(method);
            return header;
        }
    }
}
