using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    }
}
