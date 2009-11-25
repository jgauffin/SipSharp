using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Transactions;

namespace SipSharp
{
    /// <summary>
    /// Takes care of all messages.
    /// </summary>
    public interface ISipStack
    {
        void Send(IRequest request);
        void Send(IResponse response);

        event EventHandler DialogCreated;
        event EventHandler DialogTerminated;

        void RegisterMethod(string methodName, EventHandler<StackRequestEventArgs> handler);
        IServerTransaction CreateServerTransaction(IRequest request);
        IClientTransaction CreateClientTransaction(IRequest request);
    }

    public class StackRequestEventArgs : EventArgs
    {
        public IRequest Request { get; private set; }
        public IServerTransaction Transaction { get; private set; }

        public StackRequestEventArgs(IRequest request, IServerTransaction transaction)
        {
            Request = request;
            Transaction = transaction;
        }


        /// <summary>
        /// Request was handled by a invoker.
        /// </summary>
        public bool IsHandled { get; set; }
    }

    public delegate void RequestHandler(ISipStack stack, IRequest request);

}
