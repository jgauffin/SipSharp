using System;
using System.Threading;

namespace SipSharp.Transactions
{
    internal class ServerInviteTransaction : IServerTransaction
    {
        private readonly ISipStack _sipStack;

        /// <summary>
        /// Retransmission timer.
        /// </summary>
        private readonly Timer _timerG;

        /// <summary>
        /// Timeout, the amount of time UAC tries to resend the request.
        /// </summary>
        private readonly Timer _timerH;

        /// <summary>
        /// When to switch to terminated state.
        /// </summary>
        private readonly Timer _timerI;

        private IRequest _request;
        private IResponse _response;

        private int _timerGValue;


        public ServerInviteTransaction(ISipStack sipStack, IRequest request)
        {
            _sipStack = sipStack;
            State = TransactionState.Proceeding;
            _request = request;
            _timerG = new Timer(OnRetransmit);
            _timerH = new Timer(OnTimeout);
            _timerI = new Timer(OnTerminated);
            _timerGValue = TransactionManager.T1;

            if (request.Method == "ACK")
                throw new InvalidOperationException("Expected any other type than ACK and INVITE");

            // The server transaction MUST generate a 100
            // (Trying) response unless it knows that the TU will generate a
            // provisional or final response within 200 ms, in which case it MAY
            // generate a 100 (Trying) response
            // Send(request.CreateResponse(StatusCode.Trying, "We are trying here..."));
        }

        private void OnRetransmit(object state)
        {
            _timerGValue = Math.Min(_timerGValue*2, TransactionManager.T2);
            _timerG.Change(_timerGValue, Timeout.Infinite);
            _sipStack.Send(_response);
        }

        private void OnTerminated(object state)
        {
            Terminate();
        }

        private void OnTimeout(object state)
        {
            // If timer H fires while in the "Completed" state, it implies that the
            // ACK was never received.  In this case, the server transaction MUST
            // transition to the "Terminated" state, and MUST indicate to the TU
            // that a transaction failure has occurred.
            Terminate();
            //TODO: Should we have a TimedOut event too?
        }

        private void Terminate()
        {
            State = TransactionState.Terminated;
            Terminated(this, EventArgs.Empty);
            _timerG.Change(Timeout.Infinite, Timeout.Infinite);
            _timerH.Change(Timeout.Infinite, Timeout.Infinite);
            _timerI.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #region IServerTransaction Members

        /// <summary>
        /// The request have been retransmitted by the UA.
        /// </summary>
        /// <param name="request"></param>
        public void Process(IRequest request)
        {
            if (_response == null)
                return; //ops. maybe show an error?
            if (State == TransactionState.Terminated)
                return;

            // If an ACK is received while the server transaction is in the
            // "Completed" state, the server transaction MUST transition to the
            // "Confirmed" state.  As Timer G is ignored in this state, any
            // retransmissions of the response will cease.
            if (request.Method == "ACK")
            {
                if (State == TransactionState.Completed)
                    State = TransactionState.Confirmed;
                _timerG.Change(Timeout.Infinite, Timeout.Infinite);
                _timerI.Change(TransactionManager.T4, Timeout.Infinite);
            }

            // If a request
            // retransmission is received while in the "Proceeding" state, the most
            // recent provisional response that was received from the TU MUST be
            // passed to the transport layer for retransmission.
            if (State == TransactionState.Proceeding)
                _sipStack.Send(_response);

            // Furthermore,
            // while in the "Completed" state, if a request retransmission is
            // received, the server SHOULD pass the response to the transport for
            // retransmission.
            if (State == TransactionState.Completed)
                _sipStack.Send(_response);
        }

        public void Send(IResponse response)
        {
            // Any other final responses passed by the TU to the server
            // transaction MUST be discarded while in the "Completed" state.
            if (State == TransactionState.Completed || State == TransactionState.Terminated)
                return;

            _response = response;

            // While in the "Trying" state, if the TU passes a provisional response
            // to the server transaction, the server transaction MUST enter the
            // "Proceeding" state.  
            if (State == TransactionState.Trying && StatusCodeHelper.Is1xx(response))
                State = TransactionState.Proceeding;

            // The response MUST be passed to the transport
            // layer for transmission.  Any further provisional responses that are
            // received from the TU while in the "Proceeding" state MUST be passed
            // to the transport layer for transmission.  

            // If the TU passes a final response (status
            // codes 200-699) to the server while in the "Proceeding" state, the
            // transaction MUST enter the "Completed" state, and the response MUST
            // be passed to the transport layer for transmission.
            if (State == TransactionState.Proceeding)
            {
                if (StatusCodeHelper.Is2xx(response))
                {
                    _sipStack.Send(response);
                    State = TransactionState.Terminated;
                }
                else if (StatusCodeHelper.Is3456xx(response))
                {
                    _sipStack.Send(response);
                    State = TransactionState.Completed;
                    if (response.Via.First.Protocol == "UDP")
                        _timerG.Change(TransactionManager.T1, Timeout.Infinite);
                    _timerH.Change(64*TransactionManager.T1, Timeout.Infinite);
                }
                if (!StatusCodeHelper.Is1xx(response))
                {
                    State = TransactionState.Completed;
                }
            }

            _response = response;
        }

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

        #endregion

        /// <summary>
        /// Ack was never received from client.
        /// </summary>
        public event EventHandler Terminated = delegate { };
    }
}