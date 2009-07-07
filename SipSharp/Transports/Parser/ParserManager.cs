using System;
using System.Collections.Generic;
using System.Net;

namespace SipSharp.Transports.Parser
{
    class ParserManager
    {
        private Queue<IMessageParser> _parsers = new Queue<IMessageParser>();
        private bool _isRequest;
        private readonly ILogWriter _logWriter = LogFactory.CreateLogger(typeof(ParserManager));

        /// <summary>
        /// Used to keep track of messages during parsing.
        /// </summary>
        /// <remarks>
        /// Is assigned to parser tags.
        /// </remarks>
        private class ParserContext
        {
            public IMessage Message { get; set; }
            public EndPoint EndPoint { get; set; }
        }

        /// <summary>
        /// Dequeue or create a new parser.
        /// </summary>
        /// <returns>Created parser.</returns>
        /// <remarks>
        /// All events will be hooked by this parser pool.
        /// </remarks>
        public IMessageParser Dequeue()
        {
            lock (_parsers)
            {
                if (_parsers.Count > 0)
                    return _parsers.Dequeue();
            }

            IMessageParser parser = Create();
            parser.ResponseLineReceived += OnResponseLine;
            parser.RequestLineReceived += OnRequestLine;
            parser.HeaderReceived += OnHeader;
            parser.BodyBytesReceived += OnBodyBytes;
            parser.MessageCompleted += OnMessageCompleted;
            return parser;
        }

        /// <summary>
        /// Dequeue a parser and setup it's tag.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        /// <remarks>
        /// The 
        /// </remarks>
        private IMessageParser Dequeue(EndPoint endPoint)
        {
            IMessageParser parser = Dequeue();
            ParserContext context = new ParserContext();
            context.EndPoint = endPoint;
            parser.Tag = context;
            return parser;
        }


        private void OnMessageCompleted(object sender, EventArgs e)
        {
            IMessageParser parser = (IMessageParser) sender;
            ParserContext context = (ParserContext)parser.Tag;

            if (_isRequest)
                RequestReceived(this, new RequestEventArgs((IRequest)context.Message, context.EndPoint));
            else
                ResponseReceived(this, new ResponseEventArgs((IResponse)context.Message, context.EndPoint));
        }


        private void OnBodyBytes(object sender, BodyEventArgs e)
        {
            IMessageParser parser = (IMessageParser)sender;
            ParserContext context = (ParserContext)parser.Tag;
            context.Message.Body.Write(e.Buffer, e.Offset, e.Count);
        }

        /// <summary>
        /// Called when a new header have been parsed.
        /// </summary>
        /// <param name="sender">Parser.</param>
        /// <param name="e">event arguments</param>
        /// <exception cref="BadRequestException">Content-length is not a number.</exception>
        private void OnHeader(object sender, HeaderEventArgs e)
        {
            IMessageParser parser = (IMessageParser)sender;
            ParserContext context = (ParserContext)parser.Tag;
            _logWriter.Write(this, LogLevel.Trace, "Header '" + e.Name + "' = " + e.Value);
            context.Message.Headers.Add(e.Name, e.Value);
        }

        private void OnRequestLine(object sender, RequestLineEventArgs e)
        {
            IMessageParser parser = (IMessageParser)sender;
            ParserContext context = (ParserContext)parser.Tag;

            _isRequest = true;
            _logWriter.Write(this, LogLevel.Trace,
                             "Request '" + e.Method + "' " + e.UriPath + " (" + e.Version + ")");
            context.Message = parser.CreateRequest(e.Method, e.UriPath, e.Version);
        }

        private void OnResponseLine(object sender, ResponseLineEventArgs e)
        {
            IMessageParser parser = (IMessageParser)sender;
            ParserContext context = (ParserContext)parser.Tag;

            _isRequest = false;
            _logWriter.Write(this, LogLevel.Trace,
                             "Response " + e.StatusCode + " '" + e.ReasonPhrase + "' (" + e.Version + ")");
            context.Message = parser.CreateResponse(e.Version, e.StatusCode, e.ReasonPhrase);
        }

        /*
        public void ParseMessage(IPEndPoint endPoint, byte[] bytes, int offset, int count)
        {
            IMessageParser parser = Dequeue(endPoint);
            parser.Parse(bytes, offset, count);
            Enqueue(parser);
        }
        */

        
        /// <summary>
        /// Parse incoming bytes.
        /// </summary>
        /// <param name="endPoint">Endpoint that we are parsing a message for.</param>
        /// <param name="buffer">buffer to parse</param>
        /// <param name="offset">where to start reading from the buffer.</param>
        /// <param name="count">number of bytes in the buffer.</param>
        /// <returns>offset in array to where the next read should start.</returns>
        /// <remarks>
        /// Will copy the remaining bytes to the beginning of the array.
        /// </remarks>
        public int Parse(EndPoint endPoint, byte[] buffer, int offset, int count)
        {
            IMessageParser parser = Dequeue(endPoint);

            int bytesParsed = parser.Parse(buffer, offset, count);
            while (bytesParsed > 0)
            {
                offset += bytesParsed;
                count -= bytesParsed;
                bytesParsed = parser.Parse(buffer, offset, count);
            }

            // Move the rest of the bytes to the beginning of the array
            if (count > 0)
                Buffer.BlockCopy(buffer, offset, buffer, 0, count);

            Enqueue(parser);
            return count;
        }
        
        public void Enqueue(IMessageParser parser)
        {
            parser.Clear();
            lock (_parsers)
                _parsers.Enqueue(parser);
        }

        protected virtual IMessageParser Create()
        {
            return new SipParser();
        }

        /// <summary>
        /// A request have been received.
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestReceived = delegate { };
        /// <summary>
        /// A response have been received from a remote end point.
        /// </summary>
        public event EventHandler<ResponseEventArgs> ResponseReceived = delegate { };
    }
}
