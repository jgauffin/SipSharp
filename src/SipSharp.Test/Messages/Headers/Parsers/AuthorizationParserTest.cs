using SipSharp.Messages.Headers;
using SipSharp.Messages.Headers.Parsers;
using SipSharp.Tools;
using Xunit;

namespace SipSharp.Test.Messages.Headers.Parsers
{
    public class AuthorizationParserTest
    {
        private readonly AuthorizationParser _parser = new AuthorizationParser();

        [Fact]
        private void Normal()
        {
            // parser will always merge headers to one line.
            string test =
                "Digest username=\"bob\"," +
                "realm=\"biloxi.com\"," +
                "nonce=\"dcd98b7102dd2f0e8b11d0f600bfb0c093\"," +
                "uri=\"sip:bob@biloxi.com\"," +
                "qop=auth," +
                "nc=00000001," +
                "cnonce=\"0a4f113b\"," +
                "response=\"6629fae49393a05397450978507c4ef1\"," +
                "opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"";

            var header = (Authorization) _parser.Parse("Authorization", new StringReader(test));
            Assert.Equal("bob", header.UserName);
            Assert.Equal("biloxi.com", header.Realm);
            Assert.Equal("dcd98b7102dd2f0e8b11d0f600bfb0c093", header.Nonce);
            Assert.Equal("auth", header.Qop);
            Assert.Equal(1, header.NonceCounter);
            Assert.Equal("0a4f113b", header.ClientNonce);
            Assert.Equal("6629fae49393a05397450978507c4ef1", header.Response);
            Assert.Equal("5ccc069c403ebaf9f0171e9517f40e41", header.Opaque);
            Assert.Equal("sip", header.Uri.Scheme);
            Assert.Equal("bob", header.Uri.UserName);
            Assert.Equal("biloxi.com", header.Uri.Domain);
        }
    }
}