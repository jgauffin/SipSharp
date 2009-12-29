using System;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parses <see cref="Authorization"/> header.
    /// </summary>
    [ParserFor(Authorization.NAME, char.MinValue)]
    [ParserFor(Authorization.PROXY_NAME, char.MinValue)]
    public class AuthorizationParser : IHeaderParser
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
        /// Authorization: Digest username="bob",
        ///                 realm="biloxi.com",
        ///                 nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093",
        ///                 uri="sip:bob@biloxi.com",
        ///                 qop=auth,
        ///                 nc=00000001,
        ///                 cnonce="0a4f113b",
        ///                 response="6629fae49393a05397450978507c4ef1",
        ///                 opaque="5ccc069c403ebaf9f0171e9517f40e41"
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

            var header = new Authorization(name)
                             {
                                 UserName = parameters["username"],
                                 Realm = parameters["realm"],
                                 Nonce = parameters["nonce"],
                                 Qop = parameters["qop"],
                                 ClientNonce = parameters["cnonce"],
                                 Opaque = parameters["opaque"],
                                 Response = parameters["response"],
                                 Uri = UriParser.Parse(parameters["uri"])
                             };

            try
            {
                header.NonceCounter = int.Parse(parameters["nc"]);
            }
            catch (Exception err)
            {
                throw new ParseException("Failed to parse 'nc' in Authorization header.", err);
            }

            return header;
        }

        #endregion
    }
}