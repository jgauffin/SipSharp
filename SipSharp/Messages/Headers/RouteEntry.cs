using System;
using SipSharp.Messages.Headers;
using SipSharp.Tools;

namespace SipSharp.Headers
{
    /// <summary>
    /// One route entry
    /// </summary>
    public class RouteEntry : IHeader, ICloneable
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
            Uri = new SipUri();
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
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new RouteEntry(_name);
        }

        #region IHeader Members

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return _name; }
        }



        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(IHeader other)
        {
            RouteEntry entry = other as RouteEntry;
            if (entry != null)
                return entry.Uri == Uri;

            return false;
        }
    }
}