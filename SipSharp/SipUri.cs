using System;
using System.Collections.Specialized;
using SipSharp.Tools;
using Xunit;

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
    public class SipUri : ICloneable
    {
        public static readonly SipUri Empty = new ReadOnlySipUri();
        private string _domain = string.Empty;
        private NameValueCollection _parameters;
        private string _password = string.Empty;
        private int _port;
        private string _protocol = string.Empty;
        private string _userName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        public SipUri()
        {
            _parameters = new NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="userName">User name.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        public SipUri(string protocol, string userName, string domain, int port)
        {
            _protocol = protocol;
            _userName = userName;
            _domain = domain;
            _port = port;
            _parameters = new NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        public SipUri(string protocol, string userName, string password, string domain, int port)
            : this(protocol, userName, domain, port)
        {
            _password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        /// <param name="values">The values.</param>
        public SipUri(string protocol, string userName, string password, string domain, int port,
                      NameValueCollection values)
        {
            _protocol = protocol;
            _userName = userName;
            _domain = domain;
            _port = port;
            _password = password;
            _parameters = values;
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
        public virtual string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>
        /// Gets or sets optional parameters.
        /// </summary>
        public virtual NameValueCollection Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
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
        public virtual string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Gets or sets the port number to use.
        /// </summary>
        public virtual int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// Gets or sets protocol.
        /// </summary>
        /// <remarks>
        /// Usually is 'SIP' or 'SIPS'.
        /// </remarks>
        public virtual string Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
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
        public virtual string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        /// <summary>
        /// Parse a URI.
        /// </summary>
        /// <param name="uri">String to parse</param>
        /// <returns><see cref="SipUri"/> if string could be parsed. Otherwise null.</returns>
        /// <example>
        /// sip:jonas@somwhere.com:5060
        /// jonas@somwhere.com:5060
        /// jonas:mypassword@somwhere.com
        /// sip:jonas@somwhere.com
        /// mailto:jonas@somwhere.com
        /// </example>
        public static SipUri Parse(string uri)
        {
            var reader = new StringReader(uri);
            string protocol = reader.Read(':', '@'); // position should be reseted
            if (reader.Current == ':')
                reader.MoveNext(); // skip delimiter
            string userName = reader.Read('@', ':');
            if (reader.Current == '@')
                reader.MoveNext(); // skip delimiter
            string password = null;
            if (userName == null)
                userName = reader.Read('@');
            else
                password = reader.Read('@');
            string domain = reader.ReadToEnd(":;");
            if (userName == null || domain == null)
                return null;

            // We got a port.
            int port = 0;
            if (reader.Current == ':')
            {
                if (!reader.MoveNext())
                    return null;
                string portStr = reader.ReadToEnd(';');
                if (portStr != null)
                {
                    if (!int.TryParse(portStr, out port))
                        return null;
                }
            }

            // parse parameters
            NameValueCollection values = null;
            if (reader.Current == ';')
            {
                values = new NameValueCollection();
                reader.ParseParameters(values);
            }

            return new SipUri(protocol, userName, password, domain, port, values);
        }

#if TEST

        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        private static void Test(string uriString)
        {
            SipUri uri = Parse(uriString);
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

        /// <summary>
        /// Will only add protocol and port if specified.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string temp = string.Empty;
            if (_protocol != null)
                temp += _protocol + ':';
            temp += _userName + '@' + _domain;
            if (_port != 0)
                temp += ":" + _port;

            foreach (string key in _parameters)
                temp += ';' + key + '=' + _parameters[key];

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
            var uri = new SipUri(_protocol, _userName, _password, _domain, _port);
            for (int i = 0; i < _parameters.Count; ++i)
                uri.Parameters.Add(_parameters.Keys[i], _parameters[i]);

            return uri;
        }

        #endregion

    }
}