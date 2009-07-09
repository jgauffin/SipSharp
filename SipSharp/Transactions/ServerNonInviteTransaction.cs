using System.Threading;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    internal class ServerNonInviteTransaction : IServerTransaction
    {
        private readonly IRequest _request;
        private readonly Timer _timerJ;
        private readonly ITransportManager _transport;
        private IResponse _response;
        private TransactionState _state;

        public ServerNonInviteTransaction(ITransportManager transport, IRequest request)
        {
            _timerJ = new Timer(OnTerminate);

            _transport = transport;
            _request = request;
            _state = TransactionState.Trying;
        }

        private void OnTerminate(object state)
        {
            _state = TransactionState.Terminated;
        }

        #region IServerTransaction Members

        public void OnRequest(IRequest request)
        {
            // Once in the "Trying" state, any further request
            // retransmissions are discarded.
            if (_state == TransactionState.Trying)
                return;

            if (_state == TransactionState.Proceeding)
            {
                _transport.Send(_response);
            }
        }

        public void Send(IResponse response)
        {
            // Any other final responses passed by the TU to the server
            // transaction MUST be discarded while in the "Completed" state
            if (!StatusCodeHelper.Is1xx(response) && _state == TransactionState.Completed)
                return;
            if (_state == TransactionState.Terminated)
                return;

            _response = response;
            if (_state == TransactionState.Trying)
            {
                if (StatusCodeHelper.Is1xx(response))
                {
                    _state = TransactionState.Proceeding;
                }
            }
            else if (_state == TransactionState.Proceeding)
            {
                if (StatusCodeHelper.Is1xx(response))
                    _transport.Send(response);
                else
                {
                    _state = TransactionState.Completed;
                    if (_request.Via.First.Protocol == "UDP")
                        _timerJ.Change(TransactionManager.T1*64, Timeout.Infinite);
                    else
                        _timerJ.Change(0, Timeout.Infinite);
                }
            }
            _transport.Send(response);
        }

        #endregion
    }
}