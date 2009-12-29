using SipSharp.Headers;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Route or Record-Route header.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Route header: The Route header field is used to force routing for a request through the listed set of proxies
    /// Examples of the use of the Route header field are in Section 16.12.1 in RFC 3261.
    /// </para>
    /// <para>
    /// Record-Route header:
    /// The Record-Route header field is inserted by proxies in a request toforce future requests in the dialog to be routed through the proxy.
    /// Examples of its use with the Route header field are described in Sections 16.12.1 in RFC 3261.
    /// </para>
    /// </remarks>
    /// <example>
    /// 
    /// <![CDATA[
    ///  Record-Route: <sip:server10.biloxi.com;lr>,
    ///    <sip:bigbox3.site3.atlanta.com;lr>
    /// ]]>
    /// <![CDATA[
    /// Route: <sip:bigbox3.site3.atlanta.com;lr>,
    ///    <sip:server10.biloxi.com;lr>
    /// ]]>
    /// </example>
    public class Route : MultiHeader<RouteEntry>
    {
        public const string RECORD_ROUTE_LNAME = "record-route";
        public const string RECORD_ROUTE_NAME = "Record-Route";
        public const string ROUTE_LNAME = "route";
        public const string ROUTE_NAME = "Route";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHeader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        public Route(string name) : base(name)
        {
        }

        public Route(Route route) : base(route)
        {
        }

        /// <summary>
        ///                     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///                     A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object Clone()
        {
            return new Route(this);
        }

        /// <summary>
        /// Create a new header value instance.
        /// </summary>
        /// <returns>A newly created header.</returns>
        protected override IHeader CreateHeader()
        {
            return new RouteEntry(((IHeader) this).Name);
        }
    }
}