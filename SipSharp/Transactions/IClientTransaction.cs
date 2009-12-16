using System;
using System.Net;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Used to make sure that requests will receive respones
    /// properly.
    /// </summary>
	public interface IClientTransaction : ITransaction
	{
        /// <summary>
        /// Gets request that the transaction is for.
        /// </summary>
        IRequest Request { get; }


        /// <summary>
        /// Process a response to the request.
        /// </summary>
        /// <param name="response">Received response</param>
        /// <param name="endPoint">End point that the response was received from.</param>
        /// <returns><c>true</c> if response was handled; otherwise <c>false</c>.</returns>
        bool Process(IResponse response, EndPoint endPoint);

        /// <summary>
        /// Invoked if transportation failed.
        /// </summary>
        event EventHandler TransportFailed;

        /// <summary>
        /// A timeout have occurred.
        /// </summary>
        event EventHandler TimedOut;

        /// <summary>
        /// A response have been received.
        /// </summary>
        event EventHandler<ResponseEventArgs> ResponseReceived;

        /// <summary>
        /// We like to reuse transaction objects. Remove all references
        /// to the transaction and reset all parameters.
        /// </summary>
	    void Cleanup();
	}
}
