using System;
using System.Net;
using SipSharp.Messages;
using SipSharp.Servers;
using SipSharp.Transactions;

namespace SipSharp
{
    /// <summary>
    /// Takes care of all messages.
    /// </summary>
    public interface ISipStack
    {
        Authenticator Authenticator { get; }
        IClientTransaction CreateClientTransaction(IRequest request);

        /// <summary>
        /// Create a new request
        /// </summary>
        /// <param name="method">Sip method.</param>
        /// <param name="from">Who is dialing?</param>
        /// <param name="to">Destination</param>
        /// <returns>Request object.</returns>
        /// <seealso cref="SipMethod"/>
        IRequest CreateRequest(string method, Contact from, Contact to);

        IServerTransaction CreateServerTransaction(IRequest request);

        /// <summary>
        /// Send a request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A created server transaction if request was sent successfully; otherwise <c>null</c>.</returns>
        IClientTransaction Send(IRequest request);

        /// <summary>
        /// A request have been received by the stack
        /// </summary>
        /// <remarks>
        /// <para>
        /// Requests handled by transactions or dialogs are not being handled by this event.
        /// </para>
        /// A new transaction are automatically created by the stack and attached to this event.
        /// </remarks>
        event EventHandler<RequestEventArgs> RequestReceived;


        /// <summary>
        /// A response have been received by the stack
        /// </summary>
        /// <remarks>
        /// <para>
        /// Responses handled by transactions or dialogs are not being handled by this event.
        /// </para>
        /// A new transaction are automatically created by the stack and attached to this event.
        /// </remarks>
        event EventHandler<ResponseEventArgs> ResponseReceived;
    }

    public class RequestEventArgs : EventArgs
    {
        public RequestEventArgs(IRequest request, IServerTransaction transaction, EndPoint endPoint)
        {
            Request = request;
            Transaction = transaction;
            RemoteEndPoint = endPoint;
        }


        /// <summary>
        /// Request was handled by a handler.
        /// </summary>
        public bool IsHandled { get; set; }

        public EndPoint RemoteEndPoint { get; private set; }
        public IRequest Request { get; private set; }
        public IServerTransaction Transaction { get; private set; }
    }

    public class ResponseEventArgs : EventArgs
    {
        public ResponseEventArgs(IResponse response, IServerTransaction transaction, EndPoint remoteEndPoint)
        {
            Response = response;
            Transaction = transaction;
            RemoteEndPoint = remoteEndPoint;
        }

        public ResponseEventArgs(IResponse response, EndPoint remoteEndPoint)
        {
            Response = response;
            RemoteEndPoint = remoteEndPoint;
        }

        /// <summary>
        /// Request was handled by a handler.
        /// </summary>
        public bool IsHandled { get; set; }

        public EndPoint RemoteEndPoint { get; private set; }
        public IResponse Response { get; private set; }
        public IServerTransaction Transaction { get; private set; }
    }
}