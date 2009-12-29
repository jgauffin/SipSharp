using System;
using SipSharp.Headers;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parses Via header.
    /// </summary>
    [ParserFor("Via", 'v')]
    public class ViaParser : IHeaderParser
    {
        public static ViaEntry ParseEntry(ITextReader reader)
        {
            //SIP/2.0/UDP erlang.bell-telephone.com:5060;branch=z9hG4bK87asdks7
            var entry = new ViaEntry();

            // read SIP/
            reader.ConsumeWhiteSpaces();
            entry.Protocol = reader.ReadUntil("/ \t");
            if (entry.Protocol == null)
                throw new FormatException("Expected Via header to start with 'SIP' or 'SIPS'.");
            reader.ConsumeWhiteSpaces('/');

            // read 2.0/
            entry.SipVersion = reader.ReadUntil("/ \t");
            if (entry.SipVersion == null)
                throw new FormatException("Expected to find sip version in Via header.");
            reader.ConsumeWhiteSpaces('/');

            // read UDP or TCP
            entry.Transport = reader.ReadWord();
            if (entry.Transport == null)
                throw new FormatException("Expected to find transport protocol after sip version in Via header.");
            reader.ConsumeWhiteSpaces();

            entry.Domain = reader.ReadUntil(";: \t");
            if (entry.Domain == null)
                throw new FormatException("Failed to find domain in via header.");
            reader.ConsumeWhiteSpaces();

            if (reader.Current == ':')
            {
                reader.Read();
                reader.ConsumeWhiteSpaces();
                string temp = reader.ReadToEnd("; \t");
                reader.ConsumeWhiteSpaces();
                int port;
                if (!int.TryParse(temp, out port))
                    throw new FormatException("Invalid port specified.");
                entry.Port = port;
            }

            UriParser.ParseParameters(entry.Parameters, reader);
            string rport = entry.Parameters["rport"];
            if (!string.IsNullOrEmpty(rport)) //parameter can exist, but be empty. = rport requested.
            {
                int value;
                if (!int.TryParse(rport, out value))
                    throw new FormatException("RPORT is not a number.");
                entry.Rport = value;
            }

            return entry;
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
            ViaEntry entry = ParseEntry(reader);
            if (reader.Current != ',')
                return new Via {entry};

            var via = new Via {entry};
            while (reader.Current == ',')
            {
                reader.ConsumeWhiteSpaces(',');
                entry = ParseEntry(reader);
                via.Add(entry);
            }
            return via;
        }

        #endregion
    }
}