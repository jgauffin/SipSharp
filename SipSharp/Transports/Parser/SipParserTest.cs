#if TEST
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading;
using SipSharp.Tests;
using Xunit;

namespace SipSharp.Transports.Parser
{
    /// <summary>
    /// Tests for <see cref="RequestParser"/>.
    /// </summary>
    public class SipParserTest
    {
        private IMessageParser _parser;
        private ResponseLineEventArgs _responseLineEventArgs;
        private RequestLineEventArgs _requestLineArgs;
        private NameValueCollection _headers = new NameValueCollection();
        private MemoryStream _body = new MemoryStream();
        private ManualResetEvent _completed = new ManualResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestParserTest"/> class.
        /// </summary>
        public SipParserTest()
        {
            _parser = new SipParser();
            _parser.BodyBytesReceived += OnBody;
            _parser.HeaderReceived += OnHeader;
            _parser.MessageCompleted += OnMessageComplete;
            _parser.RequestLineReceived += OnRequestLine;
            _parser.ResponseLineReceived += OnResponseLine;
            _parser.LogWriter = ConsoleLogWriter.Instance;
        }

        private void OnResponseLine(object sender, ResponseLineEventArgs e)
        {
            _responseLineEventArgs = e;
        }

        private void OnRequestLine(object sender, RequestLineEventArgs e)
        {
            _requestLineArgs = e;
        }

        private void OnMessageComplete(object sender, EventArgs e)
        {
            _completed.Set();
        }

        private void OnHeader(object sender, HeaderEventArgs e)
        {
            _headers.Add(e.Name, e.Value);
        }

        private void OnBody(object sender, BodyEventArgs e)
        {
            _body.Write(e.Buffer, e.Offset, e.Count);
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
            Assert.Equal(@"""J Rosenberg \\\""""       <sip:jdrosen@example.com>  ;  tag = 98asjd8", _headers["from"]);
            Assert.Equal("0009  INVITE", _headers["cseq"]);

            _body.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(_body);
            string body = reader.ReadToEnd();
        }

        [Fact]
        private void TestSpanningHeader()
        {
            byte[] test = Encoding.UTF8.GetBytes("INVITE sip:vivekg@chair-dnrc.example.com SIP/2.0\r\nHeader  \t: Spanning\r\n over multiple lines.\r\n");
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
            _parser.Parse(test, 0, 10);
            _parser.Parse(test, 10, test.Length - 10);
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
            _parser.Parse(test, 0, 50);
            _parser.Parse(test, 50, test.Length - 50);
            Assert.Equal("Spanning over multiple lines.", _headers["Header"]);
            Assert.Equal("header", _headers["Another"]);
            Assert.Equal("a third one.", _headers["Yet"]);
        }

    }
}
#endif