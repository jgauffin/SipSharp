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
    }

    public class StackRequestEventArgs : EventArgs
    {
        public IRequest Request { get; private set; }

        public StackRequestEventArgs(IRequest request)
        {
            Request = request;
        }
    }

    public delegate void RequestHandler(ISipStack stack, IRequest request);

}
