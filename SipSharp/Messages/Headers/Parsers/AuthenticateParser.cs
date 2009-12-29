using System;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parser for <see cref="Authenticate"/>
    /// </summary>
    [ParserFor(Authenticate.PROXY_NAME, char.MinValue)]
    [ParserFor(Authenticate.WWW_NAME, char.MinValue)]
    public class AuthenticateParser : IHeaderParser
    {
        #region IHeaderParser Members

        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="name">Name of header being parsed.</param>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        /// <example>
        /// Digest realm="atlanta.com",
        /// domain="sip:boxesbybob.com", qop="auth",
        /// nonce="f84f1cec41e6cbe5aea9c8e88d359",
        /// opaque="", stale=FALSE, algorithm=MD5
        /// </example>
        public IHeader Parse(string name, ITextReader reader)
        {
            reader.ConsumeWhiteSpaces();
            string digest = reader.ReadWord().ToLower();
            if (digest != "digest")
                throw new ParseException("Authorization header is not digest authentication");

            reader.ConsumeWhiteSpaces();
            var parameters = new KeyValueCollection();
            UriParser.ParseParameters(parameters, reader, ',');

            var header = new Authenticate(name)
                             {
                                 Algortihm = parameters["algorithm"],
                                 Domain = UriParser.Parse(parameters["domain"]),
                                 Realm = parameters["realm"],
                                 Nonce = parameters["nonce"],
                                 Qop = parameters["qop"],
                                 Opaque = parameters["opaque"]
                             };

            try
            {
                header.Stale = bool.Parse(parameters["stale"]);
            }
            catch (Exception err)
            {
                throw new ParseException("Failed to parse 'stale' in WWW-Authenticate header.", err);
            }

            return header;
        }

        #endregion
    }
}