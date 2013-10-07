using System;
using SipSharp.Tools;
using Xunit;

namespace SipSharp.Messages.Headers.Parsers
{
    public class UriParser
    {
        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="reader">Reader containing the string that should be parsed.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        /// <example>
        /// sip:jonas@somwhere.com:5060
        /// jonas@somwhere.com:5060
        /// jonas:mypassword@somwhere.com
        /// sip:jonas@somwhere.com
        /// mailto:jonas@somwhere.com
        /// </example>
        public static SipUri Parse(ITextReader reader)
        {
            string first = reader.ReadUntil("@:");
            string scheme = null;
            string userName;
            string password = null;
            string domain;
            int port = 0;
            bool containsAt = reader.Contains('@');


            // scheme:domain:port
            // scheme:domain
            // domain:port
            if (reader.Current == ':' && !containsAt)
            {
                reader.Consume();

                //scheme:domain:port or scheme:domain
                if (IsValidScheme(first))
                {
                    scheme = first;
                    domain = reader.ReadToEnd(":");
                    if (reader.EOF)
                        return new SipUri(scheme, domain, 0);

                    //scheme:domain:port
                    reader.Consume(':');
                    first = reader.ReadToEnd();
                }
                else // domain:port or just domain
                {
                    domain = first;
                    first = reader.ReadToEnd(":");
                }

                if (!int.TryParse(first, out port))
                    throw new ParseException("Port is not a number: " + first);

                return new SipUri(scheme, domain, port);
            }
                // Can either be "scheme:username" 
                // or "username:password" 
                // or "scheme:username:password"
            else if (reader.Current == ':')
            {
                reader.Consume();


                // Check if we got another colon (scheme:username:password)
                string second = reader.ReadUntil(":@");
                if (reader.Current == ':')
                {
                    scheme = first;
                    userName = second;
                    reader.Consume();
                    password = reader.ReadUntil('@');
                }
                    // it's "scheme:username" or "username:password"
                else
                {
                    //TODO: Create a ProtocolProvider singleton
                    if (first == "tel" || first == "sip" || first == "sips" || first == "mailto")
                    {
                        scheme = first;
                        userName = second;
                    }
                    else
                    {
                        userName = first;
                        password = second;
                    }
                }
            }
                // only username
            else
                userName = first;


            reader.Consume(); // eat delimiter.
            domain = reader.ReadToEnd(":;");
            if (reader.Current == '\r' || userName == null)
                return null; // domain was not specified.

            // We got a port.
            if (reader.Current == ':')
            {
                reader.Consume();
                string portStr = reader.ReadToEnd(';');
                if (portStr != null)
                {
                    if (!int.TryParse(portStr, out port))
                        return null;
                }
            }

            // parse parameters
            var values = new KeyValueCollection();
            if (reader.Current == ';')
                ParseParameters(values, reader);

            return new SipUri(scheme, userName, password, domain, port, values);
        }

        /// <summary>
        /// Parse all semicolon separated parameters.
        /// </summary>
        /// <param name="parameters"></param>
        public static void ParseParameters(IKeyValueCollection<string, string> parameters, ITextReader reader)
        {
            ParseParameters(parameters, reader, ';');
        }

        private static bool IsValidScheme(string scheme)
        {
            return scheme == "tel" || scheme == "sip" || scheme == "sips" || scheme == "mailto";
        }

        /// <summary>
        /// Parse all semicolon separated parameters.
        /// </summary>
        /// <param name="parameters">String containing all parameters to parse</param>
        /// <param name="delimiter">Delimiter separating parameters.</param>
        /// <example>
        /// <code>
        /// KeyValueCollection parameters = new KeyValueCollection();
        /// UriParser.ParseParameters(parameters, "hej=hello,welcome=now", ',');
        /// </code>
        /// </example>
        /// <remarks>
        /// Parameter names are converted to lower case.
        /// </remarks>
        public static void ParseParameters(IKeyValueCollection<string, string> parameters, ITextReader reader,
                                           char delimiter)
        {
            reader.Consume(' ', '\t');
            while (!reader.EOF)
            {
                if (reader.Current == delimiter)
                    reader.Consume();
                if (reader.EOF)
                    return;

                reader.Consume(' ', '\t');
                string name = reader.ReadToEnd("=" + delimiter);
                if (name == null)
                    break;
                name = name.ToLower();

                // No semicolon after last parameter.
                if (reader.EOF)
                {
                    parameters.Add(name, string.Empty);
                    return;
                }

                if (reader.Current == delimiter)
                {
                    parameters.Add(name, string.Empty);
                    continue;
                }

                reader.Consume(' ', '\t', '=');
                string value = reader.Current == '"' ? reader.ReadQuotedString() : reader.ReadToEnd(" \t" + delimiter);

                // no value
                if (value == null)
                {
                    parameters.Add(name, string.Empty);
                    continue;
                }
                parameters.Add(name, value);
                reader.Consume(' ', '\t');

                if (reader.Current != delimiter)
                    break;
            }
        }

        /// <summary>
        /// Parse a message value.
        /// </summary>
        /// <param name="text">Text containg uri.</param>
        /// <returns>Newly created header.</returns>
        /// <exception cref="ParseException">Header value is malformed.</exception>
        /// <example>
        /// sip:jonas@somwhere.com:5060
        /// jonas@somwhere.com:5060
        /// jonas:mypassword@somwhere.com
        /// sip:jonas@somwhere.com
        /// mailto:jonas@somwhere.com
        /// </example>
        public static SipUri Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            var reader = new StringReader(text);
            return Parse(reader);
        }

        public static Contact ParseContact(ITextReader reader)
        {
            /*
                When the header field value contains a display name, the URI
                including all URI parameters is enclosed in "<" and ">".  If no "<"
                and ">" are present, all parameters after the URI are header
                parameters, not URI parameters.  The display name can be tokens, or a
                quoted string, if a larger character set is desired.
             */

            reader.ConsumeWhiteSpaces();


            string name;
            if (reader.Current == '\"')
            {
                name = reader.ReadQuotedString();
                reader.Consume('\t', ' ');
            }
            else
                name = string.Empty;

            SipUri uri;

            bool isEnclosed = reader.Current == '<';
            if (reader.Current != '<' && name != string.Empty)
                throw new FormatException("Expected to find '<' in contact.");

            reader.Consume();
            string uriText = isEnclosed ? reader.ReadToEnd('>') : reader.ReadToEnd(';');
            if (uriText == null)
                throw new FormatException("Failed to find '>' in contact.");
            try
            {
                uri = Parse(uriText);
            }
            catch (FormatException err)
            {
                throw new FormatException("Failed to parse uri in contact.", err);
            }
            reader.Consume('>', '\t', ' ');


            // Read parameters.
            var parameters = new KeyValueCollection();
            ParseParameters(parameters, reader);
            return new Contact(parameters) {Name = name, Uri = uri};
        }

#if TEST

        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        private static void Test(string uriString)
        {
            var reader = new StringReader();
            reader.Assign(uriString);

            SipUri uri = Parse(reader);
            if (uri == null)
                throw new InvalidOperationException("Failed to parse: " + uriString);

            Assert.Equal(uriString, uri.ToString());
        }

        [Fact]
        private static void TestCombined()
        {
            Test("sip:caller@example.net;tag=134161461246");
            Test("sip:jonas@somwhere.com:5060");
            Test("jonas@somwhere.com:5060");
            Test("sip:jonas@somwhere.com");
            Test("mailto:jonas@somwhere.com");
            Test("sip:jonas@somwhere.com;lr");
        }
#endif
    }
}