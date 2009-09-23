using System;
using System.Threading;

namespace SipSharp.Transactions
{
    internal class ServerNonInviteTransaction : IServerTransaction
    {
        private readonly IRequest _request;
        private readonly Timer _timerJ;
        private readonly ISipStack _sipStack;
        private IResponse _response;

        public ServerNonInviteTransaction(ISipStack sipStack, IRequest request)
        {
            _timerJ = new Timer(OnTerminate);

            _sipStack = sipStack;
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
                _sipStack.Send(_response);
            }
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
                    _sipStack.Send(response);
                else
                {
                    State = TransactionState.Completed;
                    if (_request.Via.First.Protocol == "UDP")
                        _timerJ.Change(TransactionManager.T1*64, Timeout.Infinite);
                    else
                        _timerJ.Change(0, Timeout.Infinite);
                }
            }
            _sipStack.Send(response);
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
            get { throw new NotImplementedException(); }
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