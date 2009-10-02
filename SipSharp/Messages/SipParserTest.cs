using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SipSharp.Messages;
using SipSharp.Tests;
using SipSharp.Transports.Parser;
using Xunit;

namespace SipSharp.Parser
{
    public class SipParserTest
    {
        private SipParser _parser;
        private NameValueCollection _headers = new NameValueCollection();
        private MemoryStream _body = new MemoryStream();
        private bool _isComplete;

        public SipParserTest()
        {
            _parser = new SipParser();
            _parser.HeaderParsed += OnHeader;
            _parser.BodyBytesReceived += OnBody;
            _parser.MessageComplete += OnComplete;
        }

        private void OnComplete(object sender, EventArgs e)
        {
            _body.Flush();
            _body.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(_body);
            Debug.WriteLine(reader.ReadToEnd());
            _isComplete = true;
        }

        private void OnBody(object sender, BodyEventArgs e)
        {
            _body.Write(e.Buffer, e.Offset, e.Count);
        }



        private void OnHeader(object sender, HeaderEventArgs e)
        {
            Debug.WriteLine(e.Name + "= " + e.Value);
            _headers.Add(e.Name, e.Value);
        }



        /// <summary>
        /// TODO: SipParser header values with colon and stuff.
        /// Read if header values can contain spaces.
        /// </summary>
        [Fact]
        private void TestAShortTortuousINVITE()
        {

            byte[] bytes = Encoding.ASCII.GetBytes(TestMessages.AShortTortuousINVITE);
            _parser.Parse(bytes, 0, bytes.Length);
            Assert.Equal("sip:vivekg@chair-dnrc.example.com ;   tag    = 1918181833n", _headers["to"]);
            Assert.Equal(@"""J Rosenberg \\\""""       <sip:jdrosen@example.com> ; tag = 98asjd8", _headers["from"]);
            Assert.Equal("0009 INVITE", _headers["cseq"]);

            _body.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(_body);
            string body = reader.ReadToEnd();
        }

        [Fact]
        private void TestSpanningHeader()
        {
            byte[] test = Encoding.UTF8.GetBytes(@"INVITE sip:vivekg@chair-dnrc.example.com SIP/2.0
Header  : 
	Spanning
	 over multiple lines.
");
            _parser.Parse(test, 0, test.Length);
            Assert.Equal("Spanning over multiple lines.", _headers["Header"]);
        }

        [Fact]
        private void TestHeadersHeaders()
        {
            byte[] test = Encoding.UTF8.GetBytes(@"INVITE sip:vivekg@chair-dnrc.example.com SIP/2.0
Header  	: Spanning
  over multiple lines.
Another:header
Yet: a third
	 one.
");
            _parser.Parse(test, 0, test.Length);
            Assert.Equal("Spanning over multiple lines.", _headers["Header"]);
            Assert.Equal("header", _headers["Another"]);
            Assert.Equal("a third one.", _headers["Yet"]);
        }

        [Fact]
        private void TestPartialRequestLine()
        {
            byte[] test = Encoding.UTF8.GetBytes(@"INVITE sip:vivekg@chair-dnrc.example.com SIP/2.0
Header  	: Spanning
  over multiple lines.
Another:header
Yet: a third
	 one.
");
            int index = _parser.Parse(test, 0, 10);
            _parser.Parse(test, index, test.Length);
            Assert.Equal("Spanning over multiple lines.", _headers["Header"]);
            Assert.Equal("header", _headers["Another"]);
            Assert.Equal("a third one.", _headers["Yet"]);
        }

        [Fact]
        private void TestPartialHeader()
        {
            byte[] test = Encoding.UTF8.GetBytes(@"INVITE sip:vivekg@chair-dnrc.example.com SIP/2.0
Header  	: Spanning
 over multiple lines.
Another:header
Yet: a third
	one.
");
            int index = _parser.Parse(test, 0, 50);
            _parser.Parse(test, index, test.Length);
            Assert.Equal("Spanning over multiple lines.", _headers["Header"]);
            Assert.Equal("header", _headers["Another"]);
            Assert.Equal("a third one.", _headers["Yet"]);
        }

    }
}
