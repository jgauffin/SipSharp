using System;

namespace SipSharp
{
    /// <summary>
    /// Sip URI.
    /// </summary>
    /// <remarks>
    /// Need to read RFC3261 section 19.1 carefully, and implement
    /// everything described.
    /// </remarks>
    /// <example>
    /// sip:user:password@host:port;uri-parameters?headers
    /// </example>
    public class SipUri : ICloneable, IEquatable<SipUri>
    {
        //public static readonly SipUri Empty = new ReadOnlySipUri();
        private string _domain = string.Empty;
        private IKeyValueCollection _parameters;
        private string _password = string.Empty;
        private int _port;
        private string _scheme = string.Empty;
        private string _userName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        public SipUri()
        {
            _parameters = new KeyValueCollection();
        }

        public SipUri(string domain)
        {
            Domain = domain;
            Parameters = new KeyValueCollection();
        }

        public SipUri(string userName, string domain)
        {
            UserName = userName;
            Domain = domain;
            Parameters = new KeyValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="userName">User name.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        public SipUri(string scheme, string userName, string domain, int port)
        {
            Scheme = scheme;
            UserName = userName;
            Domain = domain;
            Port = port;
            Parameters = new KeyValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        public SipUri(string scheme, string userName, string password, string domain, int port)
            : this(scheme, userName, domain, port)
        {
            Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        /// <param name="values">The values.</param>
        public SipUri(string scheme, string userName, string password, string domain, int port,
                      IKeyValueCollection values)
        {
            Scheme = scheme;
            UserName = userName;
            Domain = domain;
            Port = port;
            Password = password;
            Parameters = values;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">Port number</param>
        public SipUri(string scheme, string domain, int port)
        {
            Scheme = scheme;
            Domain = domain;
            Port = port;
            Parameters = new KeyValueCollection();
        }

        /// <summary>
        /// Gets or sets domain that the user belongs to.
        /// </summary>
        /// <remarks>
        /// The host providing the SIP resource.  The host part contains
        /// either a fully-qualified domain name or numeric IPv4 or IPv6
        /// address.  Using the fully-qualified domain name form is
        /// RECOMMENDED whenever possible.
        /// </remarks>
        public string Domain
        {
            get { return _domain; }
            set { _domain = value ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets optional parameters.
        /// </summary>
        public IKeyValueCollection Parameters
        {
            get { return _parameters; }
            private set { _parameters = value; }
        }

        /// <summary>
        /// Gets or sets a password associated with the user.
        /// </summary>
        /// <remarks>
        /// While the SIP and SIPS URI syntax allows this field to be present, its use is
        /// NOT RECOMMENDED, because the passing of authentication information in clear
        /// text (such as URIs) has proven to be a security risk in almost every case where
        /// it has been used.  For instance, transporting a PIN number in this field
        /// exposes the PIN.
        /// </remarks>
        public string Password
        {
            get { return _password; }
            set { _password = value ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets the port number to use.
        /// </summary>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Gets or sets uri scheme
        /// </summary>
        /// <remarks>
        /// Usually is 'sip' or 'sips'.
        /// </remarks>
        /// <value>
        /// Always lower case.
        /// </value>
        public string Scheme
        {
            get { return _scheme; }
            set
            {
                _scheme = value ?? string.Empty;
                _scheme = _scheme.ToLower();
            }
        }

        /// <summary>
        /// Gets or sets user name.
        /// </summary>
        /// <para>
        /// The identifier of a particular resource at the host being
        /// addressed.  The term "host" in this context frequently refers
        /// to a domain.  The "userinfo" of a URI consists of this user
        /// field, the password field, and the @ sign following them.  The
        /// user info part of a URI is optional and MAY be absent when the
        /// destination host does not have a notion of users or when the
        /// host itself is the resource being identified.  If the @ sign is
        /// present in a SIP or SIPS URI, the user field MUST NOT be empty.
        /// </para><para>
        /// If the host being addressed can process telephone numbers, for
        /// instance, an Internet telephony gateway, a telephone-
        /// subscriber field defined in RFC 2806 [9] MAY be used to
        /// populate the user field.  There are special escaping rules for
        /// encoding telephone-subscriber fields in SIP and SIPS URIs
        /// described in Section 19.1.2.
        /// </para>
        public string UserName
        {
            get { return _userName; }
            set { _userName = value ?? string.Empty; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as SipUri;
            if (other == null)
                return false;

            return other.Domain.Equals(Domain, StringComparison.OrdinalIgnoreCase)
                   && other.Parameters.Count == Parameters.Count
                   && other.Password.Equals(Password, StringComparison.OrdinalIgnoreCase)
                   && other.Port == Port
                   && other.Scheme.Equals(Scheme, StringComparison.OrdinalIgnoreCase)
                   && other.UserName.Equals(UserName, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Will only add scheme and port if specified.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string temp = string.Empty;
            if (!string.IsNullOrEmpty(_scheme))
                temp += _scheme + ':';
            if (!string.IsNullOrEmpty(_userName))
                temp += _userName + '@' + _domain;
            else
                temp += _domain;
            if (_port != 0)
                temp += ":" + _port;

            foreach (var pair in _parameters)
            {
                if (string.IsNullOrEmpty(pair.Value))
                    temp += ';' + pair.Key;
                else
                    temp += ';' + pair.Key + '=' + pair.Value;
            }

            return temp;
        }

        #region ICloneable Members

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public object Clone()
        {
            return new SipUri(_scheme, _userName, _password, _domain, _port, (IKeyValueCollection) _parameters.Clone());
        }

        #endregion

        #region IEquatable<SipUri> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(SipUri other)
        {
            return other.Domain.Equals(Domain, StringComparison.OrdinalIgnoreCase)
                   && other.Parameters.Count == Parameters.Count
                   && other.Password.Equals(Password, StringComparison.OrdinalIgnoreCase)
                   && other.Port == Port
                   && other.Scheme.Equals(Scheme, StringComparison.OrdinalIgnoreCase)
                   && other.UserName.Equals(UserName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}