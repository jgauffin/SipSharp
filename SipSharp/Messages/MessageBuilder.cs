using System;
using SipSharp.Headers;
using SipSharp.Messages.Headers;
using SipSharp.Parser;
using SipSharp.Transports.Parser;

namespace SipSharp.Messages
{
    /// <summary>
    /// Creates a single message for one of the end points.
    /// </summary>
    /// <remarks>
    /// The factory is 
    /// </remarks>
    internal class MessageBuilder : IDisposable
    {
        private readonly MessageFactory _msgFactory;
        private readonly HeaderFactory _factory;
        private readonly SipParser _parser;

        private delegate void HeaderHandler(string name, string value);
        private HeaderHandler _onHeader;
        private Message _message;

        public MessageBuilder(MessageFactory msgFactory, HeaderFactory factory, SipParser parser)
        {
            _msgFactory = msgFactory;
            _factory = factory;
            _parser = parser;
            parser.HeaderParsed += OnHeader;
            parser.MessageComplete += OnMessageComplete;
            parser.RequestLineParsed += OnRequestLine;
            parser.ResponseLineParsed += OnResponseLine;
        }

        private void OnResponseLine(object sender, ResponseLineEventArgs e)
        {
            _message = _msgFactory.CreateResponse(e.Version, e.StatusCode, e.ReasonPhrase);
        }

        private void OnRequestLine(object sender, RequestLineEventArgs e)
        {
            _message = _msgFactory.CreateRequest(e.Method, e.UriPath, e.Version);
        }

        private void OnMessageComplete(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        private void OnHeader(object sender, HeaderEventArgs e)
        {
            IHeader header = _factory.Parse(e.Name, e.Value);
            _message.Assign(header.Name.ToLower(), header);
        }

        /// <summary>
        /// Will continue the parsing until nothing more can be parsed.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Parse(byte[] buffer, int offset, int length)
        {
            int lastOffset = _parser.Parse(buffer, offset, length);
            int currentOffset = _parser.Parse(buffer, lastOffset, length);

            while (lastOffset != currentOffset)
            {
                lastOffset = currentOffset;
                currentOffset = _parser.Parse(buffer, lastOffset, length);
            }

            return currentOffset;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            
        }

        public event EventHandler<RequestEventArgs> RequestCompleted = delegate { };
        public event EventHandler<ResponseEventArgs> ResponseCompleted = delegate { };
    }
}
