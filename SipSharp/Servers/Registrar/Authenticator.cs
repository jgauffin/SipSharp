using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages.Headers;

namespace SipSharp.Servers.Registrar
{
    /// <summary>
    /// Helper taking care of WWW-Authenticate and Authorization headers.
    /// </summary>
    public class Authenticator
    {
        private NonceManager _nonce = new NonceManager();

        public WwwAuthenticate CreateWwwHeader(string realm, SipUri domain)
        {
            WwwAuthenticate header = new WwwAuthenticate
                                         {
                                             Algortihm = "MD5",
                                             Domain = domain,
                                             Realm = realm,
                                             Nonce = _nonce.Create(),
                                             Opaque = Guid.NewGuid().ToString().Replace("-", string.Empty),
                                             Qop = "auth",
                                             Stale = false
                                         };
            return header;
        }
    }
}
