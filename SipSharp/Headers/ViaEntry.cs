using System;
using System.Collections.Specialized;
using SipSharp.Tools;

namespace SipSharp.Headers
{
    /// <summary>
    /// The Via header field indicates the path taken by the request so far and
    /// indicates the path that should be followed in routing responses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The branch ID parameter in the Via header field values serves as a transaction
    /// identifier, and is used by proxies to detect loops.
    /// </para>
    /// <para>
    /// A Via header field value contains the transport protocol used to send the
    /// message, the client's host name or network address, and possibly the port
    /// number at which it wishes to receive responses. A Via header field value can
    /// also contain parameters such as "maddr", "ttl", "received", and "branch", whose
    /// meaning and use are described in other sections. For implementations compliant
    /// to this specification, the value of the branch parameter MUST start with the
    /// magic cookie "z9hG4bK", as discussed in Section 8.1.1.7.
    /// </para>
    /// <para>
    /// The compact form of the via header is "v".
    /// </para>
    /// </remarks>
    public class ViaEntry : IHeader, ICloneable
    {
        private readonly NameValueCollection _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViaEntry"/> class.
        /// </summary>
        /// <param name="domain">MUST include sip: or any other protocol</param>
        /// <param name="branch"></param>
        public ViaEntry(string domain, string branch)
        {
            SipVersion = "SIP/2.0";
            Rport = 0;
            Domain = domain;
            Branch = branch;
            _parameters = new NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViaEntry"/> class.
        /// </summary>
        public ViaEntry()
        {
            SipVersion = "SIP/2.0";
            Rport = 0;
            _parameters = new NameValueCollection();
        }

    	private ViaEntry(ViaEntry entry)
    	{
    		SipVersion = entry.SipVersion;
    		Protocol = entry.Protocol;
    		Domain = entry.Domain;
    		Rport = entry.Rport;
    		SentBy = entry.SentBy;
    		foreach (string key in entry._parameters)
    			_parameters.Add(key, entry._parameters[key]);
    	}

    	/// <summary>
        /// Gets or sets string used to identify the transaction that created this request.
        /// </summary>
        /// <remarks>
        /// The branch parameter value MUST be unique across space and time for
        /// all requests sent by the UA.  The exceptions to this rule are CANCEL
        /// and ACK for non-2xx responses.  As discussed below, a CANCEL request
        /// will have the same value of the branch parameter as the request it
        /// cancels.
        /// </remarks>
        public string Branch
        {
            get { return _parameters["Branch"]; }
            set { _parameters["Branch"] = value; }
        }

        /// <summary>
        /// Gets or sets domain and port that the response should be sent through.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// if the Via header field value contains a "maddr"
        /// parameter, the response MUST be forwarded to the address listed
        /// there, using the port indicated in "sent-by", or port 5060 if
        /// none is present.
        /// </summary>
        public string Maddr
        {
            get { return _parameters["maddr"]; }
            set { _parameters["maddr"] = value; }
        }

        /// <summary>
        /// Gets or sets port to connect on in server..
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets protocol used for transportation.
        /// </summary>
        /// <example>'UDP' or 'TCP'</example>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets IP address that the request was received from.
        /// </summary>
        public string Received
        {
            get { return _parameters["Received"]; }
            set { _parameters["Received"] = value; }
        }

        /// <summary>
        /// Gets or sets remote port.
        /// </summary>
        public int Rport { get; set; }

        /// <summary>
        /// client requests that the server send the response
        /// back to the source IP address and port from which the request
        ///originated.
        /// 
        /// (rfc 3581)
        /// </summary>
        public bool RportWanted
        {
            get { return _parameters["rport"] != null; }
        }

        ///<summary>
        ///Gets or sets  host (FQDN is preferred), and port (optional), of the one that sent the request.
        ///</summary>
        public string SentBy { get; set; }

        /// <summary>
        /// Gets or sets sip version
        /// </summary>
        /// <example>SIP/2.0</example>
        public string SipVersion { get; set; }

        /// <summary>
        /// Parse a VIA header
        /// </summary>
        /// <param name="reader">Reader containing the string.</param>
        /// <exception cref="FormatException">Expected to find protocol in Via header.</exception>
        internal void Parse(StringReader reader)
        {
            //SIP/2.0/UDP erlang.bell-telephone.com:5060;branch=z9hG4bK87asdks7


            // read SIP/
            SipVersion = reader.Read(" \t/");
            if (SipVersion == null)
                throw new FormatException("Expected Via header to start with 'SIP'.");
            reader.SkipWhiteSpaces('/');

            // read 2.0/
            string temp = reader.Read(" \t/");
            if (temp == null)
                throw new FormatException("Expected to find sip version in Via header.");
            SipVersion += '/' + temp;
            reader.SkipWhiteSpaces('/');

            // read UDP or TCP
            Protocol = reader.ReadWord();
            if (Protocol == null)
                throw new FormatException("Expected to find transport protocol after sip version in Via header.");
            reader.SkipWhiteSpaces();

            Domain = reader.Read(";: \t");
            if (Domain == null)
                throw new FormatException("Failed to find domain in via header.");
            reader.SkipWhiteSpaces();

            if (reader.Current == ':')
            {
                reader.MoveNext();
                reader.SkipWhiteSpaces();
                temp = reader.ReadToEnd("; \t");
                reader.SkipWhiteSpaces();
                int port;
                if (!int.TryParse(temp, out port))
                    throw new FormatException("Invalid port specified.");
                Port = port;
            }

            reader.ParseParameters(_parameters);
            string rport = _parameters["rport"];
            if (rport != null)
            {
                int value;
                if (!int.TryParse(rport, out value))
                    throw new FormatException("RPORT is not a number.");
                Rport = value;
            }
        }

        #region IHeader Members

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return "Via"; }
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
            get { return ToString(); }
        }

        /// <summary>
        /// Gets all header values.
        /// </summary>
        string[] IHeader.Values
        {
            get { return new[] {ToString()}; }
        }

        /// <summary>
        /// Parse a VIA header value.
        /// </summary>
        /// <param name="value">One or more via headers</param>
        /// <exception cref="FormatException">If header contains invalid via header.</exception>
        public void Parse(string value)
        {
            Parse(new StringReader(value));
        }

        #endregion

    	/// <summary>
    	///                     Creates a new object that is a copy of the current instance.
    	/// </summary>
    	/// <returns>
    	///                     A new object that is a copy of this instance.
    	/// </returns>
    	/// <filterpriority>2</filterpriority>
    	public object Clone()
    	{
    		return new ViaEntry(this);
    	}
    }
}