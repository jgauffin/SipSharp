using System;
using System.Net;

namespace SipSharp
{
    /// <summary>
    /// A response have been received.
    /// </summary>
    public class ResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseEventArgs"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="endPoint">remote end point</param>
        public ResponseEventArgs(IResponse response, EndPoint endPoint)
        {
            Response = response;
            RemoteEndPoint = endPoint;
        }

        /// <summary>
        /// Gets or sets response.
        /// </summary>
        public IResponse Response { get; private set; }

        /// <summary>
        /// End point that the message was received from.
        /// </summary>
        public EndPoint RemoteEndPoint { get; private set; }

    }
}