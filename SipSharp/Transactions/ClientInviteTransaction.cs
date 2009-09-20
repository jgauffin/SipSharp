using System;
using System.Net;
using System.Threading;
using SipSharp.Headers;
using SipSharp.Messages;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Client transaction for INVITE message
    /// </summary>
    /// <remarks>
    /// <para>A INVITE message is being sent by the client,
    /// track it and make sure that it gets a FINAL response.</para>
    /// </remarks>
    public class ClientInviteTransaction : IClientTransaction
    {
        private readonly IRequest _request;

        /// <summary>
        /// Controls request retransmissions
        /// </summary>
        private readonly Timer _timerA;

        /// <summary>
        /// Controls transaction timeouts
        /// </summary>
        private readonly Timer _timerB;

        /// <summary>
        /// Timer D reflects the amount of time that the server transaction can
        /// remain in the "Completed" state when unreliable transports are used.
        /// </summary>
        /// <remarks>
        /// <para>Should be at least 32 seconds.</para>
        /// <para>This is equal to Timer H in the INVITE server transaction.</para>
        /// </remarks>
        private readonly Timer _timerD;

        private readonly int _timerDValue = 32000;

        private readonly ISipStack _sipStack;
        private int _timerAValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInviteTransaction"/> class.
        /// </summary>
        /// <param name="sipStack">The sip stack.</param>
        /// <param name="request">Request that will be sent immidiately by the transaction.</param>
        public ClientInviteTransaction(ISipStack sipStack, IRequest request)
        {
            _sipStack = sipStack;
            if (request.Method != "INVITE")
                throw new SipSharpException("Can only be used with invite transactions.");

            State = TransactionState.Calling;
            _sipStack.Send(request);
            _request = request;

            // If an unreliable transport is being used, the client transaction MUST 
            // start timer A with a value of T1. If a reliable transport is being used, 
            // the client transaction SHOULD NOT start timer A 
            if (request.Via.First.Protocol == "UDP")
            {
                _timerAValue = TransactionManager.T1;
                _timerA = new Timer(OnRetransmission, null, _timerAValue, Timeout.Infinite);
            }
            else _timerDValue = 0;

            // For any transport, the client transaction MUST start timer B with a value of 64*T1 seconds 
            _timerB = new Timer(OnTimeout, null, TransactionManager.T1*64, Timeout.Infinite);
            _timerD = new Timer(OnTerminate, null, _timerDValue, Timeout.Infinite);
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
            var ack = new Request("ACK", _request.Uri.ToString(), "SIP/2.0")
                          {
                              Method = "ACK",
                              Uri = _request.Uri,
                              CallId = _request.CallId,
                              From = _request.From,
                              To = response.To,
                              CSeq = new CSeq(_request.CSeq.SeqNr, "ACK")
                          };
            ack.Via.AddToTop(_request.Via.First);
            ack.Headers.Add("Route", (IHeader)response.Route.Clone());

            return ack;
        }

        private void OnRetransmission(object state)
        {
            /* RFC 3261 17.1.1.2.
	            When timer A fires, the client transaction MUST retransmit the request by passing 
	            it to the transport layer, and MUST reset the timer with a value of 2*T1.
                      
	            These retransmissions SHOULD only be done while the client
	            transaction is in the "calling" state.
			  
			    The formal definition of retransmit within the context of the transaction layer is to take the message
                previously sent to the transport layer and pass it to the transport
                layer once more.
            */
            if (State != TransactionState.Calling)
                return;

            _timerAValue *= 2;
            _timerA.Change(_timerAValue, Timeout.Infinite);
            _sipStack.Send(_request);
        }

        private void OnTerminate(object state)
        {
            /* If timer D fires while the client transaction is in the "Completed"
			 * state, the client transaction MUST move to the terminated state.
			 */
            if (State == TransactionState.Completed)
            {
                State = TransactionState.Terminated;
                Terminated(this, EventArgs.Empty);
            }
        }

        private void OnTimeout(object state)
        {
            _timerB.Change(Timeout.Infinite, Timeout.Infinite);

            // If the client transaction is still in the "Calling" state when timer
            // B fires, the client transaction SHOULD inform the TU that a timeout has occurred.
            if (State == TransactionState.Calling)
            {
                TimedOut(this, EventArgs.Empty);
                State = TransactionState.Terminated;
            }
        }


        public bool Process(IResponse response, EndPoint endPoint)
        {
            if (State == TransactionState.Terminated)
                return false; // Ignore everything in terminated until we get disposed.

            /*
			   If the client transaction receives a provisional response while in
			   the "Calling" state, it transitions to the "Proceeding" state. In the
			   "Proceeding" state, the client transaction SHOULD NOT retransmit the
			   request any longer. Furthermore, the provisional response MUST be
			   passed to the TU.  Any further provisional responses MUST be passed
			   up to the TU while in the "Proceeding" state.                    
			*/
            if (State == TransactionState.Calling && StatusCodeHelper.Is1xx(response))
            {
                _timerA.Change(Timeout.Infinite, Timeout.Infinite);
                State = TransactionState.Proceeding;
            }
            else if (State == TransactionState.Calling || State == TransactionState.Proceeding)
            {
                bool changeTimerD = false;
                /* When in either the "Calling" or "Proceeding" states, reception of a
				 * 2xx response MUST cause the client transaction to enter the
				 * "Terminated" state, and the response MUST be passed up to the TU.
				 */
                if (StatusCodeHelper.Is2xx(response))
                {
                    State = TransactionState.Terminated;
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
                    State = TransactionState.Completed;
                    SendAck(response);
                    changeTimerD = true;
                }

                ResponseReceived(this, new ResponseEventArgs(response, endPoint));

                //triggering it here instead of in the if clause to make sure
                // that response have been processed successfully first.
                if (changeTimerD)
                    _timerD.Change(_timerDValue, Timeout.Infinite);
            }
            else if (State == TransactionState.Completed)
            {
                /*
				 * Any retransmissions of the final response that are received while in
				 * the "Completed" state MUST cause the ACK to be re-passed to the
				 * transport layer for retransmission, but the newly received response
				 * MUST NOT be passed up to the TU
				 */
                SendAck(response);
            }

            return true;
        }


        private void SendAck(IResponse response)
        {
            _sipStack.Send(CreateAck(response));
        }

        /// <summary>
        /// A timeout have occurred.
        /// </summary>
        public event EventHandler TimedOut = delegate { };

        /// <summary>
        /// A response have been received.
        /// </summary>
        public event EventHandler<ResponseEventArgs> ResponseReceived = delegate { };

        public event EventHandler Terminated = delegate { };

        /// <summary>
        /// We like to reuse transaction objects. Remove all references
        /// to the transaction and reset all parameters.
        /// </summary>
        public void Cleanup()
        {
            throw new NotImplementedException();
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
            get
            {
                string token = _request.Via.First.Branch;
                token += _request.Via.First.SentBy;

                if (_request.Method == SipMethod.ACK)
                    token += SipMethod.INVITE;
                else
                    token += _request.Method;
                return token;
            }
        }

        /// <summary>
        /// Gets current transaction state
        /// </summary>
        public TransactionState State { get; private set; }
    }
}