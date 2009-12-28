using System;
using System.Threading;
using SipSharp.Messages;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    internal class ServerNonInviteTransaction : IServerTransaction
    {
        private readonly IRequest _request;
        private readonly Timer _timerJ;
        private readonly ITransportLayer _transport;
        private IResponse _response;

        public ServerNonInviteTransaction(ITransportLayer transport, IRequest request)
        {
            _timerJ = new Timer(OnTerminate);

            _transport = transport;
            _request = request;
            State = TransactionState.Trying;
        }

        private void OnTerminate(object state)
        {
            State = TransactionState.Terminated;
            Terminated(this, EventArgs.Empty);
        }

        #region IServerTransaction Members

        public void Process(IRequest request)
        {
            // Once in the "Trying" state, any further request
            // retransmissions are discarded.
            if (State == TransactionState.Trying)
                return;

            if (State == TransactionState.Proceeding)
            {
                _transport.Send(_response);
            }
        }

        /// <summary>
        /// Gets request that created the transaction.
        /// </summary>
        public IRequest Request
        {
            get { return _request; }
        }

        public void Send(IResponse response)
        {
            // Any other final responses passed by the TU to the server
            // transaction MUST be discarded while in the "Completed" state
            if (!StatusCodeHelper.Is1xx(response) && State == TransactionState.Completed)
                return;
            if (State == TransactionState.Terminated)
                return;

            _response = response;
            if (State == TransactionState.Trying)
            {
                if (StatusCodeHelper.Is1xx(response))
                {
                    State = TransactionState.Proceeding;
                }
            }
            else if (State == TransactionState.Proceeding)
            {
                if (StatusCodeHelper.Is1xx(response))
                    _transport.Send(response);
                else
                {
                    State = TransactionState.Completed;
                    if (!_request.IsReliableProtocol)
                        _timerJ.Change(TransactionManager.T1*64, Timeout.Infinite);
                    else
                        _timerJ.Change(0, Timeout.Infinite);
                }
            }
            _transport.Send(response);
        }

        #endregion

        /// <summary>
        /// Gets dialog that the transaction belongs to
        /// </summary>
        //IDialog Dialog { get; }

        /// <summary>
        /// Gets transaction identifier.
        /// </summary>
        public string Id
        {
            get
            {
                return _request.GetTransactionId();
            }
        }

        /// <summary>
        /// Gets dialog that the transaction belongs to
        /// </summary>
        //IDialog Dialog { get; }
        /// <summary>
        /// Gets current transaction state
        /// </summary>
        public TransactionState State { get; private set; }

        public event EventHandler Terminated;
    }
}