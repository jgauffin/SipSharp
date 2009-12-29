namespace SipSharp.Transactions
{
    /// <summary>
    /// A server transaction.
    /// </summary>
    public interface IServerTransaction : ITransaction
    {
        /// <summary>
        /// Gets request that created the transaction.
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// The request have been retransmitted by the UA.
        /// </summary>
        /// <param name="request"></param>
        void Process(IRequest request);

        /// <summary>
        /// Send a new response.
        /// </summary>
        /// <param name="response"></param>
        void Send(IResponse response);
    }
}