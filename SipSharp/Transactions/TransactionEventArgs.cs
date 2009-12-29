using System;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Something have happened with a transaction
    /// </summary>
    public class TransactionEventArgs : EventArgs
    {
        public TransactionEventArgs(ITransaction transaction)
        {
            Transaction = transaction;
        }

        public ITransaction Transaction { get; private set; }
    }
}