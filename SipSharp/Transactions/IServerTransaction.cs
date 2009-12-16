namespace SipSharp.Transactions
{
    /// <summary>
    /// A server transaction.
    /// </summary>
    public interface IServerTransaction : ITransaction
    {
        /// <summary>
        /// Send a new response.
        /// </summary>
        /// <param name="response"></param>
        void Send(IResponse response);

        /// <summary>
        /// The request have been retransmitted by the UA.
        /// </summary>
        /// <param name="request"></param>
        void Process(IRequest request);

        /// <summary>
        /// Gets request that created the transaction.
        /// </summary>
        IRequest Request { get; }
    }
}
