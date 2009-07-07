using System.Collections.Specialized;
using System.IO;
using SipSharp.Headers;

namespace SipSharp
{
    class Response : IResponse
    {
        public Response(string version, StatusCode code, string phrase)
        {
            StatusCode = code;
            SipVersion = version;
            ReasonPhrase = phrase;
            Body = new MemoryStream();
            Headers = new NameValueCollection();
        }

        /// <summary>
        /// Gets body stream.
        /// </summary>
        public Stream Body { get; private set; }

        /// <summary>
        /// All headers.
        /// </summary>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// Gets or sets used version of the SIP protocol.
        /// </summary>
        public string SipVersion { get; set; }

        /// <summary>
        /// Gets or sets whom the message is from.
        /// </summary>
        public Contact From { get; set; }

    	/// <summary>
    	/// Gets or sets transaction identifier.
    	/// </summary>
    	/// <remarks>
    	/// The CSeq header field serves as a way to identify and order transactions. It
    	/// consists of a sequence number and a method. The method MUST match that of the
    	/// request. For non-REGISTER requests outside of a dialog, the sequence number
    	/// value is arbitrary. The sequence number value MUST be expressible as a 32-bit
    	/// unsigned integer and MUST be less than 2**31. As long as it follows the above
    	/// guidelines, a client may use any mechanism it would like to select CSeq header
    	/// field values.
    	/// </remarks>
    	public CSeq CSeq { get; set; }

    	/// <summary>
    	/// Gets proxies that the message have to pass through.
    	/// </summary>
    	/// <remarks>
    	/// <para>The Via header field indicates the transport used for the transaction and
    	/// identifies the location where the message is to be sent. A Via header field
    	/// value is added only after the transport that will be used to reach the next hop
    	/// has been selected.</para>
    	/// </remarks>
    	public Via Via { get; internal set; }

    	/// <summary>
        /// Gets or sets whom the message is to.
        /// </summary>
        public Contact To { get; set; }

        /// <summary>
        /// Gets or sets SIP status code.
        /// </summary>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets text describing why the status code was used.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets where we want to be contacted
        /// </summary>
        public SipUri Contact { get; set; }

        /// <summary>
        /// Gets or sets all routes.
        /// </summary>
        public Route Route { get; internal set;  }

		public string CallId
		{
			get; set;
		}
    }
}
