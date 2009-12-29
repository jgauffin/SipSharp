using SipSharp.Messages.Headers;
using SipSharp.Messages.Headers.Parsers;
using SipSharp.Tools;
using Xunit;

namespace SipSharp.Test.Messages.Headers.Parsers
{
    public class RouteHeaderTest
    {
        private readonly RouteParser _parser;

        [Fact]
        private void Test()
        {
            IHeader header = _parser.Parse("Route",
                                           new StringReader(
                                               "<sip:bigbox3.site3.atlanta.com;lr>,\r\n <sip:server10.biloxi.com)"));
            Assert.IsType(typeof (Route), header);
            var route = (Route) header;
            Assert.Equal("bigbox3.site3.atlanta.com", route.Items[0].Uri.Domain);
            Assert.Equal("sip", route.Items[0].Uri.Scheme);
            Assert.True(route.Items[0].IsLoose);
            Assert.Equal("server10.biloxi.com", route.Items[1].Uri.Domain);
            Assert.Equal("sip", route.Items[1].Uri.Scheme);
            Assert.False(route.Items[1].IsLoose);
        }
    }
}