using System;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// A WWW or Proxy Authenticate header field.
    /// </summary>
    /// <remarks>
    /// First part in authentication process. The UAS sends this header
    /// and the UAC responds with a <see cref="Authorization"/> header.
    /// </remarks>
    /// <example>
    /// Digest realm="atlanta.com",
    /// domain="sip:boxesbybob.com", qop="auth",
    /// nonce="f84f1cec41e6cbe5aea9c8e88d359",
    /// opaque="", stale=FALSE, algorithm=MD5
    /// </example>
    public class Authenticate : IHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticate"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        public Authenticate(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticate"/> class.
        /// </summary>
        public Authenticate()
        {
            Name = WWW_NAME;
        }

        /// <summary>
        /// Gets or sets domain used to calc response.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets authentication domain.
        /// </summary>
        public SipUri Domain { get; set; }

        /// <summary>
        /// Gets or sets quality of protection.
        /// </summary>
        /// <remarks>
        /// If present, its value MUST be one of the alternatives the server indicated it supports in the WWW-Authenticate header
        /// </remarks>
        public string Qop { get; set; }

        /// <summary>
        /// Gets or sets a server-specified data string which should be uniquely generated each time a 401 response is made.
        /// </summary>
        /// <remarks>
        /// It is recommended that this string be base64 or hexadecimal data. Specifically, since the string is passed in the header lines as a quoted string, the double-quote character is not allowed.
        /// </remarks>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets a string of data, specified by the server, which should be returned by the client unchanged in the Authorization header of subsequent requests with URIs in the same protection space.
        /// </summary>
        public string Opaque { get; set; }

        /// <summary>
        /// Gets or sets a string indicating a pair of algorithms used to produce the digest and a checksum.
        /// </summary>
        /// <remarks>
        /// If this is not present it is assumed to be "MD5". If the algorithm is not understood, the challenge should be ignored (and a different one used, if there is more than one).
        /// </remarks>
        public string Algortihm { get; set; }

        /// <summary>
        /// Gets or sets stale.
        /// </summary>
        public bool Stale { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
    {
        get; private
        set;
    }

        public const string WWW_NAME = "WWW-Authenticate";
        public const string WWW_LNAME = "www-authenticate";
        public const string PROXY_NAME = "Proxy-Authenticate";
        public const string PROXY_LNAME = "proxy-authenticate";


        public override string ToString()
        {
            return
                string.Format(
                    @"Digest realm=""{0}"",
    domain=""{1}"", 
    qop=""{2}"",
    nonce=""{3}"",
    opaque=""{4}"", 
    stale={5}, 
    algorithm={6}",
                    Realm, Domain, Qop, Nonce, Opaque, Stale.ToString().ToUpper(), Algortihm);
        }

    }
}
