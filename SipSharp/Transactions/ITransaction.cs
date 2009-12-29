using System;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Base interface for transactions
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Gets dialog that the transaction belongs to
        /// </summary>
        //IDialog Dialog { get; }
        /// <summary>
        /// Gets transaction identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets current transaction state
        /// </summary>
        TransactionState State { get; }

        /// <summary>
        /// Gets bransch that identifies that transaction.
        /// </summary>
        //string BranschId { get; }
        /// <summary>
        /// Transaction have been terminated.
        /// </summary>
        event EventHandler Terminated;
    }
}