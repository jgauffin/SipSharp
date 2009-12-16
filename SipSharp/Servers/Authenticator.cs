using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages.Headers;
using SipSharp.Servers.Registrar;

namespace SipSharp.Servers
{
    /// <summary>
    /// Helper taking care of WWW-Authenticate and Authorization headers.
    /// </summary>
    public class Authenticator
    {
        private NonceManager _nonce = new NonceManager();

        public Authenticate CreateWwwHeader(string realm, SipUri domain)
        {
            Authenticate header = new Authenticate(Authenticate.WWW_NAME)
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

        public bool IsNonceValid(string nonce)
        {
            return _nonce.IsValid(nonce);
        }

        public bool IsAuthenticated(IRequest request)
        {

            if (e.Request.Headers[Authorization.LNAME] == null)
            {
                response.StatusCode = StatusCode.Unauthorized;
                response.ReasonPhrase = "You must authorize";
                IHeader wwwHeader = _authenticator.CreateWwwHeader(Realm, Domain);
                response.Headers.Add(Authenticate.WWW_NAME, wwwHeader);
                transaction.Send(response);
                return;
            }
            
        }
    }
}