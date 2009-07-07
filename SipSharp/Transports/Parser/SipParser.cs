using System;
using System.Text;

namespace SipSharp.Transports.Parser
{
    /// <summary>
    /// Our request parser.
    /// </summary>
    public class SipParser : IMessageParser
    {
        private readonly BodyEventArgs _bodyArgs = new BodyEventArgs();
        private readonly HeaderEventArgs _headerArgs = new HeaderEventArgs();
        private readonly RequestLineEventArgs _requestLineArgs = new RequestLineEventArgs();
        private readonly ResponseLineEventArgs _responseLineEventArgs = new ResponseLineEventArgs();
        private byte[] _buffer;
        private char _char;
        private int _end;
        private int _lineNumber;
        private ILogWriter _log = NullLogWriter.Instance;
        private char _nextChar;
        private int _pos;
        private int _startPos;
        private ParserStateHandler _stateMethod;
        private int _bodyBytesLeft = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SipParser"/> class.
        /// </summary>
        public SipParser()
        {
            _stateMethod = ParseFirstLine;
        }


        /// <summary>
        /// We've got a complete header name, lets 
        /// skip all white spaces before colon.
        /// </summary>
        /// <returns>What to do when this method is done.</returns>
        private HandlerState FindColon()
        {
            if (_char == ':')
            {
                _stateMethod = FindStartOfValue;
                return HandlerState.Ok;
            }

            return HandlerState.Continue;
        }

        /// <summary>
        /// Log for end of header name.
        /// </summary>
        /// <returns></returns>
        private HandlerState FindEndOfName()
        {
            if (char.IsWhiteSpace(_char) || _char == ':')
            {
                _headerArgs.Name = Encoding.UTF8.GetString(_buffer, _startPos, _pos - _startPos);
                _stateMethod = FindColon;
                _startPos = -1;
                _log.Write(this, LogLevel.Trace, "Found header name: " + _headerArgs.Name);

                return _char == ':' ? HandlerState.Restart : HandlerState.Ok;
            }

            return HandlerState.Continue;
        }

        private HandlerState FindEndOfParameterName()
        {
            return HandlerState.Continue;
        }

        /// <summary>
        /// End of header value (end can be new line, or semicolon or white space)
        /// </summary>
        /// <returns></returns>
        private HandlerState FindEndOfValue()
        {
            if (char.IsWhiteSpace(_char))
            {
                // Ignore all whitespaces which are not new line
                if (_char != '\r' && _nextChar != '\n')
                    return HandlerState.Continue;

                if (_headerArgs.Value == null)
                {
                    _headerArgs.Value = Encoding.UTF8.GetString(_buffer, _startPos, _pos - _startPos);
                    _log.Write(this, LogLevel.Trace, "Found header value (may be partial): " + _headerArgs.Value);
                }
                else
                {
                    _headerArgs.Value += Encoding.UTF8.GetString(_buffer, _startPos, _pos - _startPos);
                    _log.Write(this, LogLevel.Trace, "Appended data to header value: " + _headerArgs.Value);
                }

                // check if it's a multiple line header.
                if (_pos + 3 < _end)
                {
                    if (_buffer[_pos + 2] == '\t' || _buffer[_pos + 2] == ' ')
                    {
                        // yes. It's a header spanning over multiple lines.
                        _pos += 2;
                        _startPos = _pos + 1;
                        _headerArgs.Value += ' '; // \r\n and space/tab corresponds to a single space.
                        return HandlerState.Restart;
                    }
                }

                _stateMethod = FindStartOfName;
                if (_headerArgs.Name == "Content-Length")
                {
                    if (!int.TryParse(_headerArgs.Value, out _bodyBytesLeft))
                        ThrowException("Content length is not a number.");
                }

                HeaderReceived(this, _headerArgs);
                _headerArgs.Value = null;
                _log.Write(this, LogLevel.Trace, "Header is completed.");
                return HandlerState.Continue;
            }

            /*if (_char == ';')
            {
                _stateMethod = FindEndOfParameterName;
                return HandlerState.Continue;
            }*/
            return HandlerState.Continue;
        }

        /// <summary>
        /// Look for beginning of header name
        /// </summary>
        /// <returns></returns>
        private HandlerState FindStartOfName()
        {
            // Body delimiter.
            if (_char == '\r' && _nextChar == '\n')
            {
                _stateMethod = ProcessBody;
                _startPos = -1;
                ++_pos;
                return HandlerState.Ok;
            }
            if (char.IsLetterOrDigit(_char))
            {
                _startPos = _pos;
                _stateMethod = FindEndOfName;
                return HandlerState.Ok;
            }

            return HandlerState.Continue;
        }

        private HandlerState ProcessBody()
        {
            _bodyArgs.Buffer = _buffer;
            _bodyArgs.Offset = _pos;

            // We need less bytes than that's left in the inbuffer.
            if (_bodyBytesLeft <= _end - _pos)
            {
                _bodyArgs.Count = _bodyBytesLeft;
                BodyBytesReceived(this, _bodyArgs);
                _bodyBytesLeft = 0;
                _pos += _bodyBytesLeft;
                return HandlerState.Restart;
            }

            // we have not enought bytes in the buffer, use all
            _bodyArgs.Count = _end - _pos;
            BodyBytesReceived(this, _bodyArgs);
            _bodyBytesLeft = _end - _pos;
            _pos = _end;
            return HandlerState.Restart;
        }

        /// <summary>
        /// Have got colon, ignore all white spaces.
        /// </summary>
        /// <returns></returns>
        private HandlerState FindStartOfValue()
        {
            if (char.IsWhiteSpace(_char))
                return HandlerState.Continue;

            _startPos = _pos;
            _stateMethod = FindEndOfValue;
            return HandlerState.Ok;
        }

        /// <summary>
        /// Parse request line
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="BadRequestException">If line is incorrect</exception>
        /// <remarks>Expects the following format: "Method SP Request-URI SP HTTP-Version CRLF"</remarks>
        protected void OnFirstLine(string value)
        {
            _log.Write(this, LogLevel.Debug, "Got first line: " + value);
            if (string.Compare(value.Substring(0, 4), "SIP/", true) != 0)
                OnRequestLine(value);
            else
                OnResponseLine(value);
        }

        /// <summary>
        /// Parses first line in a request message
        /// </summary>
        /// <param name="value">First line in a message</param>
        /// <exception cref="BadRequestException">If first line cannot be parsed successfully.</exception>
        private void OnRequestLine(string value)
        {
            //Request-Line   = 	Method   SP   Request-URI   SP   SIP-Version   CRLF 
            int pos = value.IndexOf(' ');
            if (pos == -1 || pos + 1 >= value.Length)
            {
                _log.Write(this, LogLevel.Warning, "Invalid request line, missing Method. Line: " + value);
                throw new BadRequestException("Invalid request line, missing Method. Line: " + value);
            }

            string method = value.Substring(0, pos).ToUpper();
            int oldPos = pos + 1;
            pos = value.IndexOf(' ', oldPos);
            if (pos == -1)
            {
                _log.Write(this, LogLevel.Warning, "Invalid request line, missing URI. Line: " + value);
                throw new BadRequestException("Invalid request line, missing URI. Line: " + value);
            }
            string path = value.Substring(oldPos, pos - oldPos);
            if (path.Length > 4196)
                throw new BadRequestException("Too long URI.");

            if (pos + 1 >= value.Length)
            {
                _log.Write(this, LogLevel.Warning, "Invalid request line, missing SIP-Version. Line: " + value);
                throw new BadRequestException("Invalid request line, missing SIP-Version. Line: " + value);
            }
            string version = value.Substring(pos + 1);
            if (version.Length < 4 || string.Compare(version.Substring(0, 3), "SIP", true) != 0)
            {
                _log.Write(this, LogLevel.Warning, "Invalid SIP version in request line. Line: " + value);
                throw new BadRequestException("Invalid SIP version in Request line. Line: " + value);
            }

            _requestLineArgs.Method = method;
            _requestLineArgs.Version = version;
            _requestLineArgs.UriPath = path;
            RequestLineReceived(this, _requestLineArgs);
        }

        /// <summary>
        /// Parses first line in a response message
        /// </summary>
        /// <param name="value">First line in a message</param>
        /// <exception cref="BadRequestException">If first line cannot be parsed successfully.</exception>
        private void OnResponseLine(string value)
        {
            //Response-Line   = SIP-Version   SP   Status-Code   SP   Reason-Phrase   CRLF 
            int pos = value.IndexOf(' ');
            if (pos == -1 || pos + 1 >= value.Length)
            {
                _log.Write(this, LogLevel.Warning, "Invalid request line, missing Method. Line: " + value);
                throw new BadRequestException("Invalid request line, missing Method. Line: " + value);
            }

            string version = value.Substring(0, pos).ToUpper();
            if (version.Length < 4 || string.Compare(version.Substring(0, 3), "SIP", true) != 0)
            {
                _log.Write(this, LogLevel.Warning, "Invalid SIP version in response line. Line: " + value);
                throw new BadRequestException("Invalid SIP version in response line. Line: " + value);
            }

            int oldPos = pos + 1;
            pos = value.IndexOf(' ', oldPos);
            if (pos == -1)
            {
                _log.Write(this, LogLevel.Warning, "Invalid response line, missing version. Line: " + value);
                throw new BadRequestException("Invalid response line, missing version. Line: " + value);
            }
            string statusCodeStr = value.Substring(oldPos, pos - oldPos);
            int code;
            if (!int.TryParse(statusCodeStr, out code) || !Enum.IsDefined(typeof (StatusCode), code))
            {
                _log.Write(this, LogLevel.Warning,
                           "Invalid response line, status code is not a valid number. Line: " + value);
                throw new BadRequestException("Invalid response line, status code is not a valid number. Line: " + value);
            }
            var statusCode = (StatusCode) code;

            if (pos + 1 >= value.Length)
            {
                _log.Write(this, LogLevel.Warning, "Invalid request line, missing reason phrase. Line: " + value);
                throw new BadRequestException("Invalid request line, missing reason phrase. Line: " + value);
            }
            string reason = value.Substring(pos + 1);

            _responseLineEventArgs.StatusCode = statusCode;
            _responseLineEventArgs.Version = version;
            _responseLineEventArgs.ReasonPhrase = reason;
            ResponseLineReceived(this, _responseLineEventArgs);
        }

        /// <summary>
        /// Wait for a complete request line
        /// </summary>
        private HandlerState ParseFirstLine()
        {
            if (_startPos == -1)
            {
                if (char.IsLetterOrDigit(_char))
                    _startPos = _pos;

                else if (_char != '\r' || _nextChar != '\n')
                {
                    LogWriter.Write(this, LogLevel.Warning, "Request line is not found.");
                    ThrowException("Invalid request line.");
                }
            }
            else if (_char == '\r')
            {
                if (_nextChar != '\n')
                {
                    LogWriter.Write(this, LogLevel.Warning,
                                    "RFC says that line breaks should be \\r\\n, got only \\n.");
                    ThrowException("Invalid line break on request line, expected CRLF.");
                }

                OnFirstLine(Encoding.UTF8.GetString(_buffer, _startPos, _pos - _startPos));
                _stateMethod = FindStartOfName;
                _startPos = -1;
                return HandlerState.Ok;
            }

            return HandlerState.Continue;
        }

        /// <summary>
        /// Throw exception
        /// </summary>
        /// <param name="msg">Error message</param>
        /// <exception cref="BadRequestException"><c>BadRequestException</c>.</exception>
        private void ThrowException(string msg)
        {
            throw new BadRequestException(msg + "(Line #" + _lineNumber + ").");
        }

        #region IMessageParser Members

        /// <summary>
        /// Clear parser state.
        /// </summary>
        public void Clear()
        {
            _pos = _end = 0;
            _char = _nextChar = char.MinValue;
            _stateMethod = ParseFirstLine;
            _lineNumber = 0;
            _startPos = -1;
        }

        /// <summary>
        /// Gets or sets the log writer.
        /// </summary>
        /// <value></value>
        public ILogWriter LogWriter
        {
            get { return _log; }
            set { _log = value; }
        }

        /// <summary>
        /// Gets or sets a optional object that identifies what this parser belongs to.
        /// </summary>
        /// <remarks>
        /// Compare with Tag in WinForms controls or State object in asynchronous methods.
        /// </remarks>
        public object Tag { get; set; }

        /// <summary>
        /// Create a new request.
        /// </summary>
        /// <returns>A new <see cref="IRequest"/>.</returns>
        public virtual IRequest CreateRequest(string method, string path, string version)
        {
            return new Request(method, path, version);
        }

        /// <summary>
        /// Create a new response object.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="code"></param>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public IResponse CreateResponse(string version, StatusCode code, string phrase)
        {
            return new Response(version, code, phrase);
        }

        /// <summary>
        /// Parses the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer containing bytes to parse.</param>
        /// <param name="offset">where to start in the buffer.</param>
        /// <param name="count">number of bytes, from offset, that can be parsed.</param>
        /// <returns>Number of bytes that was parsed.</returns>
        public int Parse(byte[] buffer, int offset, int count)
        {
            _pos = offset;
            _end = _pos + count;
            _buffer = buffer;

            while (_pos < _end)
            {
                _char = (char) buffer[_pos];
                _nextChar = _end > _pos + 1 ? (char) buffer[_pos + 1] : char.MinValue;
                if (_char == '\r')
                    ++_lineNumber;

                switch (_stateMethod())
                {
                    case HandlerState.Restart:
                        continue;
                    case HandlerState.Continue:
                        break;
                    case HandlerState.Exit:
                        return _pos - offset;
                    case HandlerState.Ok:
                        break;
                }

                ++_pos;
            }

            return _pos - offset;
        }


        /// <summary>
        /// A request have been successfully parsed.
        /// </summary>
        public event EventHandler MessageCompleted = delegate { };

        /// <summary>
        /// More body bytes have been received.
        /// </summary>
        public event EventHandler<BodyEventArgs> BodyBytesReceived = delegate { };

        /// <summary>
        /// Request line have been received.
        /// </summary>
        public event EventHandler<RequestLineEventArgs> RequestLineReceived = delegate { };

        /// <summary>
        /// Response line have been received.
        /// </summary>
        public event EventHandler<ResponseLineEventArgs> ResponseLineReceived = delegate { };

        /// <summary>
        /// A header have been received.
        /// </summary>
        public event EventHandler<HeaderEventArgs> HeaderReceived = delegate { };

        #endregion

        #region Nested type: HandlerState

        private enum HandlerState
        {
            /// <summary>
            /// Do not increase index, run again
            /// with the new state handler.
            /// </summary>
            Restart,

            /// <summary>
            /// Copy the remaning bytes and exit, 
            /// since current state handler needs more bytes.
            /// </summary>
            Exit,

            /// <summary>
            /// Continue running (in the current state)
            /// </summary>
            Continue,

            /// <summary>
            /// Current state is done, reset start pos
            /// </summary>
            Ok
        }

        #endregion

        #region Nested type: ParserStateHandler

        private delegate HandlerState ParserStateHandler();

        #endregion
    }
}