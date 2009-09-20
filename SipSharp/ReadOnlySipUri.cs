using System;

namespace SipSharp
{
    /// <summary>
    /// Read only version of <see cref="SipUri"/>
    /// </summary>
    public sealed class ReadOnlySipUri : SipUri
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlySipUri"/> class.
        /// </summary>
        public ReadOnlySipUri()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="userName">User name.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        public ReadOnlySipUri(string protocol, string userName, string domain, int port) : base(protocol, userName, domain, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipUri"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="port">The port.</param>
        public ReadOnlySipUri(string protocol, string userName, string password, string domain, int port) : base(protocol, userName, password, domain, port)
        {
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
        public ReadOnlySipUri(string protocol, string userName, string password, string domain, int port, IKeyValueCollection values) : base(protocol, userName, password, domain, port, values)
        {
        }

        /// <summary>
        /// Gets or sets domain that the user belongs to.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// The host providing the SIP resource.  The host part contains
        /// either a fully-qualified domain name or numeric IPv4 or IPv6
        /// address.  Using the fully-qualified domain name form is
        /// RECOMMENDED whenever possible.
        /// </remarks>
        /// <exception cref="InvalidOperationException">SipUri is read-only.</exception>
        public override string Domain
        {
            get { return base.Domain; }
            set { throw new InvalidOperationException("SipUri is read-only."); }
        }

        /// <summary>
        /// Gets or sets optional parameters.
        /// </summary>
        /// <value></value>
        /// <exception cref="InvalidOperationException">SipUri is read-only.</exception>
        public override IKeyValueCollection Parameters
        {
            get { return (IKeyValueCollection)Parameters.AsReadOnly(); }
            set { throw new InvalidOperationException("SipUri is read-only."); }
        }

        /// <summary>
        /// Gets or sets a password associated with the user.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// While the SIP and SIPS URI syntax allows this field to be present, its use is
        /// NOT RECOMMENDED, because the passing of authentication information in clear
        /// text (such as URIs) has proven to be a security risk in almost every case where
        /// it has been used.  For instance, transporting a PIN number in this field
        /// exposes the PIN.
        /// </remarks>
        /// <exception cref="InvalidOperationException">SipUri is read-only.</exception>
        public override string Password
        {
            get { return base.Password; }
            set { throw new InvalidOperationException("SipUri is read-only."); }
        }

        /// <summary>
        /// Gets or sets the port number to use.
        /// </summary>
        /// <value></value>
        /// <exception cref="InvalidOperationException">SipUri is read-only.</exception>
        public override int Port
        {
            get { return base.Port; }
            set { throw new InvalidOperationException("SipUri is read-only."); }
        }

        /// <summary>
        /// Gets or sets protocol.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Usually is 'SIP' or 'SIPS'.
        /// </remarks>
        /// <exception cref="InvalidOperationException">SipUri is read-only.</exception>
        public override string Scheme
        {
            get { return base.Scheme; }
            set { throw new InvalidOperationException("SipUri is read-only."); }
        }

        /// <summary>
        /// Gets or sets user name.
        /// </summary>
        /// <value></value>
        /// <para>
        /// The identifier of a particular resource at the host being
        /// addressed.  The term "host" in this context frequently refers
        /// to a domain.  The "userinfo" of a URI consists of this user
        /// field, the password field, and the @ sign following them.  The
        /// user info part of a URI is optional and MAY be absent when the
        /// destination host does not have a notion of users or when the
        /// host itself is the resource being identified.  If the @ sign is
        /// present in a SIP or SIPS URI, the user field MUST NOT be empty.
        /// </para>
        /// <para>
        /// If the host being addressed can process telephone numbers, for
        /// instance, an Internet telephony gateway, a telephone-
        /// subscriber field defined in RFC 2806 [9] MAY be used to
        /// populate the user field.  There are special escaping rules for
        /// encoding telephone-subscriber fields in SIP and SIPS URIs
        /// described in Section 19.1.2.
        /// </para>
        /// <exception cref="InvalidOperationException">SipUri is read-only.</exception>
        public override string UserName
        {
            get { return base.UserName; }
            set { throw new InvalidOperationException("SipUri is read-only."); }
        }
    }
}