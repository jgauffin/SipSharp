using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Something have happened with a transaction
    /// </summary>
    public class TransactionEventArgs : EventArgs
    {
        public ITransaction Transaction { get; private set; }

        public TransactionEventArgs(ITransaction transaction)
        {
            Transaction = transaction;
        }
    }
}
