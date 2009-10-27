using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages;

namespace SipSharp.Transactions
{
    public static class ExtensionMethods
    {
        public static string GetTransactionId(this IRequest request)
        {
            /*The request matches a transaction if:

  1. the branch parameter in the request is equal to the one in the
     top Via header field of the request that created the
     transaction, and

  2. the sent-by value in the top Via of the request is equal to the
     one in the request that created the transaction, and

  3. the method of the request matches the one that created the
     transaction, except for ACK, where the method of the request
     that created the transaction is INVITE.
 */
            string method = request.Method;
            if (method == SipMethod.ACK)
                method = SipMethod.INVITE;

            return request.Via.First.Branch + "|" + request.Via.First.SentBy + "|" + method;
        }

    }
}
