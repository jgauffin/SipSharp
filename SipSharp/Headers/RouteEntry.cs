using System;

namespace SipSharp.Headers
{
    /// <summary>
    /// One route entry
    /// </summary>
    public class RouteEntry : IHeader
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteEntry"/> class.
        /// </summary>
        public RouteEntry()
            : this("Route")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteEntry"/> class.
        /// </summary>
        /// <param name="headerName">Name of the header.</param>
        public RouteEntry(string headerName)
        {
            _name = headerName;
            Uri = SipUri.Empty;
        }

        /// <summary>
        /// True if loose routing is used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A proxy is said to be loose routing if it follows the procedures defined in
        /// RFC3261 for processing of the Route header field. These procedures
        /// separate the destination of the request (present in the Request-URI) from  the
        /// set of proxies that need to be visited along the way (present in the Route
        /// header field). A proxy compliant to these mechanisms is also known as a loose
        /// router.
        /// </para>
        /// </remarks>
        public bool IsLoose
        {
            get { return Uri.Parameters["lr"] != null; }
            set
            {
                if (value)
                    Uri.Parameters["lr"] = string.Empty;
                else
                    Uri.Parameters.Remove("lr");
            }
        }

        /// <summary>
        /// Gets or sets routing URI.
        /// </summary>
        public SipUri Uri { get; set; }

        /// <summary>
        /// Returns header as string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ((IHeader) this).Value;
        }

        #region IHeader Members

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets header value(s).
        /// </summary>
        /// <remarks>
        /// Values will be combined (separated by comma) if more than
        /// one value exists.
        /// </remarks>
        string IHeader.Value
        {
            get
            {
                return IsLoose
                           ? string.Format("<{0};lr>", Uri)
                           : string.Format("<{0}>", Uri);
            }
        }

        /// <summary>
        /// Gets all header values.
        /// </summary>
        string[] IHeader.Values
        {
            get { return new[] {((IHeader) this).Value}; }
        }


        /// <summary>
        /// Parse a value.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times.
        /// </remarks>
        /// <param name="value">string containing one or more values.</param>
        /// <exception cref="FormatException">If value format is incorrect.</exception>
        void IHeader.Parse(string value)
        {
            Uri = SipUri.Parse(value);
        }

        #endregion
    }
}