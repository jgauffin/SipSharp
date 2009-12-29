using System;
using SipSharp.Headers;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers.Parsers
{
    /// <summary>
    /// Parses <see cref="Route"/>
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// <sip:bigbox3.site3.atlanta.com;lr>,
    ///    <sip:server10.biloxi.com;lr>
    /// ]]>
    /// </example>
    [ParserFor(Route.ROUTE_NAME, char.MinValue)]
    [ParserFor(Route.RECORD_ROUTE_NAME, char.MinValue)]
    public class RouteParser : IHeaderParser
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
            Route route = new Route(name);
            try
            {
                SipUri uri = UriParser.Parse(reader);
                route.Items.Add(new RouteEntry(name){Uri = uri});
                while (reader.Current == ',')
                {
                    reader.ConsumeWhiteSpaces(',');
                    if (!reader.EOF)
                        uri = UriParser.Parse(reader);
                    route.Items.Add(new RouteEntry(name) { Uri = uri });
                }
            }
            catch (FormatException err)
            {
                throw new ParseException("Failed to parse header '" + name + "'.", err);
            }

            return route;
        }
    }
}
