using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages.Headers;
using SipSharp.Messages.Headers.Parsers;
using SipSharp.Tools;
using Xunit;

namespace SipSharp.Test.Messages.Headers.Parsers
{
    public class ContactParserTest
    {
        
        [Fact]
        private void Test()
        {
            ContactHeader c = Parse("\"A. G. Bell\" <sip:agb@bell-telephone.com> ;tag=a48s");
            Assert.Equal("A. G. Bell", c.FirstContact.Name);
            Assert.Equal("sip", c.FirstContact.Uri.Scheme);
            Assert.Equal("agb", c.FirstContact.Uri.UserName);
            Assert.Equal("bell-telephone.com", c.FirstContact.Uri.Domain);
            Assert.Equal("a48s", c.Parameters["tag"]);
        }

        private ContactHeader Parse(string text)
        {
            ContactParser parser = new ContactParser();
            return (ContactHeader) parser.Parse("From", new StringReader(text));
        }
    }
}
