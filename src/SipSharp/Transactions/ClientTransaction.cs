using System;
using System.Net;
using System.Threading;
using SipSharp.Dialogs;
using SipSharp.Headers;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    internal class ClientTransaction
    {
        private readonly ITransportManager _transportManager;
        private IDialog _dialog;
        private EndPoint _endPoint;
        private IRequest _request;
        private TransactionState _state;
        private Timer _timer1;

        /// <summary>
        /// Controls request retransmissions
        /// </summary>
        private Timer _timerA;

        private int _timerAValue;

        /// <summary>
        /// Controls transaction timeouts
        /// </summary>
        private Timer _timerB;

        /// <summary>
        /// Timer D reflects the amount of time that the server transaction can
        /// remain in the "Completed" state when unreliable transports are used.
        /// </summary>
        /// <remarks>
        /// <para>Should be at least 32 seconds.</para>
        /// <para>This is equal to Timer H in the INVITE server transaction.</para>
        /// </remarks>
        private Timer _timerD;

        private int _timerDValue = 32000;

        /// <summary>
        /// Timeout timer for Non-Invite transactions.
        /// </summary>
        private Timer _timerF;

        /// <summary>
        /// Retransmit timer for Non-Invite transactions.
        /// </summary>
        private Timer _timerE;

        private int _timerEValue = TransactionManager.T1*2;

        /// <summary>
        /// Time in completed state timer
        /// </summary>
        /// <remarks>
        /// The "Completed" state exists to buffer any additional response retransmissions 
        /// that may be received.
        /// </remarks>
        private Timer _timerK;


        public ClientTransaction(ITransportManager transportManager, IMessage message)
        {
            _transportManager = transportManager;
            if (message is IRequest)
            {
                IRequest request = (Request) message;
                if (request.Method == "INVITE")
                    SetupInvite(request);
            }
        }

        public void AddMessage(IMessage message)
        {
            IResponse response = (IResponse) message;
            if (_request.Method == "INVITE")
            {
                ProcessInviteResponse(response);
            }
            else
            {
                ProcessNonInviteResponse(response);
            }
        }

        private void ProcessNonInviteResponse(IResponse response)
        {
            /* If a provisional response is received while in the "Trying" state, the
             * response MUST be passed to the TU, and then the client transaction
             * SHOULD move to the "Proceeding" state.  
             */
            if (StatusCodeHelper.Is1xx(response))
            {
                _state = TransactionState.Proceeding;
            }

                /* If a final response (status codes 200-699) is received while in the "Trying" state,
             * the response MUST be passed to the TU, and the client transaction MUST transition
             * to the "Completed" state
             * 
             * same stuff is stated for proceeding state.
             */
            else
            {
                _state = TransactionState.Completed;

                // Once the client transaction enters the "Completed" state, it MUST set
                // Timer K to fire in T4 seconds for unreliable transports, and zero
                // seconds for reliable transports
                if (_timerK == null)
                    _timerK = new Timer(OnTerminate, null, Timeout.Infinite, Timeout.Infinite);
                _timerK.Change(TransactionManager.T4, Timeout.Infinite);
            }

            ResponseReceived(this, new ResponseEventArgs(response, _endPoint));
        }

        private void ProcessInviteResponse(IResponse response)
        {
            /*
               If the client transaction receives a provisional response while in
               the "Calling" state, it transitions to the "Proceeding" state. In the
               "Proceeding" state, the client transaction SHOULD NOT retransmit the
               request any longer. Furthermore, the provisional response MUST be
               passed to the TU.  Any further provisional responses MUST be passed
               up to the TU while in the "Proceeding" state.                    
            */
            if (_state == TransactionState.Calling && StatusCodeHelper.Is1xx(response))
            {
                _timerA.Change(Timeout.Infinite, Timeout.Infinite);
                _state = TransactionState.Proceeding;
            }
            else if (_state == TransactionState.Calling || _state == TransactionState.Proceeding)
            {
                /* When in either the "Calling" or "Proceeding" states, reception of a
                 * 2xx response MUST cause the client transaction to enter the
                 * "Terminated" state, and the response MUST be passed up to the TU.
                 */
                if (StatusCodeHelper.Is2xx(response))
                {
                    _state = TransactionState.Terminated;
                }

                    /*
                 * When in either the "Calling" or "Proceeding" states, reception of a
                 * response with status code from 300-699 MUST cause the client
                 * transaction to transition to "Completed".  
                 * 
                 * Report msg and send an ACK back.
                 */
                else if (StatusCodeHelper.Is3456xx(response))
                {
                    _state = TransactionState.Completed;
                    SendAck(response);
                    _timerD.Change(_timerDValue, Timeout.Infinite);
                }

                ResponseReceived(this, new ResponseEventArgs(response, _endPoint));
            }
            else if (_state == TransactionState.Proceeding)
            {
                ResponseReceived(this, new ResponseEventArgs(response, _endPoint));
            }
            else if (_state == TransactionState.Completed)
            {
                /*
                 * Any retransmissions of the final response that are received while in
                 * the "Completed" state MUST cause the ACK to be re-passed to the
                 * transport layer for retransmission, but the newly received response
                 * MUST NOT be passed up to the TU
                 */
                SendAck(response);
            }
        }

        private void SendAck(IResponse response)
        {
            _transportManager.Send(_endPoint, CreateAck(response));
        }

        protected virtual IRequest CreateAck(IResponse response)
        {
            /* The ACK request constructed by the client transaction MUST contain
	         * values for the Call-ID, From, and Request-URI that are equal to the
	         * values of those header fields in the request passed to the transport
	         * by the client transaction (call this the "original request").  
             * 
             * The To header field in the ACK MUST equal the To header field in the
	         * response being acknowledged, and therefore will usually differ from
	         * the To header field in the original request by the addition of the
	         * tag parameter.  
             * 
             * The ACK MUST contain a single Via header field, and
	         * this MUST be equal to the top Via header field of the original
	         * request.  
             * 
             * The CSeq header field in the ACK MUST contain the same
	         * value for the sequence number as was present in the original request,
	         * but the method parameter MUST be equal to "ACK".
             * 
             * If the INVITE request whose response is being acknowledged had Route
             * header fields, those header fields MUST appear in the ACK.  This is
             * to ensure that the ACK can be routed properly through any downstream
             * stateless proxies.
             */
            Request ack = new Request("ACK", _request.Uri.ToString(), "SIP/2.0")
                              {
                                  Method = "ACK",
                                  Uri = _request.Uri,
                                  CallId = _request.CallId,
                                  From = _request.From,
                                  To = response.To,
                                  CSeq = new CSeq(_request.CSeq.SeqNr, "ACK")
                              };
            ack.Via.AddToTop(_request.Via.First);
            foreach (var route in response.Route)
                ack.Headers.Add("Route", route.ToString());

            return ack;
        }

        private void OnRetransmission(object state)
        {
            if (_request.Method == "INVITE")
            {
                /* RFC 3261 17.1.1.2.
                    When timer A fires, the client transaction MUST retransmit the request by passing 
                    it to the transport layer, and MUST reset the timer with a value of 2*T1.
                  
                    These retransmissions SHOULD only be done while the client
                    transaction is in the "calling" state.
                */
                if (_state != TransactionState.Calling)
                    return;

                _timerAValue *= 2;
                _timerA.Change(_timerAValue, Timeout.Infinite);
                _transportManager.Send(_endPoint, _request);
            }
            else
            {
                /* If Timer E fires while in the "Proceeding" state, the request MUST be
                   passed to the transport layer for retransmission, and Timer E MUST be
                   reset with a value of T2 seconds.  
                 */
                _timerEValue = Math.Min(_timerEValue*2, TransactionManager.T2);
                _transportManager.Send(_endPoint, _request);
            }
        }

        private void OnTimeout(object state)
        {
            if (_request.Method == "INVITE")
            {
                // If the client transaction is still in the "Calling" state when timer
                // B fires, the client transaction SHOULD inform the TU that a timeout has occurred.
                if (_state == TransactionState.Calling)
                    TimeoutTriggered(this, EventArgs.Empty);
            }
            else
            {
                _state = TransactionState.Terminated;

                /* If Timer F fires while the client transaction is still in the
                 * "Trying" state, the client transaction SHOULD inform the TU about the
                 * timeout, and then it SHOULD enter the "Terminated" state                
                 */
                if (_state == TransactionState.Trying)
                    TimeoutTriggered(this, EventArgs.Empty);
            }
        }

        private void SetupInvite(IRequest request)
        {
            _state = TransactionState.Calling;
            _transportManager.Send(_endPoint, request);
            _request = request;

            // If an unreliable transport is being used, the client transaction MUST 
            // start timer A with a value of T1. If a reliable transport is being used, 
            // the client transaction SHOULD NOT start timer A 
            if (request.Via.First.Protocol == "UDP")
            {
                _timerAValue = TransactionManager.T1;
                _timerA = new Timer(OnRetransmission, null, _timerAValue, Timeout.Infinite);
            }

            // For any transport, the client transaction MUST start timer B with a value of 64*T1 seconds 
            _timerB = new Timer(OnTimeout, null, TransactionManager.T1*64, Timeout.Infinite);

            _timerD = new Timer(OnTerminate, null, _timerDValue, Timeout.Infinite);
        }

        private void Setup(IRequest request)
        {
            _state = TransactionState.Trying;
            _timerF = new Timer(OnTimeout, null, TransactionManager.T1*64, Timeout.Infinite);
            if (request.Via.First.Protocol == "UDP")
                _timerE = new Timer(OnRetransmission, null, _timerEValue, Timeout.Infinite);
            _transportManager.Send(_endPoint, request);
        }

        private void OnTerminate(object state)
        {
            /* If timer D fires while the client transaction is in the "Completed"
             * state, the client transaction MUST move to the terminated state.
             */
            if (_state == TransactionState.Completed)
            {
                _state = TransactionState.Terminated;

            }
        }

        /// <summary>
        /// Determines if this transaction equals a sip message.
        /// </summary>
        /// <param name="obj">Another transaction object, or a <see cref="IResponse"/>.</param>
        /// <returns>True if equal; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IResponse)
            {
                //A response matches a client transaction under two conditions:

                //  1.  If the response has the same value of the branch parameter in
                //      the top Via header field as the branch parameter in the top
                //      Via header field of the request that created the transaction.

                //  2.  If the method parameter in the CSeq header field matches the
                //      method of the request that created the transaction.  The
                //      method is needed since a CANCEL request constitutes a
                //      different transaction, but shares the same value of the branch
                //      parameter.
                IResponse response = (IResponse) obj;
                return response.Via.First.Branch == _request.Via.First.Branch
                       && response.CSeq.Method == _request.CSeq.Method;

            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void TriggerTransportFailed()
        {
            _state = TransactionState.Terminated;
            TransportFailed(this, EventArgs.Empty);
        }
        /// <summary>
        /// Invoked if transportation failed.
        /// </summary>
        public event EventHandler TransportFailed = delegate { };

        /// <summary>
        /// A timeout have occurred.
        /// </summary>
        public event EventHandler TimeoutTriggered = delegate { };

        /// <summary>
        /// A response have been received.
        /// </summary>
        public event EventHandler<ResponseEventArgs> ResponseReceived = delegate { };

        /// <summary>
        /// Transaction have been terminated.
        /// </summary>
        /// <remarks>
        /// Event used to dispose transaction or to re-use it.
        /// </remarks>
        public event EventHandler Terminated = delegate { };
    }
}