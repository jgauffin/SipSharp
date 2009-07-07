using System;
using System.Net;

namespace SipSharp.Transports.Parser
{
    /// <summary>
    /// Event driven parser used to parse incoming HTTP requests.
    /// </summary>
    /// <remarks>
    /// The parser supports partial messages and keeps the states between
    /// each parsed buffer. It's therefore important that the parser gets
    /// <see cref="Clear"/>ed if a client disconnects.
    /// </remarks>
    public interface IMessageParser
    {
        /// <summary>
        /// Current state in parser.
        /// </summary>
//        RequestParserState CurrentState { get; }

        /// <summary>
        /// Parses the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer containing bytes to parse.</param>
        /// <param name="offset">where to start in the buffer.</param>
        /// <param name="count">number of bytes, from offset, that can be parsed.</param>
        /// <returns>Number of bytes that was parsed.</returns>
        /// <exception cref="BadRequestException"><c>BadRequestException</c>.</exception>
        int Parse(byte[] buffer, int offset, int count);

        /// <summary>
        /// A request have been successfully parsed.
        /// </summary>
        event EventHandler MessageCompleted;

        /// <summary>
        /// More body bytes have been received.
        /// </summary>
        event EventHandler<BodyEventArgs> BodyBytesReceived;

        /// <summary>
        /// Request line have been received.
        /// </summary>
        event EventHandler<RequestLineEventArgs> RequestLineReceived;

        /// <summary>
        /// Response line have been received.
        /// </summary>
        event EventHandler<ResponseLineEventArgs> ResponseLineReceived;

        /// <summary>
        /// A header have been received.
        /// </summary>
        event EventHandler<HeaderEventArgs> HeaderReceived;

        /// <summary>
        /// Gets or sets a optional object that identifies what this parser belongs to.
        /// </summary>
        /// <remarks>
        /// Compare with Tag in WinForms controls or State object in asynchronous methods.
        /// </remarks>
        object Tag { get; set; }

        /// <summary>
        /// Clear parser state.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets or sets the log writer.
        /// </summary>
        ILogWriter LogWriter { get; set; }

        /// <summary>
        /// Create a new request.
        /// </summary>
        /// <returns>A new <see cref="IRequest"/>.</returns>
        IRequest CreateRequest(string method, string path, string version);

        /// <summary>
        /// Create a new response object.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="code"></param>
        /// <param name="phrase"></param>
        /// <returns></returns>
        IResponse CreateResponse(string version, StatusCode code, string phrase);
    }

    /// <summary>
    /// Current state in the parsing.
    /// </summary>
    public enum RequestParserState
    {
        /// <summary>
        /// Should parse the request line
        /// </summary>
        FirstLine,
        /// <summary>
        /// Searching for a complete header name
        /// </summary>
        HeaderName,
        /// <summary>
        /// Searching for colon after header name (ignoring white spaces)
        /// </summary>
        AfterName,
        /// <summary>
        /// Searching for start of header value (ignoring white spaces)
        /// </summary>
        Between,
        /// <summary>
        /// Searching for a complete header value (can span over multiple lines, as long as they are prefixed with one/more whitespaces)
        /// </summary>
        HeaderValue,

        /// <summary>
        /// Adding bytes to body
        /// </summary>
        Body
    }
}