using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Transaction states.
    /// </summary>
    /// <remarks>
    /// These states are defined in RFC3261.
    /// </remarks>
    public enum TransactionState
    {
        /// <summary>
        /// Initial state for INVITe transactions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When in either the "Calling" or "Proceeding" states, reception of a
        /// response with status code from 300-699 MUST cause the client
        /// transaction to transition to "Completed".  
        /// </para>
        /// <para>
        /// State should be switched to Proceeding if a provisional response
        /// is received.
        /// </para>
        /// </remarks>
        Calling,

        /// <summary>
        /// This is transaction initial state. Used only in Non-INVITE transaction.
        /// </summary>
        Trying,

        /// <summary>
        /// Second INVITE state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the client transaction receives a provisional response while in
        /// the "Calling" state, it transitions to the "Proceeding" state
        /// </para>
        /// <para>
        /// In the "Proceeding" state, the client transaction SHOULD NOT retransmit the
        /// request any longer.
        /// </para>
        /// <para>
        /// When in either the "Calling" or "Proceeding" states, reception of a
        /// response with status code from 300-699 MUST cause the client
        /// transaction to transition to "Completed".  
        /// </para>
        /// </remarks>
        Proceeding,

        /// <summary>
        /// Transaction has got final response.
        /// </summary>
        /// <remarks>
        /// The "Completed" state exists to buffer any additional response retransmissions
        /// that may be received (which is why the client transaction remains there only
        /// for unreliable transports)
        /// </remarks>
        Completed,

        /// <summary>
        /// Transation has got ACK from request maker. This is used only by INVITE server transaction.
        /// </summary>
        Confirmed,

        /// <summary>
        /// Transaction has terminated and waits disposing.
        /// </summary>
        Terminated,
    }
}
