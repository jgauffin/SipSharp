using System;
using SipSharp.Tools;
using SipSharp.Transports.Parser;

namespace SipSharp.Parser
{
    public class SipParser
    {
        private readonly BodyEventArgs _bodyEventArgs = new BodyEventArgs();
        private readonly HeaderEventArgs _headerEventArgs = new HeaderEventArgs();
        private readonly BufferReader _reader = new BufferReader();
        private readonly RequestLineEventArgs _requestEventArgs = new RequestLineEventArgs();
        private readonly ResponseLineEventArgs _responseEventArgs = new ResponseLineEventArgs();

        private int _bodyBytesLeft;
        private byte[] _buffer;
        private string _headerName;
        private ParserMethod _parserMethod;
        private string _headerValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SipParser"/> class.
        /// </summary>
        public SipParser()
        {
            _parserMethod = ParseFirstLine;
        }


        /// <summary>
        /// Gets or sets current line number.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Parser method to copy all body bytes.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Needed since a TCP packet can contain multiple messages after each other, or partial messages.</remarks>
        private bool GetBody()
        {
            // Got enough bytes to complete body.
            if (_reader.RemainingLength >= _bodyBytesLeft)
            {
                OnBodyBytes(_buffer, _reader.Index, _bodyBytesLeft);
                _reader.Index += _bodyBytesLeft;
                _bodyBytesLeft = 0;
                OnComplete();
                return false;
            }

            OnBodyBytes(_buffer, _reader.Current, _reader.RemainingLength);
            _bodyBytesLeft -= _reader.RemainingLength;
            _reader.Index = _reader.Length - 1;
            return _reader.Index != _reader.Length;
        }

        /// <summary>
        /// Try to find a header name.
        /// </summary>
        /// <returns></returns>
        private bool GetHeaderName()
        {
            // empty line. body is begining.
            if (_reader.Current == '\r' && _reader.Peek == '\n')
            {
                // Eat the line break
                _reader.Consume('\r', '\n');

                // Don't have a body?
                if (_bodyBytesLeft == 0)
                {
                    OnComplete();
                    _parserMethod = ParseFirstLine;
                }
                else
                    _parserMethod = GetBody;

                return true;
            }

            _headerName = _reader.ReadUntil(':');
            if (_headerName == null)
                return false;

            _reader.Consume(); // eat colon
            _parserMethod = GetHeaderValue;
            return true;
        }

        /// <summary>
        /// Get header values.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Will also look for multi header values and automatically merge them to one line.</remarks>
        private bool GetHeaderValue()
        {
            // remove white spaces.
            _reader.Consume(' ', '\t');

            // multi line or empty value?
            if (_reader.Current == '\r' && _reader.Peek == '\n')
            {
                _reader.Consume('\r', '\n');

                // empty value.
                if (_reader.Current != '\t' && _reader.Current != ' ')
                {
                    OnHeader(_headerName, string.Empty);
                    _headerName = null;
                    _headerValue = string.Empty;
                    _parserMethod = GetHeaderName;
                    return true;
                }

                if(_reader.RemainingLength < 1)
                    return false;

                // consume one whitespace
                _reader.Consume();

                // and fetch the rest.
                return GetHeaderValue();
            }

            string value = _reader.ReadLine();
            if (value == null)
                return false;

            _headerValue += value;
            if (string.Compare(_headerName, "Content-Length", true) == 0)
            {
                if (!int.TryParse(value, out _bodyBytesLeft))
                    throw new ParserException("Content length is not a number.");
            }

            OnHeader(_headerName, value);
            _headerName = null;
            _headerValue = string.Empty;
            _parserMethod = GetHeaderName;
            return true;
        }

        /// <summary>
        /// Toggle body bytes event.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        protected virtual void OnBodyBytes(byte[] bytes, int offset, int count)
        {
            _bodyEventArgs.AssignInternal(bytes, offset, count);
            BodyBytesReceived(this, _bodyEventArgs);
        }

        /// <summary>
        /// Raise the MessageComplete event, since we have successfully parsed a message and it's body.
        /// </summary>
        protected virtual void OnComplete()
        {
            MessageComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// First message line.
        /// </summary>
        /// <param name="words">Will always contain three elements.</param>
        /// <remarks>Used to raise the RequestLineParsed or ResponseLineParsed event 
        /// depending on the words in the array.</remarks>
        protected virtual void OnFirstLine(string[] words)
        {
            if (words[0] == "SIP/2.0")
            {
                _responseEventArgs.Version = words[0];
                try
                {
                    _responseEventArgs.StatusCode = (StatusCode) Enum.Parse(typeof (StatusCode), words[1]);
                }
                catch (ArgumentException err)
                {
                    throw new BadRequestException("Status code '" + words[1] + "' is not known.", err);
                }
                _responseEventArgs.ReasonPhrase = words[1];
                ResponseLineParsed(this, _responseEventArgs);
            }
            else
            {
                _requestEventArgs.Method = words[0];
                _requestEventArgs.UriPath = words[1];
                _requestEventArgs.Version = words[2];
                RequestLineParsed(this, _requestEventArgs);
            }
        }

        private void OnHeader(string name, string value)
        {
            _headerEventArgs.Name = name;
            _headerEventArgs.Value = value;
            HeaderParsed(this, _headerEventArgs);
        }

        /// <summary>
        /// Continue parsing a message
        /// </summary>
        /// <param name="buffer">Byte buffer containting bytes</param>
        /// <param name="offset">Where to start the parsing</param>
        /// <param name="count">Number of bytes to parse</param>
        /// <returns>index where the parsing stopped.</returns>
        /// <exception cref="ParserException">Parsing failed.</exception>
        public int Parse(byte[] buffer, int offset, int count)
        {
            _buffer = buffer;
            _reader.Assign(buffer, offset, count);
            while (_parserMethod()) ;
            return _reader.Index;
        }

        public bool ParseFirstLine()
        {
            _reader.Consume('\r', '\n');

            // Do not contain a complete first line.
            if (!_reader.Contains('\n'))
                return false;

            var words = new string[3];
            words[0] = _reader.ReadUntil(' ');
            _reader.Consume(); // eat delimiter
            words[1] = _reader.ReadUntil(' ');
            _reader.Consume(); // eat delimiter
            words[2] = _reader.ReadLine();
            if (string.IsNullOrEmpty(words[0])
                || string.IsNullOrEmpty(words[1])
                || string.IsNullOrEmpty(words[2]))
                    throw new BadRequestException("Invalid request/response line.");

            OnFirstLine(words);
            _parserMethod = GetHeaderName;
            return true;
        }

        public event EventHandler<RequestLineEventArgs> RequestLineParsed = delegate { };
        public event EventHandler<ResponseLineEventArgs> ResponseLineParsed = delegate { };
        public event EventHandler<HeaderEventArgs> HeaderParsed = delegate { };
        public event EventHandler<BodyEventArgs> BodyBytesReceived = delegate { };
        public event EventHandler MessageComplete = delegate { };

        #region Nested type: ParserMethod

        /// <summary>
        /// Used to be able to quickly swap parser method.
        /// </summary>
        /// <returns></returns>
        private delegate bool ParserMethod();

        #endregion
    }
}