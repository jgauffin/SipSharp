using System;
using SipSharp.Messages.Headers;

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
    /// <example>
    /// 
    /// </example>
    public class ViaEntry : IHeader, ICloneable
    {
        private readonly KeyValueCollection _parameters = new KeyValueCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="ViaEntry"/> class.
        /// </summary>
        /// <param name="domain">MUST include sip: or any other protocol</param>
        /// <param name="branch"></param>
        public ViaEntry(string domain, string branch)
        {
            SipVersion = "2.0";
            Protocol = "SIP";
            Rport = 0;
            Domain = domain;
            Branch = branch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViaEntry"/> class.
        /// </summary>
        public ViaEntry()
        {
            SipVersion = "2.0";
            Protocol = "SIP";
            Rport = 0;
        }

        private ViaEntry(ViaEntry entry)
        {
            Protocol = entry.Protocol;
            SipVersion = entry.SipVersion;
            Transport = entry.Transport;
            Domain = entry.Domain;
            foreach (var pair in entry._parameters)
                _parameters.Add(pair.Key, pair.Value);
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

        internal KeyValueCollection Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets or sets port to connect on in server..
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets protocol.
        /// </summary>
        /// <value>"SIP" or "SIPS"</value>
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
        public int Rport
        {
            get { return int.Parse(_parameters["rport"]); }
            set
            {
                if (value <= 0)
                    _parameters.Remove("rport");
                else
                    _parameters["rport"] = value.ToString();
            }
        }

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
            set
            {
                if (value)
                    _parameters["rport"] = null;
                else
                    _parameters.Remove("rport");
            }
        }

        ///<summary>
        ///Gets or sets  host (FQDN is preferred), and port (optional), of the one that sent the request.
        ///</summary>
        public string SentBy
        {
            get { return _parameters["sent-by"]; }
            set { _parameters["sent-by"] = value; }
        }

        /// <summary>
        /// Gets or sets sip version
        /// </summary>
        /// <example>2.0</example>
        public string SipVersion { get; set; }

        /// <summary>
        /// Gets or sets protocol used for transportation.
        /// </summary>
        /// <example>'UDP' or 'TCP'</example>
        public string Transport { get; set; }

        public override string ToString()
        {
            string temp = Protocol + '/' + SipVersion + '/' + Transport + ' ' + Domain;
            if (Port > 0)
                temp += ":" + Port;

            foreach (var parameter in Parameters)
            {
                temp += ";" + parameter.Key;
                if (!string.IsNullOrEmpty(parameter.Value))
                    temp += "=" + parameter.Value;
            }

            return temp;
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
            var entry = other as ViaEntry;
            if (entry == null)
                return false;

            return string.Compare(Transport, entry.Transport, true) == 0
                   && string.Compare(Protocol, entry.Protocol, true) == 0
                   && string.Compare(Domain, entry.Domain, true) == 0
                   && Port == entry.Port
                   && Parameters.Count == entry.Parameters.Count;
        }

        #endregion
    }
}