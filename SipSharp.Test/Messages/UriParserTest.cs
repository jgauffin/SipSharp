using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Tools;
using Xunit;
using UriParser = SipSharp.Messages.Headers.Parsers.UriParser;

namespace SipSharp.Test.Messages
{
    /// <summary>
    /// Tests for <see cref="UriParser"/>.
    /// </summary>
    public class UriParserTest
    {

        [Fact]
        private void TestParameters()
        {
            KeyValueCollection collection = new KeyValueCollection();
            
            UriParser.ParseParameters(collection, new StringReader(";welcome;lr=true"));
            Assert.Equal(string.Empty, collection["welcome"]);
            Assert.Equal("true", collection["lr"]);

            collection.Clear();
            UriParser.ParseParameters(collection, new StringReader(";welcome"));
            Assert.Equal(string.Empty, collection["welcome"]);

            collection.Clear();
            UriParser.ParseParameters(collection, new StringReader(";welcome;"));
            Assert.Equal(string.Empty, collection["welcome"]);

            collection.Clear();
            UriParser.ParseParameters(collection, new StringReader(";welcome=world"));
            Assert.Equal("world", collection["welcome"]);

            collection.Clear();
            UriParser.ParseParameters(collection, new StringReader(";welcome=world;"));
            Assert.Equal("world", collection["welcome"]);

            collection.Clear();
            UriParser.ParseParameters(collection, new StringReader(";welcome=world;yes"));
            Assert.Equal("world", collection["welcome"]);
            Assert.Equal(string.Empty, collection["yes"]);

            collection.Clear();
            UriParser.ParseParameters(collection, new StringReader(";welcome=world;yes;"));
            Assert.Equal("world", collection["welcome"]);
            Assert.Equal(string.Empty, collection["yes"]);
        }

        [Fact]
        private void TestRequestLine()
        {
            SipUri uri = UriParser.Parse("sips:127.0.0.1:5061");
            Assert.Equal(5061, uri.Port);
            Assert.Equal("sips", uri.Scheme);
            Assert.Equal("127.0.0.1", uri.Domain);

            uri = UriParser.Parse("127.0.0.1:5061");
            Assert.Equal(5061, uri.Port);
            Assert.Equal(string.Empty, uri.Scheme);
            Assert.Equal("127.0.0.1", uri.Domain);

            uri = UriParser.Parse("sips:127.0.0.1");
            Assert.Equal(0, uri.Port);
            Assert.Equal("sips", uri.Scheme);
            Assert.Equal("127.0.0.1", uri.Domain);

        }
    }
}
